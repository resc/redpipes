using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Polly;
using Polly.Registry;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Policies
{
    public static class DynamicPolicy
    {
        static DynamicPolicy()
        {
            var defaultRegistry = new PolicyRegistry();
            _defaultRegistryLocator = () => defaultRegistry;
            _registryLocator = _defaultRegistryLocator;
        }

        private static readonly Func<IPolicyRegistry<string>> _defaultRegistryLocator;
        private static Func<IPolicyRegistry<string>> _registryLocator;

        /// <summary> This function is used to retrieve the <see cref="IPolicyRegistry{TKey}"/>
        /// that is returned from <see cref="DefaultRegistry"/>,
        /// if set to null the default registry locator is used </summary>
        public static Func<IPolicyRegistry<string>> DefaultRegistryLocator
        {
            [NotNull]
            get { return _registryLocator; }
            set { _registryLocator = value ?? _defaultRegistryLocator; }
        }

        [NotNull]
        public static IPolicyRegistry<string> DefaultRegistry
        {
            get { return DefaultRegistryLocator(); }
        }

        /// <summary> Integrates a policy with the given <paramref name="policyKey"/> into the pipeline.
        /// An <see cref="AsyncPolicy"/> is retrieved from the registry using the <see cref="policyKey"/> every time the pipe is executed,
        /// using the registry returned from the <see cref="DefaultRegistry"/> property at the time
        /// <see cref="WithDynamicPolicy{TIn,T}(RedPipes.Configuration.IPipeBuilder{TIn,T},string)"/> is called.
        /// If no policy is available under teh given key, the pipe is executed without one.</summary>
        public static IBuilder<TIn, T> WithDynamicPolicy<TIn, T>(this IBuilder<TIn, T> builder, [NotNull] string policyKey)
        {
            return builder.WithDynamicPolicy(policyKey, DefaultRegistry);
        }

        /// <summary> Integrates a policy with the given <paramref name="policyKey"/> into the pipeline.
        /// An <see cref="AsyncPolicy"/> is retrieved from the <paramref name="policyRegistry"/>
        /// using the <see cref="policyKey"/> every time the pipe is executed.
        /// If no policy is available under the given key, the pipe is executed without one. </summary>
        public static IBuilder<TIn, T> WithDynamicPolicy<TIn, T>(this IBuilder<TIn, T> builder, [NotNull] string policyKey, [NotNull] IReadOnlyPolicyRegistry<string> policyRegistry)
        {
            if (policyKey == null) throw new ArgumentNullException(nameof(policyKey));
            if (policyRegistry == null) throw new ArgumentNullException(nameof(policyRegistry));
            return builder.Use(new Builder<T>(policyKey, policyRegistry));
        }

        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly string _policyKey;
            private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

            public Builder(string policyKey, IReadOnlyPolicyRegistry<string> policyRegistry)
            {
                _policyKey = policyKey;
                _policyRegistry = policyRegistry;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(next, _policyKey, _policyRegistry);
                return Task.FromResult(pipe);
            }


        }

        class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;
            private readonly string _policyKey;
            private readonly IReadOnlyPolicyRegistry<string> _policyRegistry;

            public Pipe(IPipe<T> next, string policyKey, IReadOnlyPolicyRegistry<string> policyRegistry)
            {
                _next = next;
                _policyKey = policyKey;
                _policyRegistry = policyRegistry;
            }

            public async Task Execute(IContext ctx, T value)
            {
                if (_policyRegistry.TryGet(_policyKey, out AsyncPolicy policy))
                    await policy.ExecuteAsync(Execute, new Params(ctx, value, _next));
                else
                    await _next.Execute(ctx, value);
            }

            private static Task Execute(Polly.Context c)
            {
                var ep = Params.Get(c);
                return ep.Next.Execute(ep.Context, ep.Value);
            }

            public class Params : IDictionary<string, object>
            {
                public static Params Get(IReadOnlyDictionary<string, object> context)
                {
                    return (Params)context[nameof(Params)];
                }

                public Params(IContext ctx, T value, IPipe<T> next)
                {
                    Context = ctx;
                    Value = value;
                    Next = next;
                }

                public IContext Context { get; }

                public T Value { get; }

                public IPipe<T> Next { get; }

                #region dictionary implementation which returns 'this' under the key of 'Params'

                // for preventing the garbage collector having to clean up an intermediary dictionary.

                IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
                {
                    yield return new KeyValuePair<string, object>(nameof(Params), this);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable<KeyValuePair<string, object>>)this).GetEnumerator();
                }

                void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
                {
                    array[arrayIndex] = new KeyValuePair<string, object>(nameof(Params), this);
                }

                bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
                {
                    return item.Key == nameof(Params) && Equals(item.Value, this);
                }

                bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
                {
                    throw new NotSupportedException();
                }

                int ICollection<KeyValuePair<string, object>>.Count { get { return 1; } }

                bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get { return true; } }

                bool IDictionary<string, object>.ContainsKey(string key)
                {
                    return key == nameof(Params);
                }

                void IDictionary<string, object>.Add(string key, object value)
                {
                    throw new NotSupportedException();
                }

                void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
                {
                    throw new NotSupportedException();
                }

                void ICollection<KeyValuePair<string, object>>.Clear()
                {
                    throw new NotSupportedException();
                }

                bool IDictionary<string, object>.Remove(string key)
                {
                    throw new NotSupportedException();
                }

                bool IDictionary<string, object>.TryGetValue(string key, out object value)
                {
                    switch (key)
                    {
                        case nameof(Params):
                            value = this;
                            return true;
                        default:
                            value = null;
                            return false;
                    }
                }

                object IDictionary<string, object>.this[string key]
                {
                    get
                    {
                        if (key == nameof(Params))
                            return this;

                        throw new KeyNotFoundException();
                    }
                    set { throw new NotSupportedException(); }
                }

                ICollection<string> IDictionary<string, object>.Keys
                {
                    get { return new string[] { nameof(Params) }; }
                }

                ICollection<object> IDictionary<string, object>.Values
                {
                    get { return new object[] { this }; }
                }

                #endregion
            }
            
            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }
    }
}

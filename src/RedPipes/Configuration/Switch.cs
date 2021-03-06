﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Extension for building a pipe switch router </summary>
    public static class Switch
    {
        /// <summary>  </summary>
        public delegate void AddCase<in TKey, TValue>(TKey key, Func<IBuilder<TValue, TValue>, IBuilder<TValue, TValue>> pipe);

        /// <summary> Declares a pipe switch which routes execution based on a key extracted from the incoming context and data</summary>
        public static IBuilder<TIn, TOut> UseSwitch<TIn, TOut, TKey>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, TKey> selector,
            [NotNull] IReadOnlyDictionary<TKey, IBuilder<TOut, TOut>> cases,
            IBuilder<TOut, TOut>? defaultCase = null,
            IEqualityComparer<TKey>? keyComparer = null,
            bool fallThrough = false,
            string? switchName = null) where TKey : notnull
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (cases == null)
            {
                throw new ArgumentNullException(nameof(cases));
            }

            defaultCase ??= Builder.Unit<TOut>();
            return Builder.Join(builder, new Builder<TOut, TKey>(selector, cases, defaultCase, keyComparer, fallThrough, switchName));
        }

        /// <summary> Declares a pipe switch which routes execution based on a key extracted from the incoming context and data</summary>
        public static IBuilder<TIn, TOut> UseSwitch<TIn, TOut, TKey>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, TKey> selector,
            [NotNull] IEnumerable<(TKey, IBuilder<TOut, TOut>)> cases,
            IBuilder<TOut, TOut>? defaultCase = null,
            IEqualityComparer<TKey>? keyComparer = null,
            bool fallThrough = false,
            string? switchName = null) where TKey : notnull
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (cases == null)
            {
                throw new ArgumentNullException(nameof(cases));
            }

            defaultCase ??= Builder.Unit<TOut>();
            return Builder.Join(builder, new Builder<TOut, TKey>(selector, cases.ToDictionary(x => x.Item1, x => x.Item2), defaultCase, keyComparer, fallThrough, switchName));
        }


        /// <summary> Declares a pipe switch which routes execution based on a key extracted from the incoming context and data</summary>
        public static IBuilder<TIn, TOut> UseSwitch<TIn, TOut, TKey>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, TKey> selector,
            [NotNull] Action<AddCase<TKey, TOut>> addCases,
            IBuilder<TOut, TOut>? defaultCase = null,
            IEqualityComparer<TKey>? keyComparer = null,
            bool fallThrough = false,
            string? switchName = null) where TKey : notnull
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            if (addCases == null)
            {
                throw new ArgumentNullException(nameof(addCases));
            }

            var dict = new ConcurrentDictionary<TKey, IBuilder<TOut, TOut>>();

            void AddCase(TKey key, Func<IBuilder<TOut, TOut>, IBuilder<TOut, TOut>> build)
            {
                dict.TryAdd(key, build(Pipe.Builder<TOut>()));
            }

            addCases(AddCase);

            defaultCase ??= Builder.Unit<TOut>();
            return Builder.Join(builder, new Builder<TOut, TKey>(selector, dict, defaultCase, keyComparer, fallThrough, switchName));
        }

        class Builder<T, TKey> : Builder, IBuilder<T, T> where TKey : notnull
        {
            private readonly Func<IContext, T, TKey> _selector;
            private readonly Dictionary<TKey, IBuilder<T, T>> _cases;
            private readonly IBuilder<T, T> _defaultCase;
            private readonly bool _fallThrough;

            public Builder(Func<IContext, T, TKey> selector,
                IReadOnlyDictionary<TKey, IBuilder<T, T>> cases,
                IBuilder<T, T> defaultCase,
                IEqualityComparer<TKey>? keyComparer,
                bool fallThrough,
                string? switchName = null) : base(switchName)
            {
                _selector = selector;
                keyComparer ??= EqualityComparer<TKey>.Default;
                _cases = new Dictionary<TKey, IBuilder<T, T>>(cases, keyComparer);
                _defaultCase = defaultCase;
                _fallThrough = fallThrough;
            }

            public async Task<IPipe<T>> Build(IPipe<T> next)
            {
                var dict = new ConcurrentDictionary<TKey, IPipe<T>>(_cases.Comparer);
                foreach (var kv in _cases)
                {
                    IPipe<T> pipe;
                    if (_fallThrough)
                        pipe = await kv.Value.Build(next).ConfigureAwait(false);
                    else
                        pipe = await kv.Value.Build().ConfigureAwait(false);
                    dict[kv.Key] = pipe;
                }

                var defaultPipe = await _defaultCase.Build(next).ConfigureAwait(false);
                return new Pipe<TKey, T>(_selector, dict, defaultPipe, Name);
            }

            public override void Accept(IGraphBuilder<IBuilder> visitor)
            {
                base.Accept(visitor);

                var list = new List<IBuilder>(_cases.Count + 1);

                foreach (var kv in _cases.OrderBy(x => x.Key))
                {
                    visitor.AddEdge(this, kv.Value, (Keys.Name, $"Case '{kv.Key}':"));
                    list.Add(kv.Value);
                }

                visitor.AddEdge(this, _defaultCase, (Keys.Name, $"Default:"));
                list.Add(_defaultCase);

                list.ForEach(b => b.Accept(visitor));
            }
        }

        class Pipe<TKey, T> : IPipe<T>
        {
            private readonly Func<IContext, T, TKey> _getKey;
            private readonly IReadOnlyDictionary<TKey, IPipe<T>> _cases;
            private readonly IPipe<T> _defaultCase;
            private readonly string _name;

            public Pipe(Func<IContext, T, TKey> getKey, IReadOnlyDictionary<TKey, IPipe<T>> cases, IPipe<T> defaultCase, string? name = null)
            {
                _getKey = getKey;
                _cases = cases;
                _defaultCase = defaultCase;
                _name = name ?? GetType().GetCSharpName();
            }

            public async Task Execute(IContext ctx, T value)
            {
                var key = _getKey(ctx, value);
                if (_cases.TryGetValue(key, out var selectedCase))
                    await selectedCase.Execute(ctx, value).ConfigureAwait(false);
                else
                    await _defaultCase.Execute(ctx, value).ConfigureAwait(false);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, _name));
                var list = new List<IPipe>(_cases.Count + 1);
                foreach (var kv in _cases.OrderBy(kv => kv.Key))
                {
                    var target = kv.Value;
                    visitor.AddEdge(this, target, (Keys.Name, $"Case '{kv.Key}':"));
                    list.Add(target);
                }

                visitor.AddEdge(this, _defaultCase, (Keys.Name, $"Default:"));
                list.Add(_defaultCase);
                list.ForEach(p => p.Accept(visitor));
            }
        }
    }
}

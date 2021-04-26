using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Pipe data transformation extension </summary>
    public static class Transform
    {
        /// <summary> The synchronous transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate (IContext, TOut) ContextAndValueTransform<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> The async transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate Task<(IContext, TOut)> AsyncContextAndValueTransform<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> Applies a value transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> Value<TIn, T, TOut>(this ITransformBuilder<TIn, T> builder, Func<T, TOut> valueTransform, string? transformName = null)
        {
            return builder.ContextAndValueAsync((ctx, value) => Task.FromResult((ctx, valueTransform(value))), transformName);
        }

        /// <summary> Applies an async value transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> ValueAsync<TIn, T, TOut>(this ITransformBuilder<TIn, T> builder, Func<IContext, T, Task<TOut>> valueTransform, string? transformName = null)
        {
            return builder.ContextAndValueAsync(async (ctx, value) =>
            {
                var newValue = await valueTransform(ctx, value).ConfigureAwait(false);
                return (ctx, newValue);
            }, transformName);
        }

        /// <summary> Applies a context and value transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> ContextAndValue<TIn, T, TOut>(this ITransformBuilder<TIn, T> builder, ContextAndValueTransform<T, TOut> contextAndValueTransform, string? transformName = null)
        {
            return builder.ContextAndValueAsync((ctx, value) => Task.FromResult(contextAndValueTransform(ctx, value)), transformName);
        }

        /// <summary> Applies an async context and value transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> ContextAndValueAsync<TIn, T, TOut>(this ITransformBuilder<TIn, T> builder, AsyncContextAndValueTransform<T, TOut> contextAndValueTransform, string? transformName = null)
        {
            return builder.Builder(new Builder<T, TOut>(contextAndValueTransform, transformName));
        }

        class Builder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
        {
            private readonly AsyncContextAndValueTransform<TIn, TOut> _transform;

            public Builder([NotNull] AsyncContextAndValueTransform<TIn, TOut> transform, string? name) : base(name)
            {
                _transform = transform;
            }

            public Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                IPipe<TIn> pipe = new Pipe<TIn, TOut>(_transform, next, Name);
                return Task.FromResult(pipe);
            }
        }

        class Pipe<TIn, TOut> : IPipe<TIn>
        {
            private readonly AsyncContextAndValueTransform<TIn, TOut> _transform;
            private readonly IPipe<TOut> _next;
            private readonly string _name;

            public Pipe(AsyncContextAndValueTransform<TIn, TOut> transform, IPipe<TOut> next, string name)
            {
                _transform = transform;
                _next = next;
                _name = name;
            }

            public async Task Execute(IContext ctx, TIn value)
            {
                var (ctx1, value1) = await _transform(ctx, value).ConfigureAwait(false);
                await _next.Execute(ctx1, value1).ConfigureAwait(false);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var name = _name ?? $"Transform ({nameof(IContext)}, {typeof(TIn).GetCSharpName()}) => ({nameof(IContext)}, {typeof(TOut).GetCSharpName()})";
                visitor.GetOrAddNode(this, (Keys.Name, name));
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    public static class Transform
    {

        /// <summary> The synchronous transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate (IContext, TOut) Func<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> The async transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate Task<(IContext, TOut)> AsyncFunc<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this ITransformBuilder<TIn, TFrom> transformBuilder, Func<TFrom, TTo> transform)
        {
            return transformBuilder.Use((ctx, value) => Task.FromResult(transform(ctx, value)));
        }

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this ITransformBuilder<TIn, TFrom> transformBuilder, System.Func<TFrom, TTo> transform)
        {
            return transformBuilder.Use((ctx, value) => Task.FromResult((ctx, transform(value))));
        }

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this ITransformBuilder<TIn, TFrom> transformBuilder, AsyncFunc<TFrom, TTo> transform)
        {
            return transformBuilder.Use(new AsyncFuncBuilder<TFrom, TTo>(transform));
        }

        class AsyncFuncBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;

            public AsyncFuncBuilder([NotNull] AsyncFunc<TIn, TOut> transform)
            {
                _transform = transform;
            }

            public Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                IPipe<TIn> pipe = new AsyncFuncPipe<TIn, TOut>(_transform, next);
                return Task.FromResult(pipe);
            }
        }

        class AsyncFuncPipe<TIn, TOut> : IPipe<TIn>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;
            private readonly IPipe<TOut> _next;

            public AsyncFuncPipe(AsyncFunc<TIn, TOut> transform, IPipe<TOut> next)
            {
                _transform = transform;
                _next = next;
            }

            public async Task Execute(IContext ctx, TIn value)
            {
                var (ctx1, value1) = await _transform(ctx, value);
                await _next.Execute(ctx1, value1);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var label = $"Transform ({nameof(IContext)}, {typeof(TIn).GetCSharpName()}) => ({nameof(IContext)}, {typeof(TOut).GetCSharpName()})";
                visitor.GetOrAddNode(this, (NodeLabels.Label, label));
                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }
    }
}

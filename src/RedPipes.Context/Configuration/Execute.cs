using System.Collections.Generic;
using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> provides the execute func extensions </summary>
    public static class Execute
    {
        /// <summary> <see cref="Func{T}"/> corresponds with <see cref="IPipe{T}.Execute"/> method </summary>
        public delegate void Func<in T>(IContext ctx, T value);

        /// <summary> <see cref="AsyncFunc{T}"/> corresponds with <see cref="IPipe{T}.Execute"/> method </summary>
        public delegate Task AsyncFunc<in T>(IContext ctx, T value);

        /// <summary>
        /// Adds the <paramref name="execute"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, Func<TOut> execute)
        {
            return builder.Use(new Builder<TOut>((ctx, value) =>
            {
                execute(ctx, value);
                return Task.CompletedTask;
            }));
        }

        /// <summary>
        /// Adds the <paramref name="execute"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, AsyncFunc<TOut> execute)
        {
            return builder.Use(new Builder<TOut>(execute));
        }

        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly AsyncFunc<T> _execute;

            public Builder(AsyncFunc<T> execute)
            {
                _execute = execute;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                return Task.FromResult((IPipe<T>)new Pipe<T>(_execute, next));
            }

        }

        internal class Pipe<T> : IPipe<T>
        {
            private readonly AsyncFunc<T> _execute;
            private readonly IPipe<T> _next;

            public Pipe(AsyncFunc<T> execute, IPipe<T> next)
            {
                _execute = execute;
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                await _execute(ctx, value);
                await _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (NodeLabels.Label, $"Execute ({nameof(IContext)} ctx, {typeof(T).GetCSharpName()} value) => {{ ... }}"));
                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }
    }
}

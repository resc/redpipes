using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{

    public static class Delegated
    {
        public delegate Task Pipe<T>(IContext ctx, T value, Pipe.ExecuteAsync<T> next);

        /// <summary>
        /// Adds the <paramref name="pipeFunc"/> delegate in the pipeline.
        /// <paramref name="pipeFunc"/> can decide to execute or ignore the next pipe.
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, Pipe<TOut> pipeFunc)
        {
            return builder.Use(next => new FuncPipe<TOut>(pipeFunc, next));
        }

        class FuncPipe<T> : IPipe<T>
        {
            private readonly Pipe<T> _pipe;
            private readonly IPipe<T> _next;

            public FuncPipe(Pipe<T> pipe, IPipe<T> next)
            {
                _pipe = pipe;
                _next = next;
            }

            public Task Execute(IContext ctx, T value)
            {
                return _pipe(ctx, value, _next.Execute);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var label = $"Execute ({nameof(IContext)} ctx, {typeof(T).GetCSharpName()} value, {_pipe.GetType().GetCSharpName()} next) => {{ ... }}";
                visitor.GetOrAddNode(this, (NodeLabels.Label, label));

                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }

    }

    /// <summary> provides the execute func extensions </summary>
    public static class Execute
    {
        /// <summary>
        /// Adds the <paramref name="execute"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder,  Pipe.Execute<TOut> execute)
        {
            return builder.Use(next => new Pipe<TOut>((ctx, value) =>
            {
                execute(ctx, value);
                return Task.CompletedTask;
            }, next));
        }

        /// <summary>
        /// Adds the <paramref name="executeAsyncAsync"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, Pipe.ExecuteAsync<TOut> executeAsyncAsync)
        {
            return builder.Use(next => new Pipe<TOut>(executeAsyncAsync, next));
        }

        internal class Pipe<T> : IPipe<T>
        {
            private readonly Pipe.ExecuteAsync<T> _executeAsync;
            private readonly IPipe<T> _next;

            public Pipe(Pipe.ExecuteAsync<T> executeAsync, IPipe<T> next)
            {
                _executeAsync = executeAsync;
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                await _executeAsync(ctx, value);
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

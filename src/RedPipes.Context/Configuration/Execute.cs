using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> provides the execute func extensions </summary>
    public static class Execute
    {
        /// <summary>
        /// Adds the <paramref name="execute"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, Pipe.Execute<TOut> execute, string name = null)
        {
            return builder.Use(next => new Pipe<TOut>((ctx, value) =>
            {
                execute(ctx, value);
                return Task.CompletedTask;
            }, next, name), name);
        }

        /// <summary>
        /// Adds the <paramref name="executeAsyncAsync"/> delegate in the pipeline
        /// and unconditionally executes the next pipe
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, Pipe.ExecuteAsync<TOut> executeAsyncAsync, string name = null)
        {
            return builder.Use(next => new Pipe<TOut>(executeAsyncAsync, next, name), name);
        }

        internal class Pipe<T> : IPipe<T>
        {
            private readonly Pipe.ExecuteAsync<T> _executeAsync;
            private readonly IPipe<T> _next;
            private readonly string _name;

            public Pipe(Pipe.ExecuteAsync<T> executeAsync, IPipe<T> next, string name)
            {
                _executeAsync = executeAsync;
                _next = next;
                _name = name;
            }

            public async Task Execute(IContext ctx, T value)
            {
                await _executeAsync(ctx, value);
                await _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var name = _name ?? $"Execute ({nameof(IContext)} ctx, {typeof(T).GetCSharpName()} value) => {{ ... }}";
                visitor.GetOrAddNode(this, (Keys.Name, name));
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }

    }
}

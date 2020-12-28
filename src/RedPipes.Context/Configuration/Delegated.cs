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
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, Pipe<TOut> pipeFunc, string? name = null)
        {
            return builder.Use(next => new FuncPipe<TOut>(pipeFunc, next, name), name);
        }

        class FuncPipe<T> : IPipe<T>
        {
            private readonly Pipe<T> _pipe;
            private readonly IPipe<T> _next;
            private readonly string _name;

            public FuncPipe(Pipe<T> pipe, IPipe<T> next, string? name)
            {
                _pipe = pipe;
                _next = next;
                _name = name ?? GenerateName();
            }

            private string GenerateName()
            {
                return $"Execute ({nameof(IContext)} ctx," +
                       $" {typeof(T).GetCSharpName()} value," +
                       $" {_pipe.GetType().GetCSharpName()} next) => {{ ... }}";
            }

            public Task Execute(IContext ctx, T value)
            {
                return _pipe(ctx, value, _next.Execute);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, _name));

                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }

    }
}

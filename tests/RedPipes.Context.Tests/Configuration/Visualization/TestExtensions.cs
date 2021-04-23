using System;
using System.Threading.Tasks;

namespace RedPipes.Configuration.Visualization
{
    static class TestExtensions
    {
        public static IBuilder<TIn, TOut> UseConsolePrinter<TIn, TOut>(this IBuilder<TIn, TOut> builder)
        {
            return builder.Use(next => new Pipe<TOut>(next), $"Print {typeof(TOut).GetCSharpName()} to console");
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;

            public Pipe(IPipe<T> next)
            {
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                await Console.Out.WriteLineAsync($"Print: {value}");
                await _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, $"Print {typeof(T).GetCSharpName()} to console"));
                visitor.AddEdge(this, _next, (Keys.Name, "Next"));
                _next.Accept(visitor);
            }
        }
    }
}

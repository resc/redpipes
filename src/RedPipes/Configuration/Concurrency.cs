using System;
using System.Threading;
using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary>  </summary>
    public static class Concurrency
    {
        /// <summary> The maximum concurrent executions of this pipe beyond this point </summary>
        public static IBuilder<TIn, TOut> UseMaxConcurrency<TIn, TOut>(this IBuilder<TIn, TOut> builder, int count, string? name = null)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Invalid initialCount");
            }

            name ??= $"Semaphore(initialCount:{count})";
            return builder.Use(next => new Pipe<TOut>(name, next, count), name);
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly string _name;
            private readonly IPipe<T> _next;
            private readonly SemaphoreSlim _semaphore;

            public Pipe(string name, IPipe<T> next, int initialCount)
            {
                _name = name;
                _next = next;
                _semaphore = new SemaphoreSlim(initialCount);
            }

            public async Task Execute(IContext ctx, T value)
            {
                await _semaphore.WaitAsync(ctx.Token);
                try
                {
                    await _next.Execute(ctx, value);
                }
                finally
                {
                    _semaphore.Release();
                }
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

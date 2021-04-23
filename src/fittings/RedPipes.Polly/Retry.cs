using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Policies
{
    /// <summary> Pipe retries  </summary>
    public static class Retry
    {
        /// <summary> Adds a retry policy to the pipe </summary>
        public static IBuilder<TIn, T> WithRetries<TIn, T>(this IBuilder<TIn, T> builder, int retryCount)
        {
            return builder.Use(new Builder<T>(retryCount));
        }

        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly int _retryCount;

            public Builder(int retryCount)
            {
                _retryCount = retryCount;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(next, _retryCount);
                return Task.FromResult(pipe);
            }
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;
            private readonly AsyncRetryPolicy _retryPolicy;

            public Pipe(IPipe<T> next, int retryCount)
            {
                _next = next;
                _retryPolicy = Polly.Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

            public async Task Execute(IContext ctx, T value)
            {
                await _retryPolicy.ExecuteAsync(async (c) => await _next.Execute(ctx, value), ctx.Token);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}

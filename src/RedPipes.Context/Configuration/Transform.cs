using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    public static class Transform
    {
        public delegate (IContext, TOut) Func<in TIn, TOut>(IContext ctx, TIn value);

        public delegate Task<(IContext, TOut)> AsyncFunc<in TIn, TOut>(IContext ctx, TIn value);

        public static RedPipes.IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this IBuilder<TIn, TFrom> builder, Func<TFrom, TTo> transform)
        {
            return builder.Use((ctx, value) => Task.FromResult(transform(ctx, value)));
        }

        public static RedPipes.IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this IBuilder<TIn, TFrom> builder, System.Func<TFrom, TTo> transform)
        {
            return builder.Use((ctx, value) => Task.FromResult((ctx, transform(value))));
        }

        public static RedPipes.IBuilder<TIn, TTo> Use<TIn, TFrom, TTo>(this IBuilder<TIn, TFrom> builder, AsyncFunc<TFrom, TTo> transform)
        {
            return builder.Use(new MiddleWare<TFrom, TTo>(transform));
        }

        /// <summary> This is an intermediate step to make the fluent api nicer to use, please do not stop here, but call <see cref="Use{TTo}"/> </summary>
        public interface IBuilder<TIn, TFrom>
        {
            RedPipes.IBuilder<TIn, TTo> Use<TTo>(RedPipes.IBuilder<TFrom, TTo> transform);
        }

        internal class Builder<TIn, TFrom> : IBuilder<TIn, TFrom>
        {
            private readonly RedPipes.IBuilder<TIn, TFrom> _input;

            public Builder([NotNull] RedPipes.IBuilder<TIn, TFrom> input)
            {
                _input = input ?? throw new ArgumentNullException(nameof(input));
            }

            public RedPipes.IBuilder<TIn, TTo> Use<TTo>([NotNull] RedPipes.IBuilder<TFrom, TTo> transform)
            {
                if (transform == null)
                    throw new ArgumentNullException(nameof(transform));

                return new TransformBuilder<TIn, TFrom, TTo>(_input, transform);
            }
        }

        class MiddleWare<TIn, TOut> : Builder, RedPipes.IBuilder<TIn, TOut>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;

            public MiddleWare([NotNull] AsyncFunc<TIn, TOut> transform)
            {
                _transform = transform;
            }

            public Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                IPipe<TIn> pipe = new Pipe<TIn, TOut>(_transform, next);
                return Task.FromResult(pipe);
            }
        }

        class Pipe<TIn, TOut> : IPipe<TIn>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;
            private readonly IPipe<TOut> _next;

            public Pipe(AsyncFunc<TIn, TOut> transform, IPipe<TOut> next)
            {
                _transform = transform;
                _next = next;
            }

            public async Task Execute(IContext ctx, TIn value)
            {
                var (ctx1, value1) = await _transform(ctx, value);
                await _next.Execute(ctx1, value1);
            }

            public IEnumerable<(string, IPipe)> Next()
            {
                yield return (nameof(_next), _next);
            }
        }
    }
}

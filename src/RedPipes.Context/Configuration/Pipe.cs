using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedPipes.Configuration.Nulls;

namespace RedPipes.Configuration
{

    public static class BuilderExtensions
    {
        /// <summary> Constructs the pipe </summary>
        /// <returns>The constructed pipe</returns>
        public static Task<IPipe<TIn>> Build<TIn, TOut>(this IBuilder<TIn, TOut> builder)
        {
            return builder.Build(NullPipe<TOut>.Instance);
        }
    }

    /// <summary> Entry point for the pipe configuration API's </summary>
    public static class Pipe
    {
        /// <summary> Creates a new untyped pipe builder,
        /// useful for creating pipe building extensions
        /// that want to decide the pipe type </summary>
        public static IBuilderProvider Build
        {
            get { return Configuration.BuilderProvider.Instance; }
        }

        /// <summary> The delegate form of <see cref="IPipe{T}"/> </summary>
        public delegate Task Func<T>(IContext ctx, T value, Execute.AsyncFunc<T> next);

        
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> input, IBuilder<TOut, TOut> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new TransformBuilder<TIn, TOut, TOut>(input, builder);
        }

        public static Transform.IBuilder<TIn, TOut> Transform<TIn, TOut>(this IBuilder<TIn, TOut> input)
        {
            return new Transform.Builder<TIn, TOut>(input);
        }

        /// <summary>
        /// Adds the <paramref name="pipe"/> delegate in the pipeline.
        /// <paramref name="pipe"/> can decide to execute or ignore the next pipe.
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, Func<TOut> pipe)
        {
            return builder.Use(new FuncPipeBuilder<TOut>(pipe));
        }
      

        class FuncPipeBuilder<T> : Builder, IBuilder<T, T>
        {
            private readonly Func<T> _pipe;

            public FuncPipeBuilder(Func<T> pipe)
            {
                _pipe = pipe;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                return Task.FromResult((IPipe<T>)new FuncPipe<T>(_pipe, next));
            }
        }

        class FuncPipe<T> : IPipe<T>
        {
            private readonly Func<T> _pipe;
            private readonly IPipe<T> _next;

            public FuncPipe(Func<T> pipe, IPipe<T> next)
            {
                _pipe = pipe;
                _next = next;
            }

            public Task Execute(IContext ctx, T value)
            {
                return _pipe(ctx, value, _next.Execute);
            }

            public IEnumerable<(string, IPipe)> Next()
            {
                yield return (nameof(_next), _next);
            }
        }
    }
}

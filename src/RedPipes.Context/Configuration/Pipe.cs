using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    /// <summary> Entry point for the pipe configuration API's </summary>
    public static class Pipe
    {
        /// <summary>Represents the synchronous variant of the <see cref="IPipe{T}.Execute"/> function. </summary>
        public delegate void Execute<in T>(IContext ctx, T value);

        /// <summary>Represents the <see cref="IPipe{T}.Execute"/> function. </summary>
        public delegate Task ExecuteAsync<in T>(IContext ctx, T value);

        /// <summary> Untyped entry point for building pipes </summary>
        public static IBuilderProvider Builder
        {
            get { return BuilderProvider.Instance; }
        }

        /// <summary> Links <paramref name="input"/> pipe to <paramref name="output"/> pipe</summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> input, IBuilder<TOut, TOut> output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            return new Builder<TIn, TOut, TOut>(input, output);
        }

        /// <summary> Sets up a builder for a transformation <see cref="IPipe{T}"/> </summary>
        public static ITransformBuilder<TIn, TOut> Transform<TIn, TOut>(this IBuilder<TIn, TOut> input)
        {
            return new TransformBuilder<TIn, TOut>(input);
        }

        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, IPipe<TOut>> builderFunc)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return new Builder<TIn, TOut, TOut>(builder, new DelegateBuilder<TOut, TOut>(builderFunc));
        }
        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, Task<IPipe<TOut>>> builderFunc)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return new Builder<TIn, TOut, TOut>(builder, new AsyncDelegateBuilder<TOut, TOut>(builderFunc));
        }
    }
}

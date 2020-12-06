using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

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
        public static IBuilderProvider Build
        {
            get { return BuilderProvider.Instance; }
        }

        public static IBuilder<T, T> For<T>()
        {
            return Build.For<T>();
        }

        public static IPipe<T> End<T>()
        {
            return new Pipe<T>("End");
        }

        public static IBuilder<TIn, TOut> StopProcessing<TIn, TOut>(this IBuilder<TIn, TOut> input)
        {
            return input.Use(next => End<TOut>(), "End");
        }

        /// <summary> Links <paramref name="input"/> pipe to <paramref name="output"/> pipe</summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> input, IBuilder<TOut, TOut> output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            return Builder.Join(input, output);
        }

        /// <summary> Sets up a builder for a transformation <see cref="IPipe{T}"/> </summary>
        public static ITransformBuilder<TIn, TOut> Transform<TIn, TOut>(this IBuilder<TIn, TOut> input, string transformName = null)
        {
            return new TransformBuilder<TIn, TOut>(input, transformName);
        }

        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, IPipe<TOut>> builderFunc, string builderName = null)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return Builder.Join(builder, new DelegateBuilder<TOut, TOut>(builderFunc, builderName));
        }

        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, Task<IPipe<TOut>>> builderFunc,
            string builderName = null)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return Builder.Join(builder, new AsyncDelegateBuilder<TOut, TOut>(builderFunc, builderName));
        }
    }


}

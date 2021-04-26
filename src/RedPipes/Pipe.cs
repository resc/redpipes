using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes
{
    /// <summary> A pipe that doesn't do anything, can be used as a pipe implementation base class </summary>
    public abstract class Pipe<T> : IPipe<T>
    {
        /// <summary> Null pipe exists only to do nothing  </summary>
        public static IPipe<T> Null { get; } = new NullPipe();

        class NullPipe : IPipe<T>
        {
            public Task Execute(IContext ctx, T value) { return Task.CompletedTask; }
            public void Accept(IGraphBuilder<IPipe> visitor) { /* do nothing */ }
        }

        private readonly IPipe? _next;
        private readonly string _name;

        /// <summary> Creates the pipe with the given name </summary>
        public Pipe(string? name = null, IPipe? next = null)
        {
            _next = next;
            _name = name ?? GetType().GetCSharpName();
        }

        /// <summary> executes this pipe's function </summary>
        public abstract Task Execute(IContext ctx, T value);

        /// <summary> Adds a node to the graph for this pipe, override this to also add edges to the output pipe segments </summary>
        public virtual void Accept(IGraphBuilder<IPipe> visitor)
        {
            visitor.GetOrAddNode(this, (Keys.Name, _name));
            if (_next == null) return;

            visitor.AddEdge(this, _next);
            _next.Accept(visitor);
        }
    }

    /// <summary> Pipe execute extensions </summary>
    public static class PipeExecuteExtensions
    {
        /// <summary> Execute the pipe with the Background context </summary>
        public static Task Execute<T>(this IPipe<T> pipe, T value)
        {
            return pipe.Execute(Context.Background, value);
        }
    }

    /// <summary> Entry point for the pipe configuration API's </summary>
    public static class Pipe
    {
        /// <summary> Null pipe exists only to do nothing  </summary>
        public static IPipe Null { get; } = new NullPipe();

        class NullPipe : IPipe
        {
            public void Accept(IGraphBuilder<IPipe> visitor) { /* do nothing */ }
        }

        /// <summary>Represents the synchronous variant of the <see cref="IPipe{T}.Execute"/> function. </summary>
        public delegate void Execute<in T>(IContext ctx, T value);

        /// <summary>Represents the <see cref="IPipe{T}.Execute"/> function. </summary>
        public delegate Task ExecuteAsync<in T>(IContext ctx, T value);

        /// <summary> Untyped entry point for building pipes, use for extensions that want to determine the pipe type themselves </summary>
        public static IBuilder Builder()
        {
            return Configuration.Builder.Unit<object>();
        }

        /// <summary> Typed entry point for building pipes </summary>
        public static IBuilder<T, T> Builder<T>()
        {
            return Configuration.Builder.Unit<T>();
        }

        /// <summary> This pipe does nothing, and doesn't pass on execution to the next pipe </summary>
        public static IPipe<T> Stop<T>()
        {
            return new StopPipe<T>();
        }

        /// <summary> This pipe does nothing, and doesn't pass on execution to the next pipe </summary>
        public static IBuilder<TIn, TOut> StopProcessing<TIn, TOut>(this IBuilder<TIn, TOut> input)
        {
            return input.Use(next => Stop<TOut>(), "Stop");
        }

        /// <summary> Links <paramref name="input"/> pipe to <paramref name="output"/> pipe</summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> input, IBuilder<TOut, TOut> output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            return Configuration.Builder.Join(input, output);
        }

        /// <summary> Sets up a builder for a transformation <see cref="IPipe{T}"/> </summary>
        public static ITransformBuilder<TIn, TOut> UseTransform<TIn, TOut>(this IBuilder<TIn, TOut> input, string? transformName = null)
        {
            return new TransformBuilder<TIn, TOut>(input, transformName);
        }

        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, IPipe<TOut>> builderFunc, string? builderName = null)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return Configuration.Builder.Join(builder, new DelegateBuilder<TOut, TOut>(builderFunc, builderName));
        }

        /// <summary>
        /// Adds the <paramref name="builderFunc"/> delegate in the pipeline. 
        /// </summary>
        public static IBuilder<TIn, TOut> UseAsync<TIn, TOut>(this IBuilder<TIn, TOut> builder,
            Func<IPipe<TOut>, Task<IPipe<TOut>>> builderFunc,
            string? builderName = null)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return Configuration.Builder.Join(builder, new AsyncDelegateBuilder<TOut, TOut>(builderFunc, builderName));
        }

        class StopPipe<T> : Pipe<T>
        {
            public StopPipe() : base("Stop") { }

            public override Task Execute(IContext ctx, T value)
            {
                return Task.CompletedTask;
            }
        }
    }
}

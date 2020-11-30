using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Entry point for the pipe configuration API's </summary>
    public static class Pipe
    {
        /// <summary> Entry point for building pipes </summary>
        public static IBuilderProvider Builder
        {
            get { return Configuration.BuilderProvider.Instance; }
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

        /// <summary> The delegate form of <see cref="IPipe{T}"/> </summary>
        public delegate Task PipeFunc<T>(IContext ctx, T value, Execute.AsyncFunc<T> next);

        /// <summary>
        /// Adds the <paramref name="pipe"/> delegate in the pipeline.
        /// <paramref name="pipe"/> can decide to execute or ignore the next pipe.
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, PipeFunc<TOut> pipe)
        {
            return builder.Use(new FuncPipeBuilder<TOut>(pipe));
        }
        /// <summary>
        /// Adds the <paramref name="pipe"/> delegate in the pipeline.
        /// <paramref name="pipe"/> can decide to execute or ignore the next pipe.
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TOut> builder, [NotNull] Func<IPipe<TOut>, IPipe<TOut>> builderFunc)
        {
            if (builderFunc == null)
            {
                throw new ArgumentNullException(nameof(builderFunc));
            }

            return new Builder<TIn, TOut, TOut>(builder, new DelegateBuilder<TOut, TOut>(builderFunc));
        }


        class DelegateBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
        {
            private readonly Func<IPipe<TOut>, IPipe<TIn>> _builderFunc;

            public DelegateBuilder(Func<IPipe<TOut>, IPipe<TIn>> builderFunc)
            {
                _builderFunc = builderFunc;
            }

            public Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                return Task.FromResult(_builderFunc(next));
            }
        }

        class FuncPipeBuilder<T> : Builder, IBuilder<T, T>
        {
            private readonly PipeFunc<T> _pipe;

            public FuncPipeBuilder(PipeFunc<T> pipe)
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
            private readonly PipeFunc<T> _pipe;
            private readonly IPipe<T> _next;

            public FuncPipe(PipeFunc<T> pipe, IPipe<T> next)
            {
                _pipe = pipe;
                _next = next;
            }

            public Task Execute(IContext ctx, T value)
            {
                return _pipe(ctx, value, _next.Execute);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var label = $"Execute ({nameof(IContext)} ctx, {typeof(T).GetCSharpName()} value, {_pipe.GetType().GetCSharpName()} next) => {{ ... }}";
                visitor.GetOrAddNode(this, (NodeLabels.Label, label));

                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }
    }
}

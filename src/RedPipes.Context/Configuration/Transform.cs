using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    public static class Transform
    {
        /// <summary> The synchronous transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate (IContext, TOut) Func<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> The async transformation function,
        /// transforms the <paramref name="ctx"/> and <paramref name="value"/> for the next pipe stage </summary>
        public delegate Task<(IContext, TOut)> AsyncFunc<in TIn, TOut>(IContext ctx, TIn value);

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> Use<TIn, T, TOut>(this ITransformBuilder<TIn, T> transformBuilder, Func<T, TOut> transform, string? transformName = null)
        {
            return transformBuilder.Use((ctx, value) => Task.FromResult(transform(ctx, value)),transformName);
        }

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> Use<TIn, T, TOut>(this ITransformBuilder<TIn, T> transformBuilder, System.Func<T, TOut> transform, string? transformName = null)
        {
            return transformBuilder.Use((ctx, value) => Task.FromResult((ctx, transform(value))),transformName);
        }

        /// <summary> Applies a transformation function to the pipe </summary>
        public static IBuilder<TIn, TOut> Use<TIn, T, TOut>(this ITransformBuilder<TIn, T> transformBuilder, AsyncFunc<T, TOut> transform, string? transformName = null)
        {
            return transformBuilder.Use(new Builder<T, TOut>(transform,transformName));
        } 

        /// <summary>
        /// Adds the <paramref name="buildTransform"/> delegate in the pipeline.
        /// </summary>
        public static IBuilder<TIn, TOut> Use<TIn, TOut>(this IBuilder<TIn, TIn> builder, [NotNull] System.Func<IPipe<TOut>, IPipe<TIn>> buildTransform, string? transformName = null)
        {
            if (buildTransform == null)
            {
                throw new ArgumentNullException(nameof(buildTransform));
            }

            return Builder.Join(builder, new DelegateBuilder<TIn, TOut>(buildTransform,transformName));
        }

        class Builder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;

            public Builder([NotNull] AsyncFunc<TIn, TOut> transform, string? name):base(name)
            {
                _transform = transform;
            }

            public Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                IPipe<TIn> pipe = new Pipe<TIn, TOut>(_transform, next,Name);
                return Task.FromResult(pipe);
            }
        }

        class Pipe<TIn, TOut> : IPipe<TIn>
        {
            private readonly AsyncFunc<TIn, TOut> _transform;
            private readonly IPipe<TOut> _next;
            private readonly string _name;

            public Pipe(AsyncFunc<TIn, TOut> transform, IPipe<TOut> next, string name)
            {
                _transform = transform;
                _next = next;
                _name = name;
            }

            public async Task Execute(IContext ctx, TIn value)
            {
                var (ctx1, value1) = await _transform(ctx, value);
                await _next.Execute(ctx1, value1);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var name = _name?? $"Transform ({nameof(IContext)}, {typeof(TIn).GetCSharpName()}) => ({nameof(IContext)}, {typeof(TOut).GetCSharpName()})";
                visitor.GetOrAddNode(this, (Keys.Name, name));
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}

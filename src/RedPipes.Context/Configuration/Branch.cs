using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    public static class Branch
    {
        /// <summary> Execute extra steps in the pipe,if the condition is true, and continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] IBuilder<TOut, TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            return builder.UseAsync(async next => new Pipe<TOut>(condition, await trueBranch.Build(next), next));
        }

        /// <summary> Execute extra steps in the pipe,if the condition is true, and continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Delegated.Pipe<TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            return builder.UseAsync(async next =>
            {
                var tb = await Pipe.Builder.For<TOut>().UseAsync(trueBranch).Build(next);
                return new Pipe<TOut>(condition, tb, next);
            });
        }

        /// <summary> Execute alternate pipe, if the condition is true </summary>
        public static IBuilder<TIn, TOut> UseAlternate<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] IBuilder<TOut, TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            return builder.UseAsync(async next => new Pipe<TOut>(condition, await trueBranch.Build(), next));
        }

        /// <summary> Execute alternate pipe, if the condition is true </summary>
        public static IBuilder<TIn, TOut> UseAlternate<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Delegated.Pipe<TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            return builder.UseAsync(async next =>
            {
                var tb = await Pipe.Builder.For<TOut>().UseAsync(trueBranch).Build();
                return new Pipe<TOut>(condition, tb, next);
            });
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly Func<IContext, T, bool> _condition;
            private readonly IPipe<T> _trueBranch;
            private readonly IPipe<T> _falseBranch;

            public Pipe(Func<IContext, T, bool> condition, IPipe<T> trueBranch, IPipe<T> falseBranch)
            {
                _condition = condition;
                _trueBranch = trueBranch;
                _falseBranch = falseBranch;
            }

            public async Task Execute(IContext ctx, T value)
            {
                if (_condition(ctx, value))
                {
                    await _trueBranch.Execute(ctx, value);
                }
                else
                {
                    await _falseBranch.Execute(ctx, value);
                }
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (NodeLabels.Label, "Condition"));
                if (visitor.AddEdge(this, _trueBranch, (EdgeLabels.Label, "True")))
                    _trueBranch.Accept(visitor);

                if (visitor.AddEdge(this, _falseBranch, (EdgeLabels.Label, "False")))
                    _falseBranch.Accept(visitor);
            }
        }
    }
}

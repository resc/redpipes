using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

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

            return builder.Use(new Builder<TOut>(condition, trueBranch, isBranch: true));
        }

        /// <summary> Execute extra steps in the pipe,if the condition is true, and continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Pipe.Func<TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            var trueBranchPipeBuilder = Pipe.Build.For<TOut>().Use(trueBranch);
            return builder.Use(new Builder<TOut>(condition, trueBranchPipeBuilder, isBranch: true));
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

            return builder.Use(new Builder<TOut>(condition, trueBranch, isBranch: false));
        }

        /// <summary> Execute alternate pipe, if the condition is true </summary>
        public static IBuilder<TIn, TOut> UseAlternate<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Pipe.Func<TOut> trueBranch)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            var trueBranchPipeBuilder = Pipe.Build.For<TOut>().Use(trueBranch);
            return builder.Use(new Builder<TOut>(condition, trueBranchPipeBuilder, isBranch: false));
        }


        class Builder<T> :Builder,  IBuilder<T, T>
        {
            private readonly Func<IContext, T, bool> _condition;
            private readonly IBuilder<T, T> _trueBranch;
            private readonly bool _isBranch;

            public Builder(Func<IContext, T, bool> condition, IBuilder<T, T> trueBranch, bool isBranch)
            {
                _condition = condition;
                _trueBranch = trueBranch;
                _isBranch = isBranch;
            }

            public async Task<IPipe<T>> Build(IPipe<T> falseBranch)
            {
                IPipe<T> trueBranch;

                if (_isBranch)
                    trueBranch = await _trueBranch.Build(falseBranch);
                else
                    trueBranch = await _trueBranch.Build();

                IPipe<T> pipe = new Pipe<T>(_condition, trueBranch, falseBranch);
                return pipe;
            }

            public override void Accept(IBuilderVisitor visitor, IBuilder next)
            {
                visitor.Edge(this, next);
                visitor.Branch(this, _trueBranch, _isBranch ? next : null);
            }
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

            public IEnumerable<(string, IPipe)> Next()
            {
                yield return (nameof(_trueBranch), _trueBranch);
                yield return (nameof(_falseBranch), _falseBranch);
            }
        }
    }
}

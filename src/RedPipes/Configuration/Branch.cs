using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Pipe extensions for branching </summary>
    public static class Branch
    {
        /// <summary> Execute steps in the <paramref name="branch"/> if the condition is true, else skip them and continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] IBuilder<TOut, TOut> branch, string? conditionDescription = null)
        {
            return builder.UseBranch(condition, branch, Builder.Unit<TOut>(), conditionDescription);
        }

        /// <summary> Execute extra step in the <paramref name="branch"/> if the condition is true, else skip it and continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Delegated.Pipe<TOut> branch,
            string? conditionDescription = null)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (branch == null)
            {
                throw new ArgumentNullException(nameof(branch));
            }

            var tb = Pipe.Build<TOut>().UseAsync(branch);
            return builder.UseBranch(condition, tb, Builder.Unit<TOut>(), conditionDescription);
        }

        /// <summary> Execute the <paramref name="trueBranch"/> if the condition is true, else execute the <paramref name="falseBranch"/>, after any branch, continue executing the pipe </summary>
        public static IBuilder<TIn, TOut> UseBranch<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] IBuilder<TOut, TOut> trueBranch,
            [NotNull] IBuilder<TOut, TOut> falseBranch,
            string? conditionDescription = null)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (trueBranch == null)
            {
                throw new ArgumentNullException(nameof(trueBranch));
            }

            if (falseBranch == null)
            {
                throw new ArgumentNullException(nameof(falseBranch));
            }

            return Builder.Join(builder, new Builder<TOut>(false, condition, trueBranch, falseBranch, conditionDescription));
        }

        /// <summary> Execute <paramref name="alternate"/> pipe, if the condition is true </summary>
        public static IBuilder<TIn, TOut> UseChoice<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] IBuilder<TOut, TOut> alternate,
            string? conditionDescription = null)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (alternate == null)
            {
                throw new ArgumentNullException(nameof(alternate));
            }

            return Builder.Join(builder, new Builder<TOut>(true, condition, alternate, Builder.Unit<TOut>(), conditionDescription));
        }

        /// <summary> Execute <paramref name="alternate"/> pipe, if the condition is true </summary>
        public static IBuilder<TIn, TOut> UseChoice<TIn, TOut>(
            this IBuilder<TIn, TOut> builder,
            [NotNull] Func<IContext, TOut, bool> condition,
            [NotNull] Delegated.Pipe<TOut> alternate,
            string? conditionDescription = null)
        {
            var tb = Pipe.Build<TOut>().UseAsync(alternate);
            return builder.UseChoice(condition, tb, conditionDescription);
        }

        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly bool _isAlternate;
            private readonly Func<IContext, T, bool> _condition;
            private readonly IBuilder<T, T> _trueBranch;
            private readonly IBuilder<T, T> _falseBranch;

            public Builder(bool isAlternate, Func<IContext, T, bool> condition, IBuilder<T, T> trueBranch, IBuilder<T, T> falseBranch, string? conditionDescription) : base(conditionDescription)
            {
                _isAlternate = isAlternate;
                _condition = condition;
                _trueBranch = trueBranch;
                _falseBranch = falseBranch;
            }

            public async Task<IPipe<T>> Build(IPipe<T> next)
            {
                var truePipe = _isAlternate
                    ? await _trueBranch.Build().ConfigureAwait(false)
                    : await _trueBranch.Build(next).ConfigureAwait(false);
                var falsePipe = await _falseBranch.Build(next).ConfigureAwait(false);
                return new Pipe<T>(_condition, truePipe, falsePipe, Name);
            }

            public override void Accept(IGraphBuilder<IBuilder> visitor)
            {
                var type = (_isAlternate ? "Alternate" : "Branch");
                visitor.GetOrAddNode(this, (Keys.Name, $"{type}: {Name}"));
                visitor.AddEdge(this, _trueBranch, (Keys.Name, "True " + type));
                visitor.AddEdge(this, _falseBranch, (Keys.Name, "False" + type));
                _trueBranch.Accept(visitor);
                _falseBranch.Accept(visitor);
            }
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly Func<IContext, T, bool> _condition;
            private readonly IPipe<T> _trueBranch;
            private readonly IPipe<T> _falseBranch;
            private readonly string _conditionDescription;

            public Pipe(Func<IContext, T, bool> condition, IPipe<T> trueBranch, IPipe<T> falseBranch, string conditionDescription)
            {
                _condition = condition;
                _trueBranch = trueBranch;
                _falseBranch = falseBranch;
                _conditionDescription = conditionDescription ?? $"({nameof(IContext)}, {typeof(T).GetCSharpName()}) => bool";
            }

            public async Task Execute(IContext ctx, T value)
            {
                if (_condition(ctx, value))
                {
                    await _trueBranch.Execute(ctx, value).ConfigureAwait(false);
                }
                else
                {
                    await _falseBranch.Execute(ctx, value).ConfigureAwait(false);
                }
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, _conditionDescription));
                if (visitor.AddEdge(this, _trueBranch, (Keys.Name, "True")))
                    _trueBranch.Accept(visitor);

                if (visitor.AddEdge(this, _falseBranch, (Keys.Name, "False")))
                    _falseBranch.Accept(visitor);
            }
        }
    }
}

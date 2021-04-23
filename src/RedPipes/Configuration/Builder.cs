using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Builder base class,
    /// also implements the builder monad by <see cref="Unit{T}"/> and <see cref="Join{TIn,T,TOut}"/>,
    /// see also <a href="https://en.wikipedia.org/wiki/Monad_(functional_programming)">Wikipedia on the Monad</a> </summary>
    public abstract class Builder : IBuilder
    {
        /// <summary> The name for this builder for use in introspection scenarios </summary>
        protected string Name { get; }

        /// <summary> creates a new builder </summary>
        protected Builder(string? name = null)
        {
            Name = name ?? GetType().GetCSharpName();
        }

        /// <summary>
        /// Override <see cref="Accept"/> so that the debugger tools
        /// can generate a pretty picture of the builder structure.
        /// See <see cref="IGraphBuilder{T}.AddEdge"/> and
        /// <see cref="IGraphBuilder{T}.GetOrAddNode"/> for more information.
        /// </summary> 
        public virtual void Accept(IGraphBuilder<IBuilder> visitor)
        {
            visitor.GetOrAddNode(this, (Keys.Name, Name));
        }

        /// <summary> Unit builder, does nothing </summary>
        public static IBuilder<T, T> Unit<T>()
        {
            return new UnitBuilder<T>();
        }

        /// <summary> Joins two builders into one </summary>
        public static IBuilder<TIn, TOut> Join<TIn, T, TOut>([NotNull] IBuilder<TIn, T> left, [NotNull] IBuilder<T, TOut> right)
        {
            if (left is UnitBuilder<T> && right is IBuilder<TIn, TOut> r)
            {
                return r;
            }

            if (right is UnitBuilder<T> && left is IBuilder<TIn, TOut> l)
            {
                return l;
            }

            return new JoinBuilder<TIn, T, TOut>(left, right);
        }

        /// <summary> Doesn't add anything to the pipeline </summary>
        sealed class UnitBuilder<T> : Builder, IBuilder<T, T>
        {
            public UnitBuilder(): base("<UNIT>")
            {
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                return Task.FromResult(next);
            } 
        }

        /// <summary> Connects two pipes </summary>
        sealed class JoinBuilder<TIn, T, TOut> : Builder, IBuilder<TIn, TOut>
        {
            private readonly IBuilder<TIn, T> _input;
            private readonly IBuilder<T, TOut> _output;

            public JoinBuilder([NotNull] IBuilder<TIn, T> left, [NotNull] IBuilder<T, TOut> right) : base("Join")
            {
                _input = left ?? throw new ArgumentNullException(nameof(left));
                _output = right ?? throw new ArgumentNullException(nameof(right));
            }

            public async Task<IPipe<TIn>> Build(IPipe<TOut> next)
            {
                var output = await _output.Build(next).ConfigureAwait(false);
                return await _input.Build(output).ConfigureAwait(false);
            }

            public override void Accept(IGraphBuilder<IBuilder> visitor)
            {
                base.Accept(visitor);
                visitor.AddEdge(this, _input, (Keys.Name, "Input"));
                visitor.AddEdge(this, _output, (Keys.Name, "Output"));
                _input.Accept(visitor);
                _output.Accept(visitor);
            }
        }
    }
}

using System;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    internal class TransformBuilder<TIn, TFrom> : ITransformBuilder<TIn, TFrom>
    {
        private readonly IBuilder<TIn, TFrom> _input;

        public TransformBuilder([NotNull] IBuilder<TIn, TFrom> input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public IBuilder<TIn, TTo> Use<TTo>([NotNull] IBuilder<TFrom, TTo> transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Builder<TIn, TFrom, TTo>(_input, transform);
        }
    }
}

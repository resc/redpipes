using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    internal class TransformBuilder<TIn, T> : ITransformBuilder<TIn, T>
    {
        private readonly IBuilder<TIn, T> _input;

        public TransformBuilder([NotNull] IBuilder<TIn, T> input)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
        }

        public IBuilder<TIn, TOut> Use<TOut>([NotNull] IBuilder<T, TOut> transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Builder<TIn, T, TOut>(_input, transform);
        }
    }

    internal class DelegateBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
    {
        private readonly Func<IPipe<TOut>, IPipe<TIn>> _build;

        public DelegateBuilder(Func<IPipe<TOut>, IPipe<TIn>> build)
        {
            _build = build;
        }

        public Task<IPipe<TIn>> Build(IPipe<TOut> next)
        {
            return Task.FromResult(_build(next));
        }
    }

    internal class AsyncDelegateBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
    {
        private readonly Func<IPipe<TOut>,Task< IPipe<TIn>>> _buildAsync;

        public AsyncDelegateBuilder(Func<IPipe<TOut>, Task<IPipe<TIn>>> buildAsync)
        {
            _buildAsync = buildAsync;
        }

        public Task<IPipe<TIn>> Build(IPipe<TOut> next)
        {
            return _buildAsync(next);
        }
    }
}

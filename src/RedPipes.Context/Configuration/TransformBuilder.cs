using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    internal class TransformBuilder<TIn, T> : ITransformBuilder<TIn, T>
    {
        private readonly IBuilder<TIn, T> _input;
        private readonly string _builderName;

        public TransformBuilder([NotNull] IBuilder<TIn, T> input, string builderName = null)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _builderName = builderName;
        }

        public IBuilder<TIn, TOut> Use<TOut>([NotNull] IBuilder<T, TOut> transform)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return Builder.Join(_input, transform);
        }
    }

    internal class DelegateBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
    {
        private readonly Func<IPipe<TOut>, IPipe<TIn>> _build;

        public DelegateBuilder([NotNull] Func<IPipe<TOut>, IPipe<TIn>> build, string builderName = null) : base(builderName)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
        }

        public Task<IPipe<TIn>> Build(IPipe<TOut> next)
        {
            return Task.FromResult(_build(next));
        }
    }

    internal class AsyncDelegateBuilder<TIn, TOut> : Builder, IBuilder<TIn, TOut>
    {
        private readonly Func<IPipe<TOut>, Task<IPipe<TIn>>> _buildAsync;

        public AsyncDelegateBuilder([NotNull] Func<IPipe<TOut>, Task<IPipe<TIn>>> buildAsync, string builderName = null) : base(builderName)
        {
            _buildAsync = buildAsync ?? throw new ArgumentNullException(nameof(buildAsync));
        }

        public Task<IPipe<TIn>> Build(IPipe<TOut> next)
        {
            return _buildAsync(next);
        }
    }
}

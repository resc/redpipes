using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Configuration
{
    class TransformBuilder<TIn, T, TOut> : IBuilder<TIn, TOut>
    {
        private readonly IBuilder<TIn, T> _in;
        private readonly IBuilder<T, TOut> _out;

        public TransformBuilder([NotNull] IBuilder<TIn, T> input, [NotNull] IBuilder<T, TOut> output)
        {
            _in = input ?? throw new ArgumentNullException(nameof(input));
            _out = output ?? throw new ArgumentNullException(nameof(output));
        }

        public async Task<IPipe<TIn>> Build(IPipe<TOut> next)
        {
            var output = await _out.Build(next);
            return await _in.Build(output);
        }

        public void Accept(IBuilderVisitor visitor, IBuilder next)
        {
            _in.Accept(visitor, _out);
            _out.Accept(visitor, next);
        }
    }
}

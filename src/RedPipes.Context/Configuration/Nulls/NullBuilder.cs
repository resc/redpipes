using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration.Nulls
{
    /// <summary> Doesn't add anything to the pipeline </summary>
    class NullBuilder<T> : Builder,  IBuilder<T, T>
    {
        public static IBuilder<T, T> Instance { get; } = new NullBuilder<T>();

        private NullBuilder()
        {
        }

        public Task<IPipe<T>> Build(IPipe<T> next)
        {
            return Task.FromResult(next);
        }

        public override void Accept(IGraphBuilder<IBuilder> visitor, IBuilder next)
        {
            next?.Accept(visitor, null);
        }
    }
}

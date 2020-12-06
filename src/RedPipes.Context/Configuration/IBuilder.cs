using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Untyped builder interface </summary>
    public interface IBuilder
    {
        void Accept(IGraphBuilder<IBuilder> visitor);
    }

    /// <summary> Builder interface to build pipes from other pipes. </summary>
    public interface IBuilder<TIn, out TOut> : IBuilder
    {
        Task<IPipe<TIn>> Build(IPipe<TOut> next);
    }
}

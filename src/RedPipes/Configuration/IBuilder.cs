using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration
{
    /// <summary> Untyped builder interface </summary>
    public interface IBuilder
    {
        /// <summary> accepts a <paramref name="visitor"/> to build the graph for the builder structure </summary>
        void Accept(IGraphBuilder<IBuilder> visitor);
    }

    /// <summary> Builder interface to build pipes from other pipes. </summary>
    public interface IBuilder<TIn, out TOut> : IBuilder
    {
        /// <summary> builds a new pipe that uses <paramref name="next"/> as the output pipe </summary>
        Task<IPipe<TIn>> Build(IPipe<TOut> next);
    }
}

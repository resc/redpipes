using System.Collections.Generic;
using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes
{
    /// <summary>Interface to retrieve the structure of a constructed pipe. </summary>
    public interface IPipe
    {
        /// <summary> Accepts a <paramref name="visitor"/> that will be used to build the pipe structure graph </summary>
        void Accept(IGraphBuilder<IPipe> visitor);
    }

    /// <summary> An execution pipe </summary>
    public interface IPipe<in T> : IPipe
    {
        /// <summary> Executes the pipe </summary>
        /// <param name="ctx">The pipe execution context</param>
        /// <param name="value">The value to process</param>
        Task Execute(IContext ctx, T value);
    }
}

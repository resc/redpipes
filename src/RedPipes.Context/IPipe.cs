using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedPipes
{
    /// <summary>Interface to retrieve the structure of a constructed pipe. </summary>
    public interface IPipe
    {
        /// <summary>
        /// Enumerates the next pipes with their names.
        /// When implementing this method, just enumerate the next pipe, not the results of next.Next() too.
        /// </summary>
        IEnumerable<(string name, IPipe next)> Next();
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

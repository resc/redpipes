using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface INode<T> : ILabeled
    {
        T Item { get; }

        ISet<IEdge<T>> OutEdges { get; }
        ISet<IEdge<T>> InEdges { get; }
    }
}
using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface INode : ILabeled
    {
        object Item { get; }

        ISet<IEdge> OutEdges { get; }
        ISet<IEdge> InEdges { get; }
    }
}

using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> a graph node </summary>
    public interface INode : ILabeled
    {
        /// <summary> the item that is represented in the graph by this node. </summary>
        object Item { get; }

        /// <summary> Edges that have this node as its <see cref="IEdge.Source"/> </summary>
        ISet<IEdge> OutEdges { get; }

        /// <summary> Edges that have this node as its <see cref="IEdge.Target"/> </summary>
        ISet<IEdge> InEdges { get; }
    }
}

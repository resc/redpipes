using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface IGraphBuilder<T>
    {
        INode<T> GetOrAddNode(T item);

        bool AddEdge(T source, T target, IDictionary<string, object> labels = null);
    }
}
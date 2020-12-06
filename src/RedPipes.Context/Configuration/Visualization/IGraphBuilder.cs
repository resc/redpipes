using System.Collections.Generic;

namespace RedPipes.Configuration.Visualization
{
    public interface IGraphBuilder<in T>
    {
        /// <summary> Gets or adds a graph node associated with the item,
        /// you can use the <see cref="ILabeled.Labels"/> on the node to set extra info </summary>
        /// <param name="item"></param>
        INode GetOrAddNode(T item);
        
        /// <summary> Adds a directed edge from <paramref name="source"/>
        /// to <paramref name="target"/>, and adds the supplied labels to the edge.
        /// You can add only 1 edge between any source and target.
        /// <see cref="AddEdge"/> returns false if the edge already existed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="labels">labels for the edge, see <see cref="Keys"/> for standard label keys</param>
        bool AddEdge(T source, T target, IDictionary<string, object> labels = null);
    }
}

namespace RedPipes.Configuration.Visualization
{
    /// <summary> the graph edge interface </summary>
    public interface IEdge : ILabeled
    {
        /// <summary> the start of this edge </summary>
        INode Source { get; }

        /// <summary> the end of this edge </summary>
        INode Target { get; }
    }
}

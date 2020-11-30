namespace RedPipes.Configuration.Visualization
{
    public interface IEdge<T> : ILabeled
    {
        INode<T> Source { get; }
        INode<T> Target { get; }

        void Remove();
    }
}
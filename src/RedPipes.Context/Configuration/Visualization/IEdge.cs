namespace RedPipes.Configuration.Visualization
{
    public interface IEdge : ILabeled
    {
        INode Source { get; }
        INode Target { get; }

        void Remove();
    }
}

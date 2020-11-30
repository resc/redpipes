using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Configuration.Nulls
{
    /// <summary> A pipe that doesn't do anything </summary>
    class NullPipe<T> : IPipe<T>
    {
        public NullPipe() { }

        public Task Execute(IContext ctx, T value)
        {
            return Task.CompletedTask;
        }
        
        public void Accept(IGraphBuilder<IPipe> visitor)
        {
            visitor.GetOrAddNode(this, (NodeLabels.Label, "End"));
        }
    }
}

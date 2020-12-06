using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes
{
    /// <summary> A pipe that doesn't do anything </summary>
    public class Pipe<T> : IPipe<T>
    {
        private readonly string _name;
        
        public Pipe(string name)
        {
            _name = name ?? GetType().GetCSharpName();
        }

        public virtual Task Execute(IContext ctx, T value)
        {
            return Task.CompletedTask;
        }

        public virtual void Accept(IGraphBuilder<IPipe> visitor)
        {
            visitor.GetOrAddNode(this, (Keys.Name, _name));
        }
    }
}

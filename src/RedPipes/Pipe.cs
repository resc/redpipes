using System.Threading.Tasks;
using RedPipes.Configuration.Visualization;

namespace RedPipes
{
    /// <summary> A pipe that doesn't do anything, can be used as a pipe implementation base class </summary>
    public abstract class Pipe<T> : IPipe<T>
    {
        private readonly IPipe? _next;
        private readonly string _name;

        /// <summary> Creates the pipe with the given name </summary>
        public Pipe(string? name = null, IPipe? next = null)
        {
            _next = next;
            _name = name ?? GetType().GetCSharpName();
        }

        /// <summary> executes this pipe's function </summary>
        public abstract Task Execute(IContext ctx, T value);

        /// <summary> Adds a node to the graph for this pipe, override this to also add edges to the output pipe segments </summary>
        public virtual void Accept(IGraphBuilder<IPipe> visitor)
        {
            visitor.GetOrAddNode(this, (Keys.Name, _name));
            if (_next == null) return;

            visitor.AddEdge(this, _next);
            _next.Accept(visitor);
        }
    }
}

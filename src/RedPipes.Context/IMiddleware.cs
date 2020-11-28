using System.Threading.Tasks;

namespace RedPipes
{
    public interface IBuilderVisitor
    {
        void Edge(IBuilder source, IBuilder target);
        void Branch(IBuilder source, IBuilder branch, IBuilder target);
    }


    /// <summary> Marker interface </summary>
    public interface IBuilder
    {
        void Accept(IBuilderVisitor visitor, IBuilder next);
    }

    public abstract class Builder : IBuilder
    {
        public virtual void Accept(IBuilderVisitor visitor, IBuilder next)
        {
            visitor.Edge(this, next);
        }
    }

    /// <summary> Middleware implementation to build the pipeline. </summary>
    public interface IBuilder<TIn, out TOut> : IBuilder
    {
        Task<IPipe<TIn>> Build(IPipe<TOut> next);
    }
}

using System.Threading.Tasks;

namespace RedPipes.Configuration.Nulls
{
    /// <summary> Doesn't add anything to the pipeline </summary>
    class NullBuilder<T> : Builder,  IBuilder<T, T>
    {
        public static IBuilder<T, T> Instance { get; } = new NullBuilder<T>();

        private NullBuilder()
        {
        }

        public Task<IPipe<T>> Build(IPipe<T> next)
        {
            return Task.FromResult(next);
        } 
    }
}

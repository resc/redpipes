using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedPipes.Configuration.Nulls
{
    /// <summary> A pipe that doesn't do anything </summary>
    class NullPipe<T> : IPipe<T>
    {
        public static IPipe<T> Instance { get; } = new NullPipe<T>();

        private NullPipe() { }

        public Task Execute(IContext ctx, T value)
        {
            return Task.CompletedTask;
        }

        public IEnumerable<(string, IPipe)> Next()
        {
            yield break;
        }
    }
}

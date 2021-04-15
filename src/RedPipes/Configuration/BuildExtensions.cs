using System.Threading.Tasks;

namespace RedPipes.Configuration
{
    /// <summary> Pipe construction extensions </summary>
    public static class BuildExtensions
    {
        /// <summary> Constructs the pipe </summary>
        /// <returns>The constructed pipe</returns>
        public static Task<IPipe<TIn>> Build<TIn, TOut>(this IBuilder<TIn, TOut> builder)
        {
            return builder.Build(Pipe.Stop<TOut>());
        }
    }
}

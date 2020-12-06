using System.Threading.Tasks;
using RedPipes.Configuration.Nulls;

namespace RedPipes.Configuration
{
    public static class BuildExtensions
    {
        /// <summary> Constructs the pipe </summary>
        /// <returns>The constructed pipe</returns>
        public static Task<IPipe<TIn>> Build<TIn, TOut>(this IBuilder<TIn, TOut> builder)
        {
            return builder.Build(Pipe.End<TOut>());
        }
    }
}

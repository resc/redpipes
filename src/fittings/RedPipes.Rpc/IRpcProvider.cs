using System.Threading.Tasks;

namespace RedPipes.Patterns.Rpc
{
    /// <summary>  </summary>
    public interface IRpcProvider
    {
        /// <summary>  </summary>
        Task<(IContext, TResponse)> Call<TRequest, TResponse>(IContext ctx, TRequest request, RpcOptions options);
    }
}

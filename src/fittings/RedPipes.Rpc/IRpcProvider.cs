using System.Threading.Tasks;

namespace RedPipes.Patterns.Rpc
{
    public interface IRpcProvider
    {
        Task<(IContext, TResponse)> Call<TRequest, TResponse>(IContext ctx, TRequest request, RpcOptions options);
    }
}

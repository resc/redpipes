using System;
using RedPipes.Configuration;

namespace RedPipes.Patterns.Rpc
{
    public interface IOnRpcResponse<TIn, TRequest>
    {
        IOnRpcError<TIn, TResponse> OnRpcResponse<TResponse>(Func<IBuilder<TResponse, TResponse>, IBuilder<TResponse, TResponse>> onResponse);
    }
}

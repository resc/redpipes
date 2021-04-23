using System;
using RedPipes.Configuration;

namespace RedPipes.Patterns.Rpc
{
    /// <summary>  </summary>
    public interface IOnRpcResponse<TIn, TRequest>
    {
        /// <summary>  </summary>
        IOnRpcError<TIn, TResponse> OnRpcResponse<TResponse>(Func<IBuilder<TResponse, TResponse>, IBuilder<TResponse, TResponse>> onResponse);
    }
}

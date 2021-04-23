using System;
using RedPipes.Configuration;

namespace RedPipes.Patterns.Rpc
{
    /// <summary>  </summary>
    public interface IOnRpcError<TIn, out TResponse>
    {
        /// <summary>  </summary>
        IBuilder<TIn, TResponse> OnRpcError<TError>(Func<IBuilder<TError, TError>, IBuilder<TError,TError>> onError) where TError : Exception;
    }
}

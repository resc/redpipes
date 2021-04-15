using System;
using RedPipes.Configuration;

namespace RedPipes.Patterns.Rpc
{
    public interface IOnRpcError<TIn, TResponse>
    {
        IBuilder<TIn, TResponse> OnRpcError<TError>(Func<IBuilder<TError, TError>, IBuilder<TError,TError>> onError) where TError : Exception;
    }
}

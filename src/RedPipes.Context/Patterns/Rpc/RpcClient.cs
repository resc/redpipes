using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration;
using RedPipes.Configuration.Nulls;

namespace RedPipes.Patterns.Rpc
{
    public class RpcClient
    {
        internal IRpcProvider Provider { get; }

        internal RpcOptions Options { get; }

        public RpcClient(IRpcProvider provider, RpcOptions options)
        {
            Provider = provider;
            Options = options;
        }

        public string Name
        {
            get { return Options.EndPoint; }
        }

        public async Task<TResponse> Call<TRequest, TResponse>(IContext ctx, IRpc<TRequest, TResponse> rpc) where TRequest : IRpc<TRequest, TResponse>
        {
            return await Call<TRequest, TResponse>(ctx, (TRequest)rpc);
        }

        public async Task<TResponse> Call<TRequest, TResponse>(IContext ctx, TRequest request)
        {
            var (_, response) = await Provider.Call<TRequest, TResponse>(ctx, request, Options);
            return response;
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, IRpc<TRequest, TResponse> rpc, Execute.AsyncFunc<TResponse> onResponse, Execute.AsyncFunc<Exception> onError) where TRequest : IRpc<TRequest, TResponse>
        {
            await Call(ctx, (TRequest)rpc, onResponse, onError);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, IRpc<TRequest, TResponse> rpc, IPipe<TResponse> onResponse, IPipe<Exception> onError) where TRequest : IRpc<TRequest, TResponse>
        {
            await Call(ctx, (TRequest)rpc, onResponse, onError);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, TRequest request, [NotNull] Execute.AsyncFunc<TResponse> onResponse, Execute.AsyncFunc<Exception> onError = null)
        {
            if (onResponse == null) throw new ArgumentNullException(nameof(onResponse));

            IPipe<TResponse> requestPipe = new Execute.Pipe<TResponse>(onResponse, NullPipe<TResponse>.Instance);
            IPipe<Exception> errorPipe = new Execute.Pipe<Exception>(onError, NullPipe<Exception>.Instance);
            await Call(ctx, request, requestPipe, errorPipe);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, TRequest request, IPipe<TResponse> onResponse, IPipe<Exception> onError = null)
        {
            if (onResponse == null) throw new ArgumentNullException(nameof(onResponse));

            var pipe = new RpcExtensions.Pipe<TRequest, TResponse, Exception>(Provider, Options, onResponse, onError);
            await pipe.Execute(ctx, request);
        }
    }
}

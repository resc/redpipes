using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration;

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

        public async Task<TResponse> Call<TRequest, TResponse>(IContext ctx, IRpcRequest<TRequest, TResponse> rpc) where TRequest : IRpcRequest<TRequest, TResponse>
        {
            return await Call<TRequest, TResponse>(ctx, (TRequest)rpc);
        }

        public async Task<TResponse> Call<TRequest, TResponse>(IContext ctx, TRequest request)
        {
            var (_, response) = await Provider.Call<TRequest, TResponse>(ctx, request, Options);
            return response;
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, IRpcRequest<TRequest, TResponse> request, [NotNull] Pipe.ExecuteAsync<TResponse> onResponse, Pipe.ExecuteAsync<Exception>? onError = null) where TRequest : IRpcRequest<TRequest, TResponse>
        {
            var responseBuilder = Pipe.Build<TResponse>().UseAsync(onResponse);
            var errorBuilder = onError == null ? null : Pipe.Build<Exception>().UseAsync(onError);
            await Call(ctx, request, responseBuilder, errorBuilder);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, TRequest request, [NotNull] Pipe.ExecuteAsync<TResponse> onResponse, Pipe.ExecuteAsync<Exception>? onError = null)
        {
            var responseBuilder = Pipe.Build<TResponse>().UseAsync(onResponse);
            var errorBuilder = onError == null ? null : Pipe.Build<Exception>().UseAsync(onError);
            await Call(ctx, request, responseBuilder, errorBuilder);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, TRequest request, [NotNull] IBuilder<TResponse, TResponse> onResponse, IBuilder<Exception, Exception>? onError = null)
        {
            if (onResponse == null)
                throw new ArgumentNullException(nameof(onResponse));

            var responsePipe = await onResponse.Build();
            var errorPipe = onError == null ? Pipe.Stop<Exception>() : await onError.Build();
            await Call(ctx, request, responsePipe, errorPipe);
        }

        public async Task Call<TRequest, TResponse>(IContext ctx, TRequest request, IPipe<TResponse> onResponse, IPipe<Exception>? onError = null)
        {
            if (onResponse == null) throw new ArgumentNullException(nameof(onResponse));

            var pipe = new RpcExtensions.Pipe<TRequest, TResponse, Exception>(Provider, Options, onResponse, onError);
            await pipe.Execute(ctx, request);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Rpc
{
    public static class RpcExtensions
    {
        private static readonly object _rpcProviderKey = Context.NewKey("RpcProvider");

        public static IOnRpcResponse<TIn, TRequest> WithRpcProvider<TIn, TRequest>(this IBuilder<TIn, TRequest> builder, IRpcProvider provider, RpcOptions options = null)
        {
            return new RpcPipeBuilder<TIn, TRequest, Exception>(builder, provider, options);
        }

        public static IContext WithRpcProvider(this IContext ctx, [NotNull] IRpcProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return ctx.With(_rpcProviderKey, provider);
        }

        public static IRpcProvider GetRpcProvider(this IContext ctx)
        {
            return ctx.TryGetValue(_rpcProviderKey, out IRpcProvider provider)
                ? provider
                : throw new NotSupportedException("No rpc provider available");
        }

        public static RpcClient GetRpcClient(this IContext ctx, string endpoint)
        {
            var opts = RpcOptions.Default.Clone();
            opts.EndPoint = endpoint;
            var provider = ctx.GetRpcProvider();
            return new RpcClient(provider, opts);
        }

        public static RpcClient With([NotNull] this RpcClient client, [NotNull] Action<RpcOptions> setOptions)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (setOptions == null)
            {
                throw new ArgumentNullException(nameof(setOptions));
            }

            var opts = client.Options.Clone();
            setOptions(opts);
            return new RpcClient(client.Provider, opts);
        }


        class RpcPipeBuilder<TIn, TRequest, TResponse> : IOnRpcResponse<TIn, TRequest>, IOnRpcError<TIn, TResponse>
        {
            private readonly IBuilder<TIn, TRequest> _builder;
            private readonly IRpcProvider _provider;
            private readonly RpcOptions _options;
            private readonly IBuilder<TResponse, TResponse> _responsePipeBuilder;

            public RpcPipeBuilder(IBuilder<TIn, TRequest> builder, IRpcProvider provider, RpcOptions options = null,
                IBuilder<TResponse, TResponse> responsePipeBuilder = null)
            {
                _builder = builder;
                _provider = provider;
                _options = options;
                _responsePipeBuilder = responsePipeBuilder;
            }

            public IBuilder<TIn, TResponse> OnRpcError<TError>(Func<IBuilder<TError, TError>, IBuilder<TError, TError>> onErrorPipeBuilder) where TError : Exception
            {
                var errorPipeBuilder = onErrorPipeBuilder(Pipe.Builder.For<TError>());
                return _builder.Transform().Use(new Builder<TRequest, TResponse, TError>(_provider, _options.Clone(), _responsePipeBuilder, errorPipeBuilder));
            }

            IOnRpcError<TIn, TR> IOnRpcResponse<TIn, TRequest>.OnRpcResponse<TR>(Func<IBuilder<TR, TR>, IBuilder<TR, TR>> onResponse)
            {
                var responsePipeBuilder = onResponse(Pipe.Builder.For<TR>());
                return new RpcPipeBuilder<TIn, TRequest, TR>(_builder, _provider, _options, responsePipeBuilder);
            }
        }

        class Builder<TRequest, TResponse, TException> : Builder, IBuilder<TRequest, TResponse> where TException : Exception
        {
            private readonly IRpcProvider _rpc;
            private readonly RpcOptions _options;
            private readonly IBuilder<TResponse, TResponse> _onResponse;
            private readonly IBuilder<TException, TException> _onException;

            public Builder(IRpcProvider rpc, RpcOptions options, IBuilder<TResponse, TResponse> onResponse, IBuilder<TException, TException> onException)
            {
                _rpc = rpc;
                _options = options;
                _onResponse = onResponse;
                _onException = onException;
            }

            public async Task<IPipe<TRequest>> Build(IPipe<TResponse> next)
            {
                var onResponse = await _onResponse.Build(next);
                var onException = await _onException.Build();
                var pipe = new Pipe<TRequest, TResponse, TException>(_rpc, _options.Clone(), onResponse, onException);
                return pipe;
            }

            public override void Accept(IGraphBuilder<IBuilder> visitor, IBuilder next)
            {
                visitor.AddEdge(this, _onResponse, (EdgeLabels.Label, "onResponse"));
                visitor.AddEdge(this, _onException, (EdgeLabels.Label, "onException"));
                _onResponse.Accept(visitor, next);
                _onException.Accept(visitor, null);
            }
        }

        internal class Pipe<TRequest, TResponse, TException> : IPipe<TRequest> where TException : Exception
        {
            private readonly IRpcProvider _rpc;
            private readonly RpcOptions _options;
            private readonly IPipe<TResponse> _onResponse;
            [CanBeNull]
            private readonly IPipe<TException> _onException;

            public Pipe([NotNull] IRpcProvider rpc, [NotNull] RpcOptions options, [NotNull] IPipe<TResponse> onResponse, IPipe<TException> onException = null)
            {
                _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
                _options = options ?? throw new ArgumentNullException(nameof(options));
                _onResponse = onResponse ?? throw new ArgumentNullException(nameof(onResponse));
                _onException = onException;
            }

            public async Task Execute(IContext ctx, TRequest request)
            {
                IContext responseCtx;
                TResponse response;
                try
                {
                    (responseCtx, response) = await _rpc.Call<TRequest, TResponse>(ctx, request, _options);
                }
                catch (TException ex)
                {
                    if (_onException == null)
                    {
                        throw;
                    }

                    await _onException.Execute(ctx, ex);
                    return;
                }

                // don't call the response pipe inside the try/catch,
                // onException is only fo handling rpc exceptions,
                // and should not be used to process
                // exceptions thrown while handling the response
                await _onResponse.Execute(responseCtx, response);
            }


            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                var label = $"Remote Procedure Call ({typeof(TRequest).GetCSharpName()} request) => ({typeof(TResponse).GetCSharpName()} response)\nendpoint: {_options.EndPoint}";
                visitor.GetOrAddNode(this, (NodeLabels.Label, label));
                if (visitor.AddEdge(this, _onResponse, (EdgeLabels.Label, "onResponse")))
                    _onResponse.Accept(visitor);

                if (visitor.AddEdge(this, _onException, (EdgeLabels.Label, "onException")))
                    _onException.Accept(visitor);
            }
        }
    }
}

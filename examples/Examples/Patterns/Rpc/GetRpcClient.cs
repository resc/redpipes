using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Rpc
{
    [Description("Some RpcClient api usage examples, use faultyservice as a first argument to watch the sparks fly")]
    public class GetRpcClient : IPipe<string[]>, IRpcProvider
    {
        private const int ResponseDelay = 1000;

        public async Task Execute(IContext ctx, string[] args)
        {
            var endpoint = args.FirstOrDefault() ?? "worker";
            args = args.Skip(1).ToArray();

            var client = ctx
                // register an RPC provider in the context
                // in real use cases this would be done further upstream in the rpc pipe
                // maybe based on tenant, incoming message transport or other information 
                .WithRpcProvider(this)
                // get rpc client for the service named svcName,
                // the rpc provider can differentiate
                // rpc calls based on the name
                .GetRpcClient(endpoint);

            // request type is annotated with marker interface, no generic declaration needed on the Call<TResponse>(...) invocation
            var resp = await client
                .Call(ctx, new ListRequest(args));
            await Console.Out.WriteLineAsync("Response received: " + resp);

            try
            {
                // timeouts can be set on the context, to prevent slow calls blocking the pipe for too long
                await client
                    .Call(ctx.WithTimeout(ResponseDelay / 2), new ListRequest(args));
                await Console.Out.WriteLineAsync("Unexpected response received: " + resp);
            }
            catch (TaskCanceledException ex)
            {
                await Console.Out.WriteLineAsync("Request timed out: " + ex);
            }

            // without marker interface, type inference doesn't work so we need to specify rpc and response types manually.
            resp = await client
                 .Call<ListRequestNoMarkerInterface, ListResponse>(ctx, new ListRequestNoMarkerInterface(args));
            await Console.Out.WriteLineAsync("Response received: " + resp);

            // per call custom rpc options
            resp = await client
                 .With(opts => opts.Timeout = TimeSpan.FromSeconds(10))
                 .Call(ctx, new ListRequest(args));
            await Console.Out.WriteLineAsync("Response received: " + resp);

            // callback style, Call(...) completes after the rpc has been sent on the wire,
            // but possibly before the response has returned
            // request type is annotated with marker interface, no generic declaration needed on the Call<TRequest,TResponse>(ctx,request,onResponse,onError) invocation
            await client.Call(ctx, new ListRequest(args), async (responseCtx, response) =>
            {
                // be careful to not use ctx here, or any other captures.
                // it's highly probable that will cause gnashing of teeth,
                // wailing of despair and face-palming by your future self.
                await Console.Out.WriteLineAsync("Response received: " + response);

            }, async (requestCtx, exception) =>
            {
                await Console.Out.WriteLineAsync("Request failed: " + exception);
            });
            // callback style, Call(...) completes after the rpc has been sent on the wire,
            // but possibly before the response has returned.
            // Request type is NOT annotated with marker interface,
            // so the generic declaration is needed on the Call<TRequest,TResponse>(ctx,request,onResponse,onError) invocation
            await client.Call<ListRequestNoMarkerInterface, ListResponse>(ctx, new ListRequestNoMarkerInterface(args), async (responseCtx, response) =>
             {
                 // be careful to not use ctx here, or any other captures.
                 // it's highly probable that will cause gnashing of teeth,
                 // wailing of despair and face-palming by your future self.
                 await Console.Out.WriteLineAsync("Response received: " + response);

             }, async (requestCtx, exception) =>
             {
                 await Console.Out.WriteLineAsync("Request failed: " + exception);
             });

            // build response and exception pipes
            var responsePipe = await GetResponsePipe();
            var exceptionPipe = await GetExceptionPipe();

            // response pipe style, Call(...) completes after the rpc has been sent on the wire, but possibly before the response has returned
            await client.Call(ctx, new ListRequest(args), responsePipe, exceptionPipe);
        }

        private static Task<IPipe<Exception>> GetExceptionPipe()
        {
            return Pipe.Builder<Exception>()
                .UseTransform().Value(exception => exception.ToString())
                .UseAsync(async (responseCtx, exception) =>
                {
                    await Console.Out.WriteLineAsync("Exception thrown: " + exception);
                }).Build();
        }

        private static async Task<IPipe<ListResponse>> GetResponsePipe()
        {
            return await Pipe.Builder<ListResponse>()
                    .UseTransform().Value(response => response.ToString())
                    .UseAsync(async (responseCtx, response) =>
                    {
                        await Console.Out.WriteLineAsync("Response received: " + response);
                    }).Build();
        }

        public async Task<(IContext, TResponse)> Call<TRequest, TResponse>(IContext ctx, TRequest request, RpcOptions options)
        {
            await Console.Out.WriteLineAsync($"Simulate calling service '{options.EndPoint}' with timeout {options.Timeout:g} and request {request}...");
            await Task.Delay(ResponseDelay, ctx.Token);
            if (StringComparer.OrdinalIgnoreCase.Equals(options.EndPoint, "faultyservice"))
            {
                throw new InvalidOperationException($"{options.EndPoint} threw an exception!");
            }
            await Console.Out.WriteLineAsync($"Simulate receiving response {typeof(TResponse).Name} from service '{options.EndPoint}'...");
            var response = Activator.CreateInstance<TResponse>();
            if (response is ListResponse lr && request is IEnumerable<string> strings)
            {
                lr.List = strings.ToArray();
            }
            return (ctx, response);
        }


        public class ListRequestNoMarkerInterface : IEnumerable<string>
        {
            public string[] Args { get; }

            public ListRequestNoMarkerInterface()
            {
                Args = Array.Empty<string>();
            }

            public ListRequestNoMarkerInterface(string[] args)
            {
                Args = args;
            }

            public IEnumerator<string> GetEnumerator()
            {
                if (Args == null) yield break;
                foreach (var arg in Args)
                    yield return arg;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }


            public override string ToString()
            {
                var s = Args.Length == 0 ? "<empty>" : string.Join(", ", Args);
                return $"{GetType().Name} {nameof(Args)}: {s}";
            }
        }

        /// <summary>  </summary>
        public class ListRequest : IEnumerable<string>, IRpcRequest<ListRequest, ListResponse>
        {
            public string[] Args { get; }

            public ListRequest()
            {
                Args = Array.Empty<string>();
            }

            public ListRequest(string[] args)
            {
                Args = args;
            }

            public IEnumerator<string> GetEnumerator()
            {
                if (Args == null) yield break;
                foreach (var arg in Args)
                    yield return arg;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override string ToString()
            {
                var s = Args.Length == 0 ? "<empty>" : string.Join(", ", Args);
                return $"{GetType().Name} {nameof(Args)}: {s}";
            }
        }

        public class ListResponse
        {
            private static readonly string[] _emptyList = Array.Empty<string>();
            private string[] _list = _emptyList;

            public string[] List
            {
                get { return _list; }
                set { _list = value ?? _emptyList; }
            }

            public override string ToString()
            {
                var s = List.Length == 0 ? "<empty>" : string.Join(", ", List);
                return $"{nameof(List)}: {s}";
            }
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {

        }
    }
}



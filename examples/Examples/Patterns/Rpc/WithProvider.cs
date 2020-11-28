using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RedPipes.Configuration;

namespace RedPipes.Patterns.Rpc
{
    [Description("Executes a http(s) GET rpc to the supplied url, append further parameters as a query string like ?q=arg2+arg3+...")]
    public class WithProvider : IPipe<string[]>, IRpcProvider
    {
        /// <summary> <see cref="IPipe{T}"/> implementation </summary>
        public async Task Execute(IContext ctx, string[] value)
        {
            var requestPipe = await Pipe
                    .Build.For<string[]>()
                    .WithRpcProvider(this, new RpcOptions { Timeout = TimeSpan.FromSeconds(5) })
                    .OnRpcResponse<string>(pipe => pipe
                        .Use(async (c, response) => await Console.Out.WriteLineAsync("Jay! I got a response!")))
                    .OnRpcError<Exception>(pipe => pipe
                        .Use(async (c, error) => await Console.Error.WriteLineAsync("Booh! I got an error:\n" + error.Message)))
                    .Use(async (c, response) =>
                    {
                        await Console.Out.WriteLineAsync(response);
                    }).Build();

            await requestPipe.Execute(ctx, value);
        }

        /// <summary> <see cref="IRpcProvider"/> implementation </summary>
        public async Task<(IContext, TResponse)> Call<TRequest, TResponse>(IContext ctx, TRequest request, RpcOptions options)
        {
            // Usually here the serialization of rpc to wire format and
            // deserialization of the response from wire format is done.

            // for demonstration purposes we fake it here.
            if (request is string[] s && (typeof(TResponse) == typeof(string)))
            {
                var (responseContext, response) = await CallString(ctx, s, options);

                return (responseContext, (TResponse)response);
            }
            else
            {
                throw new NotSupportedException("Only string[] supported as rpc type and string as response type");
            }
        }

        private async Task<(IContext, object)> CallString(IContext ctx, string[] strings, RpcOptions options)
        {
            if (strings.Length < 1)
                throw new InvalidOperationException("1 parameter required");

            var builder = new UriBuilder(strings[0]);

            if (strings.Length > 1)
            {
                var q = "?q=";
                foreach (var p in strings.Skip(1))
                {
                    q += Uri.EscapeUriString(p);
                    q += "+";
                }

                builder.Query = q.TrimEnd('+');
            }

            using var client = new HttpClient()
            {
                Timeout = options.Timeout,
            };
            var response = await client.GetAsync(builder.Uri);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return (ctx, content);
        }

        public IEnumerable<(string, IPipe)> Next()
        {
            yield break;
        }
    }
}

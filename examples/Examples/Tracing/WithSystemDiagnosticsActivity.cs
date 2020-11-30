using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;
using RedPipes.Patterns.Rpc;
using RedPipes.Telemetry.Tracing;

namespace RedPipes.Tracing
{
    [Description("Shows how to include tracing in a pipe, executes a http(s) GET request to the supplied url in the first argument, append further arguments as a query string like ?q=arg2+arg3+...")]
    public class WithSystemDiagnosticsActivity : IPipe<string[]>, IRpcProvider
    {
        private const int MaxValueStringLength = 30;

        static WithSystemDiagnosticsActivity()
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;
        }

        /// <summary> <see cref="IPipe{T}"/> implementation </summary>
        public async Task Execute(IContext ctx, string[] value)
        {
            using var listener = new ActivityListener()
            {
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllDataAndRecorded,
                ShouldListenTo = x => true,
                ActivityStarted = ActivityStarted,
                ActivityStopped = ActivityStopped,
            };

            ActivitySource.AddActivityListener(listener);

            var rpc = await Pipe
                    .Builder.For<string[]>()
                    .UseDiagnosticsActivity("rpc.request", ActivityKind.Client, getTags: (c, args) => args.Select((a, i) => new KeyValuePair<string, object>($"arg{i}", a)))
                    .WithRpcProvider(this, new RpcOptions { Timeout = TimeSpan.FromSeconds(5) })
                    .OnRpcResponse<string>(pipe => pipe
                        .UseDiagnosticsActivity("rpc.response")
                        .Use((_, rsp) => Console.WriteLine("Response received: {0}", string.Join("," ,rsp))))
                    .OnRpcError<Exception>(pipe => pipe
                        .UseDiagnosticsActivity("rpc.error", getTags: GetExceptionTags)
                        .Use((_, ex) => Console.WriteLine("Handled exception: {0}", ex.Message)))
                    .Build(); 

            try
            {
                await rpc.Execute(ctx, value);
            }
            catch
            {
                // ignore, tracing will show the exception
            }
        }

        private IEnumerable<KeyValuePair<string, object>> GetExceptionTags(IContext c, Exception ex)
        {
            yield return new KeyValuePair<string, object>("exception.type", ex.GetType().Name);
            yield return new KeyValuePair<string, object>("exception.message", ex.Message);
            yield return new KeyValuePair<string, object>("exception.stacktrace", ex.ToString());
        }

        private void ActivityStarted(Activity activity)
        {
            Console.WriteLine($"ti:{activity.TraceId} si:{activity.SpanId} ts:{activity.Context.TraceState} op:{activity.OperationName}, type:{activity.Kind} started at {activity.StartTimeUtc:O}, data: {{{string.Join(", ", activity.TagObjects.Select(t => $"{t.Key}: \"{ToString(t.Value, MaxValueStringLength)}\""))}}}");
        }

        private void ActivityStopped(Activity activity)
        {
            Console.WriteLine($"ti:{activity.TraceId} si:{activity.SpanId} ts:{activity.Context.TraceState} op:{activity.OperationName}, type:{activity.Kind} stopped and took {activity.Duration:g}, data: {{{string.Join(", ", activity.TagObjects.Select(t => $"{t.Key}: \"{ToString(t.Value, MaxValueStringLength)}\""))}}}");
            foreach (var e in activity.Events)
            {
                Console.WriteLine($"    event '{e.Name}' at {activity.StartTimeUtc:O}, data: {{{string.Join(", ", e.Tags.Select(t => $"{t.Key}: \"{ToString(t.Value, MaxValueStringLength)}\""))}}}");
            }
        }

        private static string ToString(object o, int maxLength)
        {
            var s = $"{o}".Replace("\r", "").Replace("\n", "\\n");
            if (s.Length > maxLength)
            {
                if (maxLength < 12)
                    return s.Substring(0, maxLength);

                var half = maxLength / 2;

                var firstHalf = s.Substring(0, half - 1);
                var secondHalfLength = maxLength - (firstHalf.Length - 2);
                var secondHalf = s.Substring(s.Length - secondHalfLength);
                return firstHalf + "..." + secondHalf;
            }

            return s;
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

        public void Accept(IGraphBuilder<IPipe> visitor)
        {

        }
    }
}

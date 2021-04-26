using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Autofac.Core.Resolving.Pipeline;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace RedPipes.Configuration.Visualization
{
    [TestClass]
    public class GraphVisualizationTests
    {
        [TestMethod]
        public async Task GeneratePipeDgmlFile()
        {
            var pipe = await Pipe.Builder<int>()
                .UseTransform()
                .Value(num => num.ToString())
                .UseTransform()
                .Value(double.Parse)
                .Build();


            var g = new DgmlGraph<IPipe>();
            pipe.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
        }

        [TestMethod]
        public async Task TestMultipleInputPipesPipeGraph()
        {
            var builder = Pipe.Builder<int>()
                .UseTransform()
                .Value(num => num.ToString())
                .UseConsolePrinter()
                .UseTransform()
                .Value(double.Parse)
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var user = await Pipe.Builder<int>().Named("User Input").Build(pipe);
            var api = await Pipe.Builder<int>().Named("API Input").Build(pipe);
            var g = new DgmlGraph<object>();
            user.Accept(g);
            api.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            //  g.SaveDgmlAsTempFileAndOpen();
        }

        [TestMethod]
        public async Task TestMultipleInputPipesBuilderGraph()
        {
            var builder = Pipe.Builder<int>()
                .UseTransform()
                .Value(num => num.ToString(), "Convert int to string")
                .UseConsolePrinter()
                .UseTransform()
                .Value(double.Parse, "Parse string to double")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var user = Pipe.Builder<int>().Named("User Input").UseTransform().Builder(builder);
            var api = Pipe.Builder<int>().Named("API Input").UseTransform().Builder(builder);
            var g = new DgmlGraph<object>();

            user.Accept(g);
            api.Accept(g);
            (await user.Build()).Accept(g);
            (await api.Build()).Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            // g.SaveDgmlAsTempFileAndOpen();
        }

        [TestMethod]
        public async Task CanVisualizeBranchedPipe()
        {
            var builder = Pipe.Builder<int>()
                .UseBranch((ctx, i) => i % 2 == 0
                    , Pipe.Builder<int>().Named("Do even stuff")
                    , Pipe.Builder<int>().Named("Do odd stuff")
                    , "x % 2 == 0")
                .UseTransform()
                .Value(num => num.ToString(), "Convert int to string")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var g = new DgmlGraph<object>();
            builder.Accept(g);
            pipe.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            // g.SaveDgmlAsTempFileAndOpen();
        }

        [TestMethod]
        public async Task CanVisualizeTrueBranchedPipe()
        {
            var builder = Pipe.Builder<int>()
                .UseBranch((ctx, i) => i % 2 == 0
                    , Pipe.Builder<int>().Named("Do even stuff")
                    , "x % 2 == 0")
                .UseTransform()
                .Value(num => num.ToString(), "Convert int to string")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var g = new DgmlGraph<object>();
            builder.Accept(g);
            pipe.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            // g.SaveDgmlAsTempFileAndOpen();
        }



        [TestMethod]
        public async Task CanVisualizeSwitchedPipe()
        {

            IBuilder<RequestContext, RequestContext> UseSerializer(Func<byte[], object> serializer, string name)
            {
                return Pipe.Builder<RequestContext>()
                    .UseTransform().ContextAndValue((ctx, rc) =>
                    {
                        rc.Serializer = serializer;
                        return (ctx, rc);
                    }, name);
            }


            var onUnknownContentType = Pipe
                .Builder<RequestContext>()
                .Use((ctx, rc) => throw new SerializationException("Unknown content type: " + rc.Request.ContentType), "Throw Unknown Content-Type Exception")
                .StopProcessing();

            var builder = Pipe.Builder<Request>()
                .Named("Receive Request")
                .UseTransform().ContextAndValue((ctx, r) => (ctx, new RequestContext(r)), "Create Request Context")
                .UseSwitch((ctx, m) => m.Request.ContentType,
                    new Dictionary<string, IBuilder<RequestContext, RequestContext>>
                    {
                        {
                            "application/json", 
                            UseSerializer(bytes =>
                            {
                                var json = Encoding.UTF8.GetString(bytes);
                                return JsonConvert.DeserializeObject(json);
                            }, "Set JSON Serializer")
                        },
                        {
                            "application/xml",
                            UseSerializer(bytes =>
                            {
                                var serializer = new XmlSerializer(typeof(object));
                                using var memoryStream = new MemoryStream(bytes);
                                var obj = serializer.Deserialize(memoryStream);
                                if (obj == null)
                                    throw new SerializationException("Serialization failed");
                                return obj;
                            }, "Set XML Serializer")
                        }
                    }
                    , onUnknownContentType
                    , fallThrough: true
                    , switchName: "Select deserializer\nbased on Content-Type")
                .Use((ctx, rc) => rc.DeserializedBody = rc.Serializer?.Invoke(rc.Request.Body), "Deserialize body")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var g = new DgmlGraph<object>();
            builder.Accept(g);
            pipe.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            // g.SaveDgmlAsTempFileAndOpen();
        }

        /// <summary>  </summary>
        class Request
        {
            public Request(string contentType, byte[] body)
            {
                ContentType = contentType;
                Body = body;
            }

            /// <summary>  </summary>
            public string ContentType { get;   }
            /// <summary>  </summary>
            public byte[] Body { get;   }
        }

        /// <summary>  </summary>
        class RequestContext
        {
            /// <summary>  </summary>
            public RequestContext(Request request)
            {
                Request = request;
            }

            public Request Request { get; }
            public Func<byte[], object>? Serializer { get; set; }
            public object? DeserializedBody { get; set; }
        }
    }


    static class NamedPipe
    {
        public static IBuilder<TIn, TOut> Named<TIn, TOut>(this IBuilder<TIn, TOut> builder, string name)
        {
            return builder.Use(next => new Pipe<TOut>(next, name), name);
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;
            private readonly string _name;

            public Pipe(IPipe<T> next, string name)
            {
                _next = next;
                _name = name;
            }

            public Task Execute(IContext ctx, T value)
            {
                return _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (Keys.Name, _name));
                visitor.AddEdge(this, _next, (Keys.Name, "Next"));
                _next.Accept(visitor);
            }
        }
    }
}

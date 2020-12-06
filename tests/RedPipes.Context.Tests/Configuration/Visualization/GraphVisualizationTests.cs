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
            var pipe = await Pipe.Build.For<int>()
                .Transform()
                .Use(num => num.ToString())
                .Transform()
                .Use(double.Parse)
                .Build();


            var g = new DgmlGraph<IPipe>();
            pipe.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
        }

        [TestMethod]
        public async Task TestMultipleInputPipesPipeGraph()
        {
            var builder = Pipe.Build.For<int>()
                .Transform()
                .Use(num => num.ToString())
                .UseConsolePrinter()
                .Transform()
                .Use(double.Parse)
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var user = await Pipe.Build.For<int>().Named("User Input").Build(pipe);
            var api = await Pipe.Build.For<int>().Named("API Input").Build(pipe);
            var g = new DgmlGraph<object>();
            user.Accept(g);
            api.Accept(g);
            g.GetDgmlDocument().Save(Console.Out);
            //  g.SaveDgmlAsTempFileAndOpen();
        }

        [TestMethod]
        public async Task TestMultipleInputPipesBuilderGraph()
        {
            var builder = Pipe.Build.For<int>()
                .Transform()
                .Use(num => num.ToString(), "Convert int to string")
                .UseConsolePrinter()
                .Transform()
                .Use(double.Parse, "Parse string to double")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var user = Pipe.Build.For<int>().Named("User Input").Transform().Use(builder);
            var api = Pipe.Build.For<int>().Named("API Input").Transform().Use(builder);
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
            var builder = Pipe.Build.For<int>()
                .UseBranch((ctx, i) => i % 2 == 0
                    , Pipe.Build.For<int>().Named("Do even stuff")
                    , Pipe.Build.For<int>().Named("Do odd stuff")
                    , "x % 2 == 0")
                .Transform()
                .Use(num => num.ToString(), "Convert int to string")
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
            var builder = Pipe.Build.For<int>()
                .UseBranch((ctx, i) => i % 2 == 0
                    , Pipe.Build.For<int>().Named("Do even stuff")
                    , "x % 2 == 0")
                .Transform()
                .Use(num => num.ToString(), "Convert int to string")
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
                return Pipe.Build.For<RequestContext>()
                    .Transform().Use((ctx, rc) =>
                    {
                        rc.Serializer = serializer;
                        return (ctx, rc);
                    }, name);
            }


            var onUnknownContentType = Pipe
                .For<RequestContext>()
                .Use((ctx, rc) => throw new SerializationException("Unknown content type: " + rc.Request.ContentType), "Throw Unknown Content-Type Exception")
                .StopProcessing();

            var builder = Pipe.Build.For<Request>()
                .Named("Receive Request")
                .Transform().Use((ctx, r) => (ctx, new RequestContext(r)), "Create Request Context")
                .UseSwitch((ctx, m) => m.Request.ContentType,
                    new Dictionary<string, IBuilder<RequestContext, RequestContext>>
                    {
                        {
                            "application/json", UseSerializer(bytes =>
                            {
                                var json = Encoding.UTF8.GetString(bytes);
                                return JsonConvert.DeserializeObject(json);
                            }, "Set JSON Serializer")
                        },{
                            "application/xml", UseSerializer(bytes =>
                            {
                                var json = Encoding.UTF8.GetString(bytes);
                                return new XmlSerializer(typeof(object)).Deserialize(new MemoryStream(bytes));
                            }, "Set XML Serializer")
                        },
                    }
                    , onUnknownContentType
                    , fallThrough: true
                    , switchName: "Select deserializer\nbased on Content-Type")
                .Use((ctx, rc) => rc.DeserializedBody = rc.Serializer(rc.Request.Body), "Deserialize body")
                .UseConsolePrinter();

            var pipe = await builder.Build();

            var g = new DgmlGraph<object>();
            builder.Accept(g);
            pipe.Accept(g);
            //  g.GetDgmlDocument().Save(Console.Out);
            g.SaveDgmlAsTempFileAndOpen();
        }

        class Request
        {
            public string ContentType { get; set; }
            public byte[] Body { get; set; }
        }
        class RequestContext
        {
            public RequestContext(Request request)
            {
                Request = request;
            }

            public Request Request { get; }
            public Func<byte[], object> Serializer { get; set; }
            public object DeserializedBody { get; set; }
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

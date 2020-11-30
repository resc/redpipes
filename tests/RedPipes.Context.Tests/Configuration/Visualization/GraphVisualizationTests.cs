using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedPipes.Configuration.Visualization
{
    [TestClass]
    public class GraphVisualizationTests
    {
        [TestMethod]
        public async Task GenerateDgmlFile()
        {
            var pipe = await Pipe.Builder.For<int>()
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
        public async Task TestMultipleInputPipes()
        {
            var pipe = await Pipe.Builder.For<int>()
                .Transform()
                .Use(num => num.ToString())
                .UseConsolePrinter()
                .Transform()
                .Use(double.Parse)
                .UseConsolePrinter()
                .Build();

            var user = await Pipe.Builder.For<int>().Named("user input").Build(pipe);
            var api = await Pipe.Builder.For<int>().Named("api input").Build(pipe);
            var g = new DgmlGraph<IPipe>();
            user.Accept(g);
            api.Accept(g);
            g.SaveDgmlAsTempFileAndOpen();
        }


    }
    static class TestExtensions
    {
        public static IBuilder<TIn, TOut> UseConsolePrinter<TIn, TOut>(this IBuilder<TIn, TOut> builder)
        {
            return builder.Use(next => new Pipe<TOut>(next));
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;

            public Pipe(IPipe<T> next)
            {
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                await Console.Out.WriteLineAsync($"Print: {value}");
                await _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (NodeLabels.Label, "Print value to console"));
                visitor.AddEdge(this, _next, (EdgeLabels.Label, "Next"));
                _next.Accept(visitor);
            }
        }
    }
    static class NamedPipe
    {
        public static IBuilder<TIn, TOut> Named<TIn, TOut>(this IBuilder<TIn, TOut> builder, string name)
        {
            return builder.Use(next => new Pipe<TOut>(next,name));
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

            public async Task Execute(IContext ctx, T value)
            {
                await Console.Out.WriteLineAsync($"Print: {value}");
                await _next.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this, (NodeLabels.Label, _name));
                visitor.AddEdge(this, _next, (EdgeLabels.Label, "Next"));
                _next.Accept(visitor);
            }
        }
    }
}

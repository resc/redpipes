using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedPipes.Introspection;

namespace RedPipes.Configuration
{
    [TestClass]
    public class PipeBuildOrderingTests
    {
        [TestMethod]
        public async Task PipeSegmentsAreExecutedInOrder()
        {
            var actual = new List<int>();

            var builder = Pipe.Builder<List<int>>()
                .Use((ctx, list) => list.Add(1))
                .Use((ctx, list) => list.Add(2))
                .UseTransform().Value(list =>
                {
                    list.Add(3);
                    return (ICollection<int>)list;
                })
                .Use((ctx, list) => list.Add(4));

            var pipe = await builder.Build();

            await pipe.Execute(Context.Background, actual);

            var expected = new List<int> { 1, 2, 3, 4 };
            Console.WriteLine(pipe.DumpPipeStructure());
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

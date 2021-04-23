using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;
using RedPipes.OpenTelemetry.Tracing;

namespace RedPipes
{
    [TestClass]
    public class PipeTests
    {
        [TestMethod]
        public async Task CanConvertValueAndContextInPipe()
        {
            ActivitySource.AddActivityListener(new ActivityListener()
            {
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ShouldListenTo = x => true,
                ActivityStarted = activity => Console.WriteLine($"{activity.DisplayName} {activity.Id} started at {activity.StartTimeUtc}"),
                ActivityStopped = obj => Console.WriteLine($"{obj.DisplayName} {obj.Id} took {obj.Duration:g}"),
            });
            var id = Guid.NewGuid();
            var builder = Pipe.Build<int>()
                .UseDiagnosticsActivity("TestActivity")
                .Transform().Use((ctx, data) => (ctx, data.ToString()));

            var output = new Output();
            var pipe = await builder.Build(output);
            int count = 0;
            while (count++ < 20)
                await pipe.Execute(Context.Background, 1);

            Assert.AreEqual("1", output.Value);

        } 

        class Output : IPipe<string>
        {
            public Task Execute(IContext ctx, string value)
            {
                Context = ctx;
                Value = value;
                return Task.CompletedTask;
            }

            public string? Value { get; private set; }

            public IContext? Context { get; private set; }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
            }
        }

    }

}

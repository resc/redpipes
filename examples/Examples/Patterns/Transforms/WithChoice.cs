using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Branching
{
    [Description("Transform demo, convert the pipeline value to another value")]
    public class WithTransform : IPipe<string[]>
    {
        public async Task Execute(IContext context, string[] args)
        {
            var pipe = await Pipe.Builder<string>()
                // convert string to an int
                .UseTransform().Value(double.Parse)
                // convert int to timespan
                .UseTransform().Value(TimeSpan.FromSeconds)
                // print timespan
                .Use((ctx, value) => Console.WriteLine($"{value:c}"))
                .Build();

            await pipe.Execute(context, args.FirstOrDefault() ?? "");
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {
            //
        }
    }
}

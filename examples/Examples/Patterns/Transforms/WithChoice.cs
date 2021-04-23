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
            var pipe = await Pipe.Build<string>()
                // convert string to an int
                .Transform().Use((ctx, value) => (ctx, int.Parse(value)))
                // convert int to timespan
                .Transform().Use((ctx, value) => (ctx, TimeSpan.FromSeconds(value)))
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

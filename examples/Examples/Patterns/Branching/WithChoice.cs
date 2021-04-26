using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Branching
{
    [Description("UseChoice demo, choose between alternate branches")]
    public class WithChoice : IPipe<string[]>
    {
        public async Task Execute(IContext context, string[] args)
        {
            var alternate = Pipe.Builder<string>()
                .Use((ctx, value) => Console.WriteLine("Only executed when condition evaluates to true")); 

            var pipe = await Pipe.Builder<string>()
                .UseChoice((ctx, value) => value == "yes", alternate)
                .Use((ctx, value) => Console.WriteLine("Only executed when condition evaluates to false"))
                .Build();

            await pipe.Execute(context, args.FirstOrDefault() ?? "");
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {
            //
        }
    }
}

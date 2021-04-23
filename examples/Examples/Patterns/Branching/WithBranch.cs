using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Branching
{
    [Description("UseBranch demo, use 'yes' or any other arg to change choices")]
    public class WithBranch : IPipe<string[]>
    {
        public async Task Execute(IContext context, string[] args)
        {
            var trueBranch = Pipe.Build<string>()
                .Use((ctx, value) => Console.WriteLine("True branch taken"));

            var falseBranch = Pipe.Build<string>()
                .Use((ctx, value) => Console.WriteLine("False branch taken"));

            var pipe = await Pipe.Build<string>()
                .UseBranch((ctx, value) => value == "yes", trueBranch, falseBranch)
                .Use((ctx, value) => Console.WriteLine("Executed after either true of false branch is taken"))
                .Build();

            await pipe.Execute(context, args.FirstOrDefault() ?? "");
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {
            //
        }
    }

}

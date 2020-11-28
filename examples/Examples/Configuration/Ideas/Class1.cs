using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RedPipes.Patterns.Auth;

namespace RedPipes.Configuration.Ideas
{
    public class ClaimCheckPipeConfiguration : Config<ClaimCheck, string[]>
    {
        protected override IBuilder<string[], string[]> Configure(IContext ctx, IBuilder<string[], string[]> pipe)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary> A class to add pipe implementation specific configuration to the pipeline. </summary>
    public abstract class Config<TPipe, TValue> : IPipe<Config<TPipe, TValue>> where TPipe : IPipe<TValue>
    {

        public IBuilder<TValue, TValue> Builder { get; private set; }

        public Task Execute(IContext ctx, Config<TPipe, TValue> value)
        {
            Builder = Configure(ctx, Pipe.Build.For<TValue>());
            return Task.CompletedTask;
        }

        /// <summary> ctx can be used to pass configuration stuff around. </summary>
        protected abstract IBuilder<TValue, TValue> Configure(IContext ctx, IBuilder<TValue, TValue> pipe);

        IEnumerable<(string name, IPipe next)> IPipe.Next()
        {
            yield break;
        }
    }
}

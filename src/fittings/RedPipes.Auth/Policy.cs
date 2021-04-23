using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;
using RedPipes.Patterns.Auth.Policies;

namespace RedPipes.Patterns.Auth
{
    public static class Policy
    {
        public static IBuilder<TIn, TOut> UseAuthPolicy<TIn, TOut>(this IBuilder<TIn, TOut> builder, Policy<TOut> policy, IBuilder<TOut, TOut> onDeny)
        {
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            return builder.UseAsync(async (next) => new Pipe<TOut>(policy, await onDeny.Build(), next), policy.Name);
        }

        public static IBuilder<TIn, TOut> UseAuthPolicy<TIn, TOut>(this IBuilder<TIn, TOut> builder, Func<PolicyBuilder<TOut>, PolicyBuilder<TOut>> configure, IBuilder<TOut, TOut> onDeny)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var policyBuilder = new PolicyBuilder<TOut>(null);
            var policy = configure(policyBuilder).Build();
            return builder.UseAuthPolicy(policy, onDeny);
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly Policy<T> _policy;
            private readonly IPipe<T> _deny;
            private readonly IPipe<T> _accept;

            public Pipe(Policy<T> policy, IPipe<T> deny, IPipe<T> accept)
            {
                _policy = policy;
                _deny = deny;
                _accept = accept;
            }

            public async Task Execute(IContext ctx, T value)
            {
                var result = _policy.Evaluate(ctx, value);
                if (Decision.Permit == result.Decision)
                    await _accept.Execute(ctx, value);
                else
                    await _deny.Execute(ctx, value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _accept, (Keys.Name, "Accept")))
                    _accept.Accept(visitor);

                if (visitor.AddEdge(this, _deny, (Keys.Name, "Deny")))
                    _deny.Accept(visitor);
            }
        }
    }
}

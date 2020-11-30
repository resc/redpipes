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

            return builder.Use(new Builder<TOut>(policy, onDeny));
        }

        public static IBuilder<TIn, TOut> UseAuthPolicy<TIn, TOut>(this IBuilder<TIn, TOut> builder, Func<PolicyBuilder<TOut>, PolicyBuilder<TOut>> configure, IBuilder<TOut, TOut> onDeny)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var policyBuilder = new PolicyBuilder<TOut>(null);
            var policy = configure(policyBuilder).Build();
            return builder.UseAuthPolicy(policy, onDeny);
        }


        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly Policy<T> _policy;
            private readonly IBuilder<T, T> _deny;

            public Builder(Policy<T> policy, IBuilder<T, T> deny)
            {
                _policy = policy;
                _deny = deny;
            }

            public async Task<IPipe<T>> Build(IPipe<T> next)
            {
                var deny = await _deny.Build();
                IPipe<T> pipe = new Pipe<T>(_policy, deny, next);
                return pipe;
            }

            public override void Accept(IGraphBuilder<IBuilder> visitor, IBuilder next)
            {
                visitor.AddEdge(this, next, (EdgeLabels.Label, "accept"));
                visitor.AddEdge(this, _deny, (EdgeLabels.Label, "deny"));
                _deny.Accept(visitor, null);
            }
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
                if (visitor.AddEdge(this, _accept, (EdgeLabels.Label, "accept")))
                    _accept.Accept(visitor);

                if (visitor.AddEdge(this, _deny, (EdgeLabels.Label, "deny")))
                    _deny.Accept(visitor);
            }
        }
    }
}

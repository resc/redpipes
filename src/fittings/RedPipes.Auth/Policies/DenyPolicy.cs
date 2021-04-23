using System;

namespace RedPipes.Auth.Policies
{
    sealed class DenyPolicy<T> : Policy<T>
    {
        public DenyPolicy() : base("DenyPolicy")
        {
        }

        protected override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            return Decision.Deny;
        }
    }
}

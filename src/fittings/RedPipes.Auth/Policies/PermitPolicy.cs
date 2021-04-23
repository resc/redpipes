using System;

namespace RedPipes.Auth.Policies
{
    sealed class PermitPolicy<T> : Policy<T>
    {
        public PermitPolicy() : base("PermitPolicy")
        {
        }

        protected override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            return Decision.Permit;
        }
    }
}

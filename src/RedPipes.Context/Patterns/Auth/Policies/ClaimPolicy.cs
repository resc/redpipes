using System;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class ClaimPolicy<T> : Policy<T>
    {
        private readonly string _claimType;
        private readonly string _claimValue;

        public ClaimPolicy(string claimType, string claimValue) : base("ClaimPolicy")
        {
            _claimType = claimType;
            _claimValue = claimValue;
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            var p = ctx.GetPrincipal();


            foreach (var identity in p.Identities)
            {
                if (identity.IsAuthenticated)
                {
                    if (identity.HasClaim(_claimType, _claimValue))
                        return Decision.Permit;
                }
            }


            return Decision.Deny;
        }
    }
}

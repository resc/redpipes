using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class ClaimPredicatePolicy<T> : Policy<T>
    {
        private readonly string? _claimType;
        private readonly Predicate<Claim> _predicate;

        public ClaimPredicatePolicy(string? claimType, Predicate<Claim> predicate) : base("ClaimPredicatePolicy")
        {
            _claimType = claimType;
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            var p = ctx.GetPrincipal();
            if (_claimType == null)
            {
                if (p.FindFirst(_predicate) != null)
                    return Decision.Permit;
            }
            else
            {
                foreach (var claim in p.FindAll(_claimType))
                    if (_predicate(claim))
                        return Decision.Permit;
            }

            return Decision.Deny;
        }
    }
}

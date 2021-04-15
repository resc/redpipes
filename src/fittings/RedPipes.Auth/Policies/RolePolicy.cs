using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class RolePolicy<T> : Policy<T>
    {
        private readonly string[] _roles;

        public RolePolicy([NotNull] IEnumerable<string> roles) : base("RolePolicy")
        {
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            _roles = roles
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToArray();
        }

        public RolePolicy(params string[] roles)
            : this((IEnumerable<string>)roles)
        {
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            var p = ctx.GetPrincipal();

            var count = 0;
            foreach (var role in _roles)
            {
                if (p.IsInRole(role))
                    count++;
            }

            if (count == _roles.Length)
                return Decision.Permit;

            return Decision.Deny;
        }
    }
}

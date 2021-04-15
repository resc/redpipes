using System.Collections.Generic;

namespace RedPipes.Patterns.Auth.Policies
{
    public struct PolicyResult<T>
    {
        public static readonly PolicyResult<T>[] EmptyResults = new PolicyResult<T>[0];

        public PolicyResult(Policy<T> policy, Decision decision, IReadOnlyCollection<PolicyResult<T>>? associatedResults = null)
        {
            Policy = policy;
            Decision = decision;
            AssociatedResults = associatedResults ?? EmptyResults;
        }

        public Policy<T> Policy { get; }

        public Decision Decision { get; }

        public IReadOnlyCollection<PolicyResult<T>> AssociatedResults { get; }
    }
}

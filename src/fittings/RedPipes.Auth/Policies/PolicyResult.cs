using System;
using System.Collections.Generic;

namespace RedPipes.Auth.Policies
{
    /// <summary>  </summary>
    public struct PolicyResult<T>
    {
        /// <summary>  </summary>
        public static readonly PolicyResult<T>[] EmptyResults = Array.Empty<PolicyResult<T>>();

        /// <summary>  </summary>
        public PolicyResult(Policy<T> policy, Decision decision, IReadOnlyCollection<PolicyResult<T>>? associatedResults = null)
        {
            Policy = policy;
            Decision = decision;
            AssociatedResults = associatedResults ?? EmptyResults;
        }

        /// <summary>  </summary>
        public Policy<T> Policy { get; }

        /// <summary>  </summary>
        public Decision Decision { get; }

        /// <summary>  </summary>
        public IReadOnlyCollection<PolicyResult<T>> AssociatedResults { get; }
    }
}

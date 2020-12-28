﻿using System;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class PermitPolicy<T> : Policy<T>
    {
        public PermitPolicy() : base("PermitPolicy")
        {
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            return Decision.Permit;
        }
    }
}

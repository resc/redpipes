﻿namespace RedPipes.Patterns.Auth.Policies
{
    sealed class DenyPolicy<T> : Policy<T>
    {
        public DenyPolicy() : base("DenyPolicy")
        {
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = null;
            return Decision.Deny;
        }
    }
}
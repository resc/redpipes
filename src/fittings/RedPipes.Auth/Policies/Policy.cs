using System;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth.Policies
{
    /// <summary>
    /// Authentication <see cref="Policy{T}"/> decide if a pipe can be executed,
    /// based on <see cref="IContext"/> and data passing through the pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Policy<T>
    {
        public static readonly Policy<T> Permit = new PermitPolicy<T>();

        public static readonly Policy<T> Deny = new DenyPolicy<T>();

        public static PolicyBuilder<T> Builder()
        {
            return new PolicyBuilder<T>(null);
        }

        public Policy(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetNameFromType() : name;
        }

        public string Name { get; }


        public PolicyResult<T> Evaluate(IContext ctx, T value)
        {
            var decision = Decide(ctx, value, out var associatedResults);
            return new PolicyResult<T>(this, decision, associatedResults);
        }

        public virtual Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            return Decision.None;
        }

        private string GetNameFromType()
        {
            var type = GetType().Name;
            var index = type.IndexOf('`');
            if (index >= 0)
                return type.Substring(0, index);

            return type;
        }

        public virtual Policy<T> Reduce()
        {
            return this;
        }
    }
}

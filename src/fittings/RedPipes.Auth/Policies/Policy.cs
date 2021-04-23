using System;

namespace RedPipes.Auth.Policies
{
    /// <summary>
    /// Authentication <see cref="Policy{T}"/> decide if a pipe can be executed,
    /// based on <see cref="IContext"/> and data passing through the pipe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Policy<T>
    {
        /// <summary> Default permit policy </summary>
        public static readonly Policy<T> Permit = new PermitPolicy<T>();

        /// <summary> Default deny policy </summary>
        public static readonly Policy<T> Deny = new DenyPolicy<T>();

        /// <summary> Policy builder </summary>
        public static PolicyBuilder<T> Builder()
        {
            return new PolicyBuilder<T>(null);
        }

        /// <summary> policy constructor </summary>
        public Policy(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetNameFromType() : name;
        }

        /// <summary> The name for this policy </summary>
        public string Name { get; }

        /// <summary> Evaluate this policy with the given <paramref name="ctx"/> and <paramref name="value"/> </summary>
        public PolicyResult<T> Evaluate(IContext ctx, T value)
        {
            var decision = Decide(ctx, value, out var associatedResults);
            return new PolicyResult<T>(this, decision, associatedResults);
        }

        /// <summary> Make the actual policy decision </summary>
        protected virtual Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
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

        /// <summary> Reduce this combined policy to a simpler equivalent one.  </summary>
        public virtual Policy<T> Reduce()
        {
            return this;
        }
    }
}

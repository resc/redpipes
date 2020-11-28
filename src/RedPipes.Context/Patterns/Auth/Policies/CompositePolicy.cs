using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class CompositePolicy<T> : Policy<T>, IReadOnlyCollection<Policy<T>>
    {
        private readonly Policy<T>[] _policies;

        public Strategy Strategy { get; }

        public CompositePolicy(string name, Strategy strategy, params Policy<T>[] policies)
            : this(name, strategy, (IEnumerable<Policy<T>>)policies)
        {

        }

        public CompositePolicy(string name, Strategy strategy, IEnumerable<Policy<T>> policies) : base(name)
        {
            Strategy = Enum.IsDefined(typeof(Strategy), strategy) ? strategy : Strategy.None;
            _policies = policies.ToArray();
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        { 
            var n = _policies.Length;

            if (n == 0)
            {
                associatedResults = null;
                return Decision.Deny;
            }

            associatedResults = new PolicyResult<T>[n];

            for (int i = 0; i < n; i++)
                associatedResults[i] = _policies[i].Evaluate(ctx, value);

            switch (Strategy)
            {
                case Strategy.Permissive:
                    {
                        foreach (var d in associatedResults)
                        {
                            if (d.Decision == Decision.Permit)
                                return Decision.Permit;
                        }

                        return Decision.Deny;
                    }
                case Strategy.Veto:
                    {
                        foreach (var d in associatedResults)
                        {
                            if (d.Decision == Decision.Deny)
                                return Decision.Deny;
                        }

                        return Decision.Permit;
                    }
                case Strategy.Majority:
                    {
                        int deny = 0;
                        int permit = 0;
                        foreach (var d in associatedResults)
                        {
                            switch (d.Decision)
                            {
                                case Decision.Deny:
                                    deny++;
                                    break;
                                case Decision.Permit:
                                    permit++;
                                    break;
                                default:
                                    continue;
                            }
                        }

                        return permit > deny ? Decision.Permit : Decision.Deny;
                    }
                case Strategy.Strict:
                    {
                        foreach (var d in associatedResults)
                        {
                            if (d.Decision == Decision.Permit)
                                continue;

                            return Decision.Deny;
                        }

                        return Decision.Permit;
                    }
                default:
                    return Decision.Deny;
            }
        }

        public override Policy<T> Reduce()
        {
            if (_policies.Length == 0)
                return Deny;
            
            var policies = new List<Policy<T>>();
            foreach (var p in _policies)
            {
                var policy = p.Reduce();
                if (policy is CompositePolicy<T> cp && cp.Strategy == Strategy)
                {
                    policies.AddRange(cp);
                }
                else
                {
                    policies.Add(policy);
                }
            }

            if (policies.Count == 1)
                return policies.First();

            return new CompositePolicy<T>(Name, Strategy, policies);
        }

        #region  IReadOnlyCollection<Policy<T>>

        public int Count
        {
            get { return _policies.Length; }
        }

        public IEnumerator<Policy<T>> GetEnumerator()
        {
            for (int i = 0; i < _policies.Length; i++)
                yield return _policies[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

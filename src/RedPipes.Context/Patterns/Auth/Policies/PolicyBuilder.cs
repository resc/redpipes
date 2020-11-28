using System;
using System.Linq;
using System.Security.Claims;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth.Policies
{
    public class PolicyBuilder<T>
    {
        private readonly Policy<T> _pol;

        public PolicyBuilder(Policy<T> pol)
        {
            _pol = pol;
        }

        #region composite policies

        public PolicyBuilder<T> Or(params Policy<T>[] policies)
        {
            return Combine(Strategy.Permissive, policies);
        }

        public PolicyBuilder<T> And(params Policy<T>[] policies)
        {
            return Combine(Strategy.Strict, policies);
        }

        public PolicyBuilder<T> Veto(params Policy<T>[] policies)
        {
            return Combine(Strategy.Veto, policies);
        }

        public PolicyBuilder<T> Majority(params Policy<T>[] policies)
        {
            return Combine(Strategy.Majority, policies);
        }

        public PolicyBuilder<T> Combine(Strategy strategy, params Policy<T>[] policies)
        {
            var pp = new[] { _pol }.Concat(policies).Where(x => x != null).ToArray();
            var policy = new CompositePolicy<T>(PolicyNameFor(strategy), strategy, pp);
            return new PolicyBuilder<T>(policy);
        }

        public PolicyBuilder<T> Combine(Strategy strategy, params PolicyBuilder<T>[] policyBuilders)
        {
            return Combine(strategy, policyBuilders.Select(p => p.Build()).ToArray());
        }

        public PolicyBuilder<T> Or(params PolicyBuilder<T>[] policyBuilders)
        {
            return Combine(Strategy.Permissive, policyBuilders.Select(p => p.Build()).ToArray());
        }

        public PolicyBuilder<T> And(params PolicyBuilder<T>[] policyBuilders)
        {
            return Combine(Strategy.Strict, policyBuilders.Select(p => p.Build()).ToArray());
        }

        public PolicyBuilder<T> Veto(params PolicyBuilder<T>[] policyBuilders)
        {
            return Combine(Strategy.Veto, policyBuilders.Select(p => p.Build()).ToArray());
        }

        public PolicyBuilder<T> Majority(params PolicyBuilder<T>[] policyBuilders)
        {
            return Combine(Strategy.Majority, policyBuilders.Select(p => p.Build()).ToArray());
        }

        #endregion

        public PolicyBuilder<T> IsTimeOfDay(TimeSpan startUtc, TimeSpan endUtcExclusive, Func<TimeSpan> timeOfDayUtc = null)
        {
            timeOfDayUtc ??= () => DateTimeOffset.UtcNow.TimeOfDay;

            if (startUtc > endUtcExclusive)
            {
                return new PolicyBuilder<T>(new CompositePolicy<T>(
                    PolicyNameFor(Strategy.Permissive),
                    Strategy.Permissive,
                    new TimeOfDayPolicy<T>(startUtc, TimeSpan.FromHours(24), timeOfDayUtc),
                    new TimeOfDayPolicy<T>(TimeSpan.Zero, endUtcExclusive, timeOfDayUtc)
                ));
            }
            else
            {
                return new PolicyBuilder<T>(new TimeOfDayPolicy<T>(startUtc, endUtcExclusive, timeOfDayUtc));
            }
        }

        private static DateTimeOffset SystemDateTime()
        {
            return DateTimeOffset.UtcNow;
        }

        public PolicyBuilder<T> IsAfter(DateTimeOffset startDateUtc, Func<DateTimeOffset> timeOfDayUtc = null)
        {
            timeOfDayUtc ??= SystemDateTime;
            return new PolicyBuilder<T>(new DateRangePolicy<T>(startDateUtc, DateTimeOffset.MaxValue, timeOfDayUtc));

        }

        public PolicyBuilder<T> IsBefore(DateTimeOffset endDateUtc, Func<DateTimeOffset> timeOfDayUtc = null)
        {
            timeOfDayUtc ??= SystemDateTime;
            return new PolicyBuilder<T>(new DateRangePolicy<T>(DateTimeOffset.MinValue, endDateUtc, timeOfDayUtc));

        }

        public PolicyBuilder<T> IsBetween(DateTimeOffset startDateUtc, DateTimeOffset endDateUtc, Func<DateTimeOffset> timeOfDayUtc = null)
        {
            timeOfDayUtc ??= SystemDateTime;
            if (startDateUtc > endDateUtc)
                return new PolicyBuilder<T>(new DateRangePolicy<T>(endDateUtc, startDateUtc, timeOfDayUtc));
            else
                return new PolicyBuilder<T>(new DateRangePolicy<T>(startDateUtc, endDateUtc, timeOfDayUtc));

        }

        public PolicyBuilder<T> IsDayOfWeek(Func<DayOfWeek> dayOfWeek = null, params DayOfWeek[] days)
        {
            dayOfWeek ??= () => DateTimeOffset.UtcNow.DayOfWeek;
            return new PolicyBuilder<T>(new DayOfWeekPolicy<T>(days, dayOfWeek));
        }

        public PolicyBuilder<T> HasRole(params string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return this;
            return new PolicyBuilder<T>(new RolePolicy<T>(roles));
        }

        public PolicyBuilder<T> HasClaim([NotNull] Predicate<Claim> claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            return new PolicyBuilder<T>(new ClaimPredicatePolicy<T>(null, claim));
        }

        public PolicyBuilder<T> HasClaim([NotNull] string claimType, [NotNull] Predicate<Claim> claim)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }


            return new PolicyBuilder<T>(new ClaimPredicatePolicy<T>(claimType, claim));
        }

        public PolicyBuilder<T> HasClaim([NotNull] string type, [NotNull] string value)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new PolicyBuilder<T>(new ClaimPolicy<T>(type, value));
        }

        public Policy<T> Build()
        {
            var policy = _pol ?? Policy<T>.Deny;
            return policy.Reduce();
        }

        private static string PolicyNameFor(Strategy strategy)
        {
            return "Composite" + strategy + "Policy";
        }
    }
}

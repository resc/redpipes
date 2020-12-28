using System;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class DateRangePolicy<T> : Policy<T>
    {
        private readonly DateTimeOffset _start;
        private readonly DateTimeOffset _endExclusive;
        private readonly Func<DateTimeOffset> _dateTimeUtc;


        public DateRangePolicy(DateTimeOffset start, DateTimeOffset endExclusive, [NotNull] Func<DateTimeOffset> dateTimeUtc) : base("DateRangePolicy")
        {
            _start = start;
            _endExclusive = endExclusive;
            _dateTimeUtc = dateTimeUtc ?? throw new ArgumentNullException(nameof(dateTimeUtc));
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = Array.Empty<PolicyResult<T>>();
            var dto = _dateTimeUtc();
            if (_start <= dto && dto < _endExclusive)
                return Decision.Permit;

            return Decision.Deny;
        }
    }
}

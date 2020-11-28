using System;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class TimeOfDayPolicy<T> : Policy<T>
    {
        private readonly TimeSpan _start;
        private readonly TimeSpan _endExclusive;
        private readonly Func<TimeSpan> _timeOfDayUtc;

        /// <summary> Be careful, if <paramref name="start"/> is greater thant </summary>
        /// <param name="start"></param>
        /// <param name="endExclusive"></param>
        /// <param name="timeOfDayUtc"></param>
        public TimeOfDayPolicy(TimeSpan start, TimeSpan endExclusive, [NotNull] Func<TimeSpan> timeOfDayUtc) : base("TimeOfDayPolicy")
        {
            VerifyRange(start, nameof(start));
            VerifyRange(endExclusive, nameof(endExclusive));
            VerifyOrder(start, endExclusive);

            _start = start;
            _endExclusive = endExclusive;
            _timeOfDayUtc = timeOfDayUtc ?? throw new ArgumentNullException(nameof(timeOfDayUtc));
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = null;
            var tod = _timeOfDayUtc();
            if (_start <= tod && tod < _endExclusive)
                return Decision.Permit;

            return Decision.Deny;
        }

        static void VerifyOrder(TimeSpan start, TimeSpan endExclusive)
        {
            if (start >= endExclusive)
                throw new ArgumentException("start value must be strictly less than endExclusive value");
        }

        static void VerifyRange(TimeSpan ts, string paramName)
        {
            if (0 > ts.Ticks || ts.Ticks > 24 * TimeSpan.TicksPerHour)
                throw new ArgumentOutOfRangeException(paramName, ts, "Valid range: 00:00:00 <= " + paramName + " <= 24:00:00");
        }
    }
}

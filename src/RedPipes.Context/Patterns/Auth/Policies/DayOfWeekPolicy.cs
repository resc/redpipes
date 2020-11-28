using System;
using System.Linq;

namespace RedPipes.Patterns.Auth.Policies
{
    sealed class DayOfWeekPolicy<T> : Policy<T>
    {
        private readonly DayOfWeek[] _days;
        private readonly Func<DayOfWeek> _dayOfWeek;

        public DayOfWeekPolicy(DayOfWeek[] days, Func<DayOfWeek> dayOfWeek) : base(nameof(DayOfWeekPolicy<T>))
        {
            _days = days.Distinct().ToArray();
            Array.Sort(_days);
            _dayOfWeek = dayOfWeek;
        }

        public override Decision Decide(IContext ctx, T value, out PolicyResult<T>[] associatedResults)
        {
            associatedResults = null;
            var dayOfWeek = _dayOfWeek();

            foreach (var day in _days)
            {
                if (day == dayOfWeek)
                    return Decision.Permit;
            }

            return Decision.Deny;
        }
    }
}

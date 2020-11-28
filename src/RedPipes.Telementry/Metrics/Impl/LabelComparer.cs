using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class LabelComparer : IComparer<KeyValuePair<string, string>>
    {
        public static LabelComparer Default { get; } = new LabelComparer(System.StringComparer.Ordinal);

        public IComparer<string> StringComparer { get; }

        public LabelComparer([NotNull] IComparer<string> stringComparer)
        {
            StringComparer = stringComparer ?? throw new ArgumentNullException(nameof(stringComparer));
        }

        public int Compare(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
        {
            var comparer = StringComparer;
            var result = comparer.Compare(x.Key, y.Key);

            if (result == 0)
                result = comparer.Compare(x.Value, y.Value);

            return result;
        }
    }
}

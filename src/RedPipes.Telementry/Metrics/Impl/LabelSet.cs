using System;
using System.Collections.Generic;
using System.Linq;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class LabelSet : OpenTelemetry.Metrics.LabelSet, IEquatable<LabelSet>
    {
        private int? _hashCode;
        private static readonly LabelComparer _labelComparer = new LabelComparer(StringComparer.Ordinal);

        /// <summary> List of ll, de-duplicated and sorted by key </summary>
        private readonly KeyValuePair<string, string>[] _labels;

        public static OpenTelemetry.Metrics.LabelSet Create(IEnumerable<KeyValuePair<string, string>> labels)
        {
            if (labels == null)
                return BlankLabelSet;

            var ll = CreateLabels(labels);
            if (ll.Length == 0)
                return BlankLabelSet;

            return new LabelSet(ll);
        }

        private LabelSet(KeyValuePair<string, string>[] ll)
        {
            _labels = ll;
        }

        private static int ComputeHashCode(KeyValuePair<string, string>[] labels)
        {
            var hc = 0;

            foreach (var kv in labels)
                hc = HashCode.Combine(hc, kv.Key.GetHashCode(), kv.Value.GetHashCode());

            return hc;
        }

        private static KeyValuePair<string, string>[] CreateLabels(IEnumerable<KeyValuePair<string, string>> ll)
        {
            var labels = ll.ToArray();
            var discarded = 0;

            // deduplicate and remove invalid keys and null values
            var keyComparer = _labelComparer.StringComparer;
            for (int i = 0; i < labels.Length; i++)
            {
                var key = labels[i].Key;
                if (!IsKeyValid(key) || labels[i].Value == null)
                {
                    labels[i] = new KeyValuePair<string, string>(null, null);
                    discarded++;
                    continue;
                }

                for (int j = i + 1; j < labels.Length; j++)
                {
                    if (keyComparer.Compare(key, labels[j].Key) == 0)
                    {
                        labels[i] = new KeyValuePair<string, string>();
                        discarded++;
                        break;
                    }
                }
            }

            if (discarded > 0)
            {
                var newLabels = new KeyValuePair<string, string>[labels.Length - discarded];
                var j = 0;
                foreach (var label in labels)
                {
                    if (label.Key == null)
                        continue;

                    newLabels[j] = label;
                    j += 1;
                }

                labels = newLabels;
            }

            Array.Sort(labels, _labelComparer);

            return labels;
        }

        public static bool IsKeyValid(string key)
        {
            return !string.IsNullOrWhiteSpace(key);
        } 

        public override IEnumerable<KeyValuePair<string, string>> Labels
        {
            get { return _labels; }
        }

        public bool Equals(LabelSet other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetHashCode() != other.GetHashCode())
                return false;

            return SlowEquals(other);
        }

        public override bool Equals(object obj)
        {
            if (obj is LabelSet mls)
            {
                return Equals(mls);
            }
            else if (obj is OpenTelemetry.Metrics.BlankLabelSet)
            {
                return false;
            }
            else if (obj is OpenTelemetry.Metrics.LabelSet ls)
            {
                return SlowEquals(ls);
            }

            return false;
        }

        private bool SlowEquals(OpenTelemetry.Metrics.LabelSet ls)
        {
            var count = 0;
            foreach (var label in ls.Labels)
            {
                var index = Array.BinarySearch(_labels, label, _labelComparer);
                if (index >= 0)
                {
                    count++;
                    continue;
                }

                return false;
            }

            return _labels.Length == count;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _hashCode ??= ComputeHashCode(_labels);
        }

        public static bool operator ==(LabelSet left, LabelSet right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LabelSet left, LabelSet right)
        {
            return !Equals(left, right);
        }
    }
}

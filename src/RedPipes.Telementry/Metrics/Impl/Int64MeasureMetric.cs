using System.Collections.Generic;
using OpenTelemetry.Metrics;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class Int64MeasureMetric : MeasureMetric<long>
    {
        private readonly InMemoryMeter _inMemoryMeter;
        private readonly string _name;

        public Int64MeasureMetric(InMemoryMeter inMemoryMeter, string name)
        {
            _inMemoryMeter = inMemoryMeter;
            _name = name;
        }

        public override BoundMeasureMetric<long> Bind(OpenTelemetry.Metrics.LabelSet labelSet)
        {
            return new Int64BoundMeasureMetric(_inMemoryMeter, _name, labelSet);
        }

        public override BoundMeasureMetric<long> Bind(IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = _inMemoryMeter.GetLabelSet(labels);
            return Bind(labelSet);
        }
    }
}
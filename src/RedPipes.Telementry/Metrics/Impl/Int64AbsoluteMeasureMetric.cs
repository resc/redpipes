using System.Collections.Generic;
using OpenTelemetry.Metrics;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class Int64AbsoluteMeasureMetric : MeasureMetric<long>
    {
        private readonly InMemoryMeter _inMemoryMeter;
        private readonly string _name;

        public Int64AbsoluteMeasureMetric(InMemoryMeter inMemoryMeter, string name)
        {
            _inMemoryMeter = inMemoryMeter;
            _name = name;
        }

        public override BoundMeasureMetric<long> Bind(OpenTelemetry.Metrics.LabelSet labelSet)
        {
            return new Int64AbsoluteBoundMeasureMetric(_inMemoryMeter, _name, labelSet);
        }

        public override BoundMeasureMetric<long> Bind(IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = _inMemoryMeter.GetLabelSet(labels);
            return Bind(labelSet);
        }
    }
}
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class DoubleCounterMetric : CounterMetric<double>
    {
        public string Name { get; }
        public InMemoryMeter Meter { get; }

        public DoubleCounterMetric(InMemoryMeter meter, string name)
        {
            Name = name;
            Meter = meter;
        }

        public override void Add(in SpanContext context, double value, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter.AddCounterValue(context, value, Name, labelSet);
        }

        public override void Add(in Baggage context, double value, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter.AddCounterValue(context, value, Name, labelSet);
        }

        public override BoundCounterMetric<double> Bind(OpenTelemetry.Metrics.LabelSet labelSet)
        {
            return new DoubleBoundCounterMetric(Meter, Name, labelSet);
        }

        public override void Add(in SpanContext context, double value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            Add(context, value, labelSet);
        }

        public override void Add(in Baggage context, double value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            Add(context, value, labelSet);
        }

        public override BoundCounterMetric<double> Bind(IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            return Bind(labelSet);
        }
    }
}
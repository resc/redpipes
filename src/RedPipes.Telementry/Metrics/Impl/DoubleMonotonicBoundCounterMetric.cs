using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class DoubleMonotonicBoundCounterMetric : BoundCounterMetric<double>
    {
        public string Name { get; }
        public InMemoryMeter Meter { get; }
        public OpenTelemetry.Metrics.LabelSet LabelSet { get; }

        public DoubleMonotonicBoundCounterMetric(InMemoryMeter meter, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter = meter;
            Name = name;
            LabelSet = labelSet;
        }

        public override void Add(in SpanContext context, double value)
        {
            Meter.AddCounterValueMonotonic(context, value, Name, LabelSet);
        }

        public override void Add(in Baggage context, double value)
        {
            Meter.AddCounterValueMonotonic(context, value, Name, LabelSet);
        }
    }
}
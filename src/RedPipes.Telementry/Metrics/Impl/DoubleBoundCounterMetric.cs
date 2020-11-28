using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class DoubleBoundCounterMetric : BoundCounterMetric<double>
    {
        public InMemoryMeter Meter { get; }
        
        public string Name { get; }

        public OpenTelemetry.Metrics.LabelSet LabelSet { get; }

        public DoubleBoundCounterMetric(InMemoryMeter meter, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter = meter;
            Name = name;
            LabelSet = labelSet;
        }

        public override void Add(in SpanContext context, double value)
        {
            Meter.AddCounterValue(context, value, Name, LabelSet);
        }

        public override void Add(in Baggage context, double value)
        {
            Meter.AddCounterValue(context, value, Name, LabelSet);
        }
    }
}
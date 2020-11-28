using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class Int64BoundCounterMetric : BoundCounterMetric<long>
    {
        public InMemoryMeter Meter { get; }
        
        public string Name { get; }

        public OpenTelemetry.Metrics.LabelSet LabelSet { get; }

        public Int64BoundCounterMetric(InMemoryMeter meter, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter = meter;
            Name = name;
            LabelSet = labelSet;
        }

        public override void Add(in SpanContext context, long value)
        {
            Meter.AddCounterValue(context, value, Name, LabelSet);
        }

        public override void Add(in Baggage context, long value)
        {
            Meter.AddCounterValue(context, value, Name, LabelSet);
        }
    }
}

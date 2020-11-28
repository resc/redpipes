using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class Int64AbsoluteBoundMeasureMetric : BoundMeasureMetric<long>
    {
        public InMemoryMeter Meter { get; }
        public string Name { get; }
        public OpenTelemetry.Metrics.LabelSet LabelSet { get; }

        public Int64AbsoluteBoundMeasureMetric(InMemoryMeter meter, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter = meter;
            Name = name;
            LabelSet = labelSet;
        }

        public override void Record(in SpanContext context, long value)
        {
            Meter.RecordInt64AbsoluteMeasure(context, value, Name, LabelSet);
        }

        public override void Record(in Baggage context, long value)
        {
            Meter.RecordInt64AbsoluteMeasure(context, value, Name, LabelSet);
        }
    }
}
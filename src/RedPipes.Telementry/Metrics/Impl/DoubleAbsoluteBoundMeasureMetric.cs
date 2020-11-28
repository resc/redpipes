using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class DoubleAbsoluteBoundMeasureMetric : BoundMeasureMetric<double>
    {
        public InMemoryMeter Meter { get; }
        public string Name { get; }
        public OpenTelemetry.Metrics.LabelSet LabelSet { get; }

        public DoubleAbsoluteBoundMeasureMetric(InMemoryMeter meter, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter = meter;
            Name = name;
            LabelSet = labelSet;
        }

        public override void Record(in SpanContext context, double value)
        {
            Meter.RecordDoubleAbsoluteMeasure(context, value, Name, LabelSet);
        }

        public override void Record(in Baggage context, double value)
        {
            Meter.RecordDoubleAbsoluteMeasure(context, value, Name, LabelSet);
        }
    }
}
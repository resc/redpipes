﻿using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class Int64MonotonicCounterMetric : CounterMetric<long>
    {
        public string Name { get; }
        public InMemoryMeter Meter { get; }

        public Int64MonotonicCounterMetric(InMemoryMeter meter, string name)
        {
            Name = name;
            Meter = meter;
        }

        public override void Add(in SpanContext context, long value, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter.AddCounterValueMonotonic(context, value, Name, labelSet);
        }

        public override void Add(in Baggage context, long value, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            Meter.AddCounterValueMonotonic(context, value, Name, labelSet);
        }

        public override BoundCounterMetric<long> Bind(OpenTelemetry.Metrics.LabelSet labelSet)
        {
            return new Int64BoundMonotonicCounterMetric(Meter, Name, labelSet);
        }

        public override void Add(in SpanContext context, long value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            Add(context, value, labelSet);
        }

        public override void Add(in Baggage context, long value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            Add(context, value, labelSet);
        }

        public override BoundCounterMetric<long> Bind(IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = Meter.GetLabelSet(labels);
            return Bind(labelSet);
        }
    }
}

using System;
using System.Collections.Generic;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class Int64ObserverMetricImpl : Int64ObserverMetric, IObserverMetric
    {
        private readonly MeasureMetric<long> _measure;
        private Action<Int64ObserverMetric> _callback;

        public Int64ObserverMetricImpl(string name, MeasureMetric<long> measure, Action<Int64ObserverMetric> callback)
        {
            _measure = measure;
            _callback = callback;
            Name = name;
        }

        public string Name { get; }

        public void Observe()
        {
            _callback?.Invoke(this);
        }

        public void Dispose()
        {
            _callback = null;
        }

        public override void Observe(long value, OpenTelemetry.Metrics.LabelSet labelset)
        {
            _measure.Record(new SpanContext(), value, labelset);
        }

        public override void Observe(long value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            _measure.Record(new SpanContext(), value, labels);
        }
    }
}

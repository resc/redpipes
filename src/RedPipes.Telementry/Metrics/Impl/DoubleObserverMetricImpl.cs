using System;
using System.Collections.Generic;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    class DoubleObserverMetricImpl : DoubleObserverMetric, IObserverMetric
    {
        private readonly MeasureMetric<double> _measure;
        private Action<DoubleObserverMetric> _callback;

        public DoubleObserverMetricImpl(string name, MeasureMetric<double> measure, Action<DoubleObserverMetric> callback)
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

        public override void Observe(double value, OpenTelemetry.Metrics.LabelSet labelset)
        {
            _measure.Record(new SpanContext(), value, labelset);
        }

        public override void Observe(double value, IEnumerable<KeyValuePair<string, string>> labels)
        {
            _measure.Record(new SpanContext(), value, labels);
        }
    }
}

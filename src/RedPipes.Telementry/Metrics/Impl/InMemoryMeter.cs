using System;
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class InMemoryMeter : Meter
    {

        private readonly string _name;
        private readonly string _version;

        public InMemoryMeter(string name, string version)
        {
            _name = name;
            _version = version;
        }

        public override CounterMetric<long> CreateInt64Counter(string name, bool monotonic = true)
        {
            if (monotonic)
            {
                return new Int64MonotonicCounterMetric(this, name);
            }
            else
            {

                return new Int64CounterMetric(this, name);
            }
        }

        public override CounterMetric<double> CreateDoubleCounter(string name, bool monotonic = true)
        {
            if (monotonic)
            {
                return new DoubleMonotonicCounterMetric(this, name);
            }
            else
            {
                return new DoubleCounterMetric(this, name);
            }
        }

        public override MeasureMetric<long> CreateInt64Measure(string name, bool absolute = true)
        {

            if (absolute)
            {
                return new Int64AbsoluteMeasureMetric(this, name);
            }
            else
            {
                return new Int64MeasureMetric(this, name);
            }
        }

        public override MeasureMetric<double> CreateDoubleMeasure(string name, bool absolute = true)
        {
            if (absolute)
            {
                return new DoubleAbsoluteMeasureMetric(this, name);
            }
            else
            {
                return new DoubleMeasureMetric(this, name);
            }
        }

        public override Int64ObserverMetric CreateInt64Observer(string name, Action<Int64ObserverMetric> callback, bool absolute = true)
        {
            var measure = CreateInt64Measure(name, absolute);
            var observer = new Int64ObserverMetricImpl(name, measure, callback);
            RegisterObserver(observer);
            return observer;
        }

        public override DoubleObserverMetric CreateDoubleObserver(string name, Action<DoubleObserverMetric> callback, bool absolute = true)
        {
            var measure = CreateDoubleMeasure(name, absolute);
            var observer = new DoubleObserverMetricImpl(name, measure, callback);
            RegisterObserver(observer);
            return observer;
        }

        public override OpenTelemetry.Metrics.LabelSet GetLabelSet(IEnumerable<KeyValuePair<string, string>> labels)
        {
            return LabelSet.Create(labels);
        }

        private void RegisterObserver(IObserverMetric observer)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValue(in SpanContext context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValue(in Baggage context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValue(in SpanContext context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValue(in Baggage context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValueMonotonic(in SpanContext context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValueMonotonic(in Baggage context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValueMonotonic(in SpanContext context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void AddCounterValueMonotonic(in Baggage context, in double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordDoubleMeasure(in SpanContext context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordDoubleMeasure(in Baggage context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordDoubleAbsoluteMeasure(in SpanContext context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordDoubleAbsoluteMeasure(in Baggage context, double value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordInt64Measure(in SpanContext context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordInt64Measure(in Baggage context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordInt64AbsoluteMeasure(in SpanContext context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }

        public void RecordInt64AbsoluteMeasure(in Baggage context, long value, string name, OpenTelemetry.Metrics.LabelSet labelSet)
        {
            throw new NotImplementedException();
        }
    }
}

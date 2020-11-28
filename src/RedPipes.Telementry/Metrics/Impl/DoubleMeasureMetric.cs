﻿using System.Collections.Generic;
using OpenTelemetry.Metrics;

namespace RedPipes.Telemetry.Metrics.Impl
{
    public class DoubleMeasureMetric : MeasureMetric<double>
    {
        private readonly InMemoryMeter _inMemoryMeter;
        private readonly string _name;

        public DoubleMeasureMetric(InMemoryMeter inMemoryMeter, string name)
        {
            _inMemoryMeter = inMemoryMeter;
            _name = name;
        }

        public override BoundMeasureMetric<double> Bind(OpenTelemetry.Metrics.LabelSet labelSet)
        {
            return new DoubleBoundMeasureMetric(_inMemoryMeter, _name, labelSet);
        }

        public override BoundMeasureMetric<double> Bind(IEnumerable<KeyValuePair<string, string>> labels)
        {
            var labelSet = _inMemoryMeter.GetLabelSet(labels);
            return Bind(labelSet);
        }
    }
}
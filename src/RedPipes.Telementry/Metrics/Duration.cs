using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Telemetry.Metrics
{
    public static class Duration
    {
        public static IBuilder<T, T> UseDurationMetric<T>([NotNull] this IBuilder<T, T> builder, [NotNull] string name)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return builder.UseDurationMetric(name, LabelSet.BlankLabelSet);
        }

        public static IBuilder<T, T> UseDurationMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] IEnumerable<KeyValuePair<string, string>> labels)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }

            return builder.Use(new Builder<T>(name, labels));
        }

        public static IBuilder<T, T> UseDurationMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] LabelSet labels)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }

            return builder.Use(new Builder<T>(name, labels));
        }

        sealed class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly BoundMeasureMetric<long> _duration;

            public Builder(string name, LabelSet labelSet)
            {
                _duration = Meters.Default.CreateInt64Measure(name).Bind(labelSet);
            }

            public Builder(string name, IEnumerable<KeyValuePair<string, string>> labelSet)
            {
                _duration = Meters.Default.CreateInt64Measure(name).Bind(labelSet);
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(next, _duration);
                return Task.FromResult(pipe);
            }
        }

        sealed class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;
            private readonly BoundMeasureMetric<long> _duration;

            public Pipe(IPipe<T> next, BoundMeasureMetric<long> duration)
            {
                _next = next;
                _duration = duration;
            }

            public async Task Execute(IContext ctx, T value)
            {
                long start = Stopwatch.GetTimestamp();
                try
                {
                    await _next.Execute(ctx, value);
                }
                finally
                {
                    long end = Stopwatch.GetTimestamp();
                    var duration = end - start;
                    _duration.Record(Tracer.CurrentSpan.Context, duration);
                }
            }
            
            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _next, (EdgeLabels.Label, "next")))
                    _next.Accept(visitor);
            }
        }
    }
}

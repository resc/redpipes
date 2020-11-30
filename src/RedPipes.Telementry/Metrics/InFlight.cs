using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Telemetry.Metrics
{
    public static class InFlight
    {
        public static IBuilder<T, T> UseInFlightMetric<T>(this IBuilder<T, T> builder, [NotNull] string name)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var blankLabelSet = OpenTelemetry.Metrics.LabelSet.BlankLabelSet;
            return builder.UseInFlightMetric(name, blankLabelSet);
        }

        public static IBuilder<T, T> UseInFlightMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] IEnumerable<KeyValuePair<string, string>> labels)
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

        public static IBuilder<T, T> UseInFlightMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] OpenTelemetry.Metrics.LabelSet labels)
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
            private readonly BoundCounterMetric<long> _inflight;

            public Builder(string name, OpenTelemetry.Metrics.LabelSet labelSet)
            {
                _inflight = Meters.Default.CreateInt64Counter(name, false).Bind(labelSet);
            }

            public Builder(string name, IEnumerable<KeyValuePair<string, string>> labelSet)
            {
                _inflight = Meters.Default.CreateInt64Counter(name, false).Bind(labelSet);
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(next, _inflight);
                return Task.FromResult(pipe);
            }
        }

        sealed class Pipe<T> : IPipe<T>
        {
            private readonly IPipe<T> _next;
            private readonly BoundCounterMetric<long> _inflight;

            public Pipe(IPipe<T> next, BoundCounterMetric<long> inflight)
            {
                _next = next;
                _inflight = inflight;
            }

            public async Task Execute(IContext ctx, T value)
            {
                try
                {
                    _inflight.Add(Tracer.CurrentSpan.Context, 1);
                    await _next.Execute(ctx, value);
                }
                finally
                {
                    _inflight.Add(Tracer.CurrentSpan.Context, -1);
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

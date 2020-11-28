using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RedPipes.Configuration;

namespace RedPipes.Telemetry.Metrics
{
    public static class Count
    {
        public static IBuilder<T, T> UseCountMetric<T>(this IBuilder<T, T> builder, [NotNull] string name)
        {
            return builder.UseCountMetric(name, LabelSet.BlankLabelSet);
        }

        public static IBuilder<T, T> UseCountMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] IEnumerable<KeyValuePair<string, string>> labels)
        {
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

        public static IBuilder<T, T> UseCountMetric<T>(this IBuilder<T, T> builder, [NotNull] string name, [NotNull] LabelSet labels)
        {
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
            private readonly BoundCounterMetric<long> _duration;

            public Builder(string name, LabelSet labelSet)
            {
                _duration = Meters.Default.CreateInt64Counter(name).Bind(labelSet);
            }

            public Builder(string name, IEnumerable<KeyValuePair<string, string>> labelSet)
            {
                _duration = Meters.Default.CreateInt64Counter(name).Bind(labelSet);
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
            private readonly BoundCounterMetric<long> _duration;

            public Pipe(IPipe<T> next, BoundCounterMetric<long> duration)
            {
                _next = next;
                _duration = duration;
            }

            public async Task Execute(IContext ctx, T value)
            {
                try
                {
                    await _next.Execute(ctx, value);
                }
                finally
                {
                    _duration.Add(Tracer.CurrentSpan.Context, 1);
                }
            }
            public IEnumerable<(string, IPipe)> Next()
            {
                yield return (nameof(_next), _next);
            }
        }
    }
}

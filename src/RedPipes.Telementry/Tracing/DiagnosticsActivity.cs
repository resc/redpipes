using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenTelemetry.Trace;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Telemetry.Tracing
{
    public static class DiagnosticsActivity
    {
        private static readonly object _activityContextKey = Context.NewKey(nameof(ActivityContext));

        public static bool TryGetActivityContext(this IContext ctx, out ActivityContext activity)
        {
            return ctx.TryGetValue(_activityContextKey, out activity);
        }

        public static IContext WithActivityContext(this IContext ctx, ActivityContext activity)
        {
            return ctx.With(_activityContextKey, activity);
        }

        public static IContext WithoutActivityContext(this IContext ctx)
        {
            return ctx.Without(_activityContextKey);
        }

        public static IBuilder<T, T> UseDiagnosticsActivity<T>(this IBuilder<T, T> builder, [NotNull] string activityName, ActivityKind kind = ActivityKind.Internal, [CanBeNull] ActivitySource source = null, [CanBeNull] Func<IContext, T, IEnumerable<KeyValuePair<string, object>>> getTags = null)
        {
            if (activityName == null)
            {
                throw new ArgumentNullException(nameof(activityName));
            }

            if (source == null)
            {
                source = ActivitySources.Default;
            }

            return builder.Use(new Builder<T>(activityName, kind, source, getTags));
        }

        sealed class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly string _name;
            private readonly ActivityKind _kind;
            private readonly ActivitySource _source;
            private readonly Func<IContext, T, IEnumerable<KeyValuePair<string, object>>> _getTags;

            public Builder(string name, ActivityKind kind, ActivitySource source, Func<IContext, T, IEnumerable<KeyValuePair<string, object>>> getTags)
            {
                _name = name;
                _kind = kind;
                _source = source;
                _getTags = getTags;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(_name, _kind, _source, _getTags, next);
                return Task.FromResult(pipe);
            }
        }

        sealed class Pipe<T> : IPipe<T>
        {
            private readonly string _name;
            private readonly ActivityKind _kind;
            private readonly ActivitySource _source;
            private readonly IPipe<T> _next;
            private readonly Func<IContext, T, IEnumerable<KeyValuePair<string, object>>> _getTags;

            public Pipe(string name, ActivityKind kind, ActivitySource source, Func<IContext, T, IEnumerable<KeyValuePair<string, object>>> getTags, IPipe<T> next)
            {
                _name = name;
                _kind = kind;
                _source = source;
                _getTags = getTags;
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                ctx.TryGetActivityContext(out ActivityContext parentContext);
                var tags = _getTags?.Invoke(ctx, value);
                using (var activity = _source.StartActivity(_name, _kind, parentContext, tags))
                {
                    try
                    {
                        if (activity != null)
                            ctx = ctx.WithActivityContext(activity.Context);

                        await _next.Execute(ctx, value);
                    }
                    catch (Exception ex)
                    {
                        activity?.RecordException(ex);
                        throw;
                    }
                }
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                visitor.GetOrAddNode(this
                    , (Keys.Name, $"Trace activity '{_name}' ({_kind})")
                    , ("DiagnosticsName", _name)
                    , ("DiagnosticsKind", _kind)
                    , ("DiagnosticsSource", $"{_source.Name} {_source.Version}"));

                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                {
                    _next.Accept(visitor);
                }
            }
        }
    }
}

using System;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace RedPipes
{
    public static partial class Context
    {
        /// <summary> Sets a timeout on the <see cref="IContext.Token"/> with the given <paramref name="delay"/>, any parent context cancellation is propagated </summary>
        public static IContext WithTimeout(this IContext ctx, TimeSpan delay)
        {
            EnsureContextNotNull(ctx);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.Token);
            cts.CancelAfter(delay);
            return new CancellationTokenSourceContext(ctx, cts, delay);
        }

        /// <summary> Creates a context with a cancellation </summary>
        public static (IContext ctx, Action cancel) WithCancel(this IContext ctx)
        {
            EnsureContextNotNull(ctx);

            var cts = new CancellationTokenSource();
            var d = Disposable.Create(cts.Cancel);
            return (new CancellationTokenSourceContext(ctx, cts, TimeSpan.Zero), d.Dispose);
        }

        /// <summary> Sets a timeout on the <see cref="IContext.Token"/> with the given <paramref name="millisecondsDelay"/>, any parent context cancellation is propagated </summary>
        public static IContext WithTimeout(this IContext ctx, int millisecondsDelay)
        {
            return ctx.WithTimeout(TimeSpan.FromMilliseconds(millisecondsDelay));
        }

        /// <summary> Creates a context with a <see cref="CancellationTokenSource"/> that supplies the <see cref="IContext.Token"/> value </summary>
        public static IContext WithCancellationTokenSource(this IContext ctx, [NotNull] CancellationTokenSource cts)
        {
            EnsureContextNotNull(ctx);
            EnsureCancellationTokenSourceNotNull(cts);

            return new CancellationTokenSourceContext(ctx, cts, TimeSpan.Zero);
        }

        private static void EnsureCancellationTokenSourceNotNull(CancellationTokenSource? cts)
        {
            if (cts == null)
                ThrowArgumentNullException(nameof(cts));
        }

        private class CancellationTokenSourceContext : ContextBase, IDisposable
        {
            [NotNull]
            private readonly CancellationTokenSource _cts;

            private readonly TimeSpan _timeout;

            public CancellationTokenSourceContext(IContext inner, CancellationTokenSource cts, TimeSpan timeout) : base(inner)
            {
                _cts = cts;
                _timeout = timeout;
            }

            public override CancellationToken Token
            {
                get { return _cts.Token; }
            }

            public void Dispose()
            {
                _cts.Dispose();
            }

            public override void ToString(StringBuilder sb, int level)
            {
                base.ToString(sb, level);

                if (_timeout > TimeSpan.Zero)
                    sb.Append($" (timeout after {_timeout:g})");
            }
        }
    }
}

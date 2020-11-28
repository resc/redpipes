using System;
using System.Threading;

namespace RedPipes
{
    /// <summary> Creates a <see cref="IDisposable"/> from an <see cref="Action"/>,
    /// the action always called exactly once, regardless of how many times <see cref="Dispose"/> is called </summary>
    public class Disposable : IDisposable
    { 
        /// <summary> Empty no-op <see cref="IDisposable"/> implementation </summary>
        public static IDisposable Empty { get; } = new Disposable(null);

        /// <summary> Creates a <see cref="IDisposable"/> from an <see cref="Action"/>,
        /// the <paramref name="action"/> always called exactly once, regardless of how many times <see cref="Dispose"/> is called.
        /// <see cref="Create"/> can safely be called with a null <paramref name="action"/> and will return a no-op <see cref="IDisposable"/>.</summary>
        public static IDisposable Create(Action action)
        {
            if (action == null) return Empty;
            return new Disposable(action);
        }

        private Action _action;
        
        private Disposable(Action action)
        {
            _action = action;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_action == null) return;

            var action = Interlocked.Exchange(ref _action, null);

            action?.Invoke();
        }
    }
}

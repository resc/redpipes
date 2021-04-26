using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedPipes
{
    /// <summary> Creates a <see cref="IDisposable"/> from an <see cref="Action"/>,
    /// the action always called exactly once, regardless of how many times <see cref="Dispose"/> is called </summary>
    public static class Disposable
    {
        /// <summary> Empty no-op <see cref="IDisposable"/> implementation </summary>
        public static IDisposable Empty { get; } = Once.Empty;

        /// <summary> Creates a <see cref="IDisposable"/> from an <see cref="Action"/>,
        /// the <paramref name="action"/> always called exactly once, regardless of how many times <see cref="Dispose"/> is called.
        /// <see cref="Create"/> can safely be called with a null <paramref name="action"/> and will return a no-op <see cref="IDisposable"/>.</summary>
        public static IDisposable Create(Action? action)
        {
            return Once.Create(action);
        }
    }

    /// <summary> Utility class wrap an action to ensure it's only called once. Also implements <see cref="IDisposable"/> so you can use it in using blocks </summary>
    public sealed class Once : IDisposable
    {
        /// <summary> Empty no-op <see cref="Once"/> implementation </summary>
        public static Once Empty { get; } = new Once(null);

        /// <summary> Creates a <see cref="Once"/> instance from an <see cref="Action"/>,
        /// the <paramref name="action"/> always called exactly once, regardless of how many times <see cref="Invoke"/> is called.
        /// <see cref="Create(Action)"/> can safely be called with a null <paramref name="action"/> and will return a no-op <see cref="Once"/>.</summary>
        public static Once Create(Action? action)
        {
            if (action == null) return Empty;
            return new Once(action);
        }

        /// <summary> Creates a <see cref="OnceAsync"/> instance from an <see cref="Func{Task}"/>,
        /// the <paramref name="action"/> always called exactly once, regardless of how many times <see cref="OnceAsync.InvokeAsync"/> is called.
        /// <see cref="CreateAsync(Func{Task})"/> can safely be called with a null <paramref name="action"/> and will return a no-op <see cref="OnceAsync"/>.</summary>
        public static OnceAsync CreateAsync(Func<Task>? action)
        {
            return OnceAsync.Create(action);
        }

        private Action? _action;

        private Once(Action? action)
        {
            _action = action;
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            Invoke();
        }

        /// <summary> Invokes the action, second and subsequent times are no-ops. </summary>
        public void Invoke()
        {
            if (_action == null) return;

            var action = Interlocked.Exchange(ref _action, default);

            action?.Invoke();
        }


    }

    /// <summary> Async variant of <see cref="Once"/> </summary>
    public class OnceAsync : IAsyncDisposable
    {
        /// <summary> Empty no-op <see cref="OnceAsync"/> implementation </summary>
        public static OnceAsync Empty { get; } = new OnceAsync(null);

        /// <summary> Creates a <see cref="OnceAsync"/> instance from an <see cref="Func{Task}"/>,
        /// the <paramref name="action"/> always called exactly once, regardless of how many times <see cref="OnceAsync.InvokeAsync"/> is called.
        /// <see cref="Create(Func{Task})"/> can safely be called with a null <paramref name="action"/> and will return a no-op <see cref="OnceAsync"/>.</summary>
        public static OnceAsync Create(Func<Task>? action)
        {
            if (action == null) return OnceAsync.Empty;
            return new OnceAsync(action);
        }

        private Func<Task>? _asyncAction;

        /// <summary>  </summary>
        public OnceAsync(Func<Task>? asyncAction)
        {
            _asyncAction = asyncAction;
        }

        /// <summary> Invokes the action, second and subsequent times are no-ops. </summary>
        public async Task InvokeAsync()
        {
            if (_asyncAction == null) return;

            var asyncAction = Interlocked.Exchange(ref _asyncAction, default);
            if (asyncAction != null)
                await asyncAction();
        }

        /// <inheritdoc />
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await InvokeAsync();
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace RedPipes
{
    public static partial class Context
    {
        /// <summary> The empty, immutable background context </summary>
        public static IContext Background { get; } = new BackgroundContext();

        private sealed class BackgroundContext : IContext
        {
            public CancellationToken Token
            {
                get { return CancellationToken.None; }
            }

            public bool TryGetValue<T>(object key, [MaybeNullWhen(false)] out T value)
            {
                value = default;
                return false;
            }

            public override string ToString()
            {
                return nameof(BackgroundContext);
            }
        }
    }
}

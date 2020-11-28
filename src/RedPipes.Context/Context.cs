﻿using System.Threading;

namespace RedPipes
{
    public static partial class Context
    {
        public static IContext Background { get; } = new BackgroundContext();

        private sealed class BackgroundContext : IContext
        {
            public CancellationToken Token
            {
                get { return CancellationToken.None; }
            } 

            public bool TryGetValue<T>(object key, out T value)
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
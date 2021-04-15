using System;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace RedPipes
{
    /// <summary> Entry point for IContext related extension methods. </summary>
    public static partial class Context
    {
        private static void EnsureContextNotNull(IContext? ctx)
        {
            if (ctx == null) ThrowArgumentNullException(nameof(ctx));
        }

        private static void EnsureKeyNotNull(object? key)
        {
            if (key == null) ThrowArgumentNullException(nameof(key));
        }

        private static void ThrowArgumentNullException(string name)
        {
            throw new ArgumentNullException(name);
        }

        private abstract class ContextBase : IContext
        {
            [NotNull]
            protected readonly IContext Inner;

            protected ContextBase(IContext inner)
            {
                Inner = inner;
            }

            public virtual CancellationToken Token
            {
                get { return Inner.Token; }
            }

            public virtual bool TryGetValue<T>(object key, out T value)
            {
                return Inner.TryGetValue(key, out value);
            }

            public override string ToString()
            {
                int level = 0;
                var sb = new StringBuilder();
                IContext ctx = this;
                while (ctx is ContextBase cb)
                {
                    sb.AppendPrefix(level);
                    cb.ToString(sb, level);
                    sb.AppendLine();
                    ctx = cb.Inner;
                    level++;
                }

                sb.AppendPrefix(level).AppendLine(ctx.ToString());


                return sb.ToString();
            }

            public virtual void ToString(StringBuilder sb, int level)
            {
                sb.Append(GetType().Name);
            }
        }

        private static StringBuilder AppendPrefix(this StringBuilder sb, int level)
        {
            if (level < 1) return sb;
            return sb.Append(' ', level * 2);
        }
    }
}

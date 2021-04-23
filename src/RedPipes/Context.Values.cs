using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RedPipes
{
    public static partial class Context
    {
        /// <summary> Adds the given <paramref name="value"/> with the given <paramref name="key"/> to the context </summary>
        public static IContext With<T>(this IContext ctx, object key, T value)
        {
            EnsureContextNotNull(ctx);
            EnsureKeyNotNull(key);
            return new ValueContext<T>(ctx, key, value);
        }

        /// <summary> Removes the value stored under the given <paramref name="key"/> from the context </summary>
        public static IContext Without(this IContext ctx, object key)
        {
            EnsureContextNotNull(ctx);
            EnsureKeyNotNull(key);
            return new NoValueContext(ctx, key);
        }

        private class NoValueContext : ContextBase
        {
            private readonly object _key;

            public NoValueContext(IContext inner, object key) : base(inner)
            {
                _key = key;
            }

            public override bool TryGetValue<T>(object key, out T value)
            {
                if (!ReferenceEquals(_key, key))
                    return Inner.TryGetValue(key, out value);

                value = default!;
                return false;
            }

            public override void ToString(StringBuilder sb, int level)
            {
                sb.Append("Key: ").Append(_key).Append(" (removed)");
            }
        }

        private class ValueContext<TValue> : ContextBase
        {
            private readonly object _key;
            private readonly TValue _value;

            public ValueContext(IContext inner, object key, TValue value) : base(inner)
            {
                _key = key;
                _value = value;
            }

            public override bool TryGetValue<T>(object key, out T value)
            {
                value = default!;

                if (!ReferenceEquals(_key, key))
                    return Inner.TryGetValue(key, out value);

                if (typeof(TValue) != typeof(T))
                    return false;

                if (!(_value is T v))
                    return false;

                value = v;
                return true;

            }

            public override void ToString(StringBuilder sb, int level)
            {
                sb.Append("Key: ").Append(_key).Append(", Value: ").Append(_value);
            }
        }
    }
}

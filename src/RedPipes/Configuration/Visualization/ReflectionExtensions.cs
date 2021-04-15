using System;
using System.Linq;
using System.Text;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> Some .NET Type extensions </summary>
    public static class ReflectionExtensions
    {

        /// <summary> Converts the given <paramref name="t"/> into its corresponding C# type name</summary>
        public static string GetCSharpName(this Type t)
        {
            var n = t.Name;
            if (t.IsGenericType)
            {
                var i = n.IndexOf("`", StringComparison.Ordinal);
                if (i >= 0)
                    n = n.Substring(0, i);

                n = $"{n}<{string.Join(",", t.GetGenericArguments().Select(t => t.GetCSharpName()))}>";
            }

            if (t.IsNested && t.DeclaringType != null)
                n = $"{t.DeclaringType.GetCSharpName()}.{n}";

            return n;
        }
    }
}

using System;
using System.Linq;
using System.Text;

namespace RedPipes.Configuration.Visualization
{
    public static class ReflectionExtensions
    {


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

            if (t.IsNested)
                n = $"{t.DeclaringType.GetCSharpName()}.{n}";

            return n;
        }
    }
}

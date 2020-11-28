using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace RedPipes.Introspection
{
    public static class Extensions
    {

        public static string DumpPipeStructure(this IPipe pipe)
        {
            var scope = new Scope();
            DumpPipeStructure(pipe, scope);
            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,// just so that < and > don't get encoded...
            };
            return JsonSerializer.Serialize(scope, opts);
        }

       
        private static void DumpPipeStructure(IPipe pipe, IScope scope)
        {
            // add system defined attrs last, so we overwrite them.
            scope.Attr("__type", pipe.GetType().CSharpName());

            if (pipe is IInspectable inspectable)
                inspectable.Inspect(scope);
            
            foreach (var (childName, child) in pipe.Next())
                DumpPipeStructure(child, scope.Scope(childName));

        }

        private static string CSharpName(this Type t, bool includeNamespaces = false)
        {
            var name = (includeNamespaces ? t.FullName : t.Name) ?? "";
            if (t.IsNested)
            {
                // add the declaring type name to the type name.
                name = t.DeclaringType.CSharpName(includeNamespaces) + "." + name;
            }

            if (t.IsGenericType)
            {
                var i = name.IndexOf('`');
                if (i >= 0)
                    name = name.Substring(0, i);
                name = name + "<" + string.Join(", ", t.GetGenericArguments().Select(ga => ga.CSharpName(includeNamespaces))) + ">";
                return name;
            }
            else
            {
                return name;
            }
        }
    }
}

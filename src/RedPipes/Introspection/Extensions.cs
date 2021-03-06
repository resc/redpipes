﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Introspection
{
    /// <summary> introspection extensions </summary>
    public static class Extensions
    {
        /// <summary> Renders the pipe structure as a JSON string. </summary>
        public static string DumpPipeStructure(this IPipe pipe)
        {
            var scope = new Scope();

            var g = new DgmlGraph<IPipe>();
            pipe.Accept(g);

            var rootNode = g.GetOrAddNode(pipe);
            
            DumpPipeStructure(rootNode, scope, new HashSet<INode>());
            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,// just so that < and > don't get encoded...
            };
            return JsonSerializer.Serialize(scope, opts);
        }

        private static void DumpPipeStructure(INode  n, IScope scope, HashSet<INode > visited)
        {
            if (!visited.Add(n))
            {
                return;
            }

            // add system defined attrs last, so we overwrite them.
            foreach (var kv in n.Labels)
                scope.Attr(kv.Key, kv.Value);

            if (n.Item is IInspectable inspectable)
                inspectable.Inspect(scope);

            scope.Attr("__type", n.Item.GetType().CSharpName());


            foreach (var e in n.OutEdges.OrderBy(x => x.Id))
            {
                var name = e.Labels.GetValueOrDefault(Keys.Name, "Next").ToString() ?? "";
                DumpPipeStructure(e.Target, scope.Scope(name) ,visited);
            }

        }

        private static string CSharpName(this Type t, bool includeNamespaces = false)
        {
            var name = (includeNamespaces ? t.FullName : t.Name) ?? "";
            if (t.IsNested)
            {
                // add the declaring type name to the type name.
                var declaringTypeName = t.DeclaringType?.CSharpName(includeNamespaces);
                if (declaringTypeName != null)
                    name = declaringTypeName + "." + name;
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

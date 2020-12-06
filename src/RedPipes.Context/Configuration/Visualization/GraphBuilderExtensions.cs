using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RedPipes.Configuration.Visualization
{
    public static class GraphBuilderExtensions
    {
        public static bool AddEdge<T>(this IGraphBuilder<T> visitor, T source, T target, params (string Label, object Value)[] labels)
        {
            Dictionary<string, object> dict = null;
            if (labels != null && labels.Length > 0)
                dict = labels.ToDictionary(x => x.Label, x => x.Value);

            return visitor.AddEdge(source, target, dict);
        }

        public static INode GetOrAddNode<T>(this IGraphBuilder<T> visitor, T item, params (string Label, object Value)[] labels)
        {
            var node = visitor.GetOrAddNode(item);
            if (labels != null && labels.Length > 0)
            {
                foreach (var l in labels)
                    node.Labels[l.Label] = l.Value;
            }
            return node;
        }


        public static void SaveDgmlAsTempFileAndOpen(this IPipe pipe)
        {
            var graph = new DgmlGraph<IPipe>();
            pipe.Accept(graph);

            SaveDgmlAsTempFileAndOpen(graph);
        }

        public static void SaveDgmlAsTempFileAndOpen<T>(this DgmlGraph<T> graph) where T : class
        {
            var tmpFile = Path.GetTempFileName();
            tmpFile += ".dgml";
            graph.GetDgmlDocument().Save(tmpFile);
            Process.Start(@"cmd.exe ", $"/c \"{tmpFile}\"")?.Dispose();
        }
    }


}

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> Extensions for <see cref="IGraphBuilder{T}"/> </summary>
    public static class GraphBuilderExtensions
    {
        /// <summary> adds an edge from <paramref name="source"/> to <paramref name="target"/> with optional <paramref name="labels"/> </summary>
        public static bool AddEdge<T>(this IGraphBuilder<T> visitor, T source, T target, params (string Label, object Value)[] labels)
        {
            Dictionary<string, object>? dict = null;
            if (  labels.Length > 0)
                dict = labels.ToDictionary(x => x.Label, x => x.Value);

            return visitor.AddEdge(source, target, dict);
        }

        /// <summary> Gets or adds a node for the given <paramref name="item"/> with optional <paramref name="labels"/> </summary>
        public static INode GetOrAddNode<T>(this IGraphBuilder<T> visitor, T item, params (string key, object value)[] labels)
        {
            var node = visitor.GetOrAddNode(item);
            if (labels.Length > 0)
            {
                foreach (var (label, value) in labels)
                    node.Labels[label] = value;
            }
            return node;
        }

        /// <summary> Saves the <paramref name="pipe"/> structure as dgml to a temp file,
        /// and attempts to open it with the system viewer for that file type (usually Visual Studio)  </summary>
        public static void SaveDgmlAsTempFileAndOpen(this IPipe pipe)
        {
            var graph = new DgmlGraph<IPipe>();
            pipe.Accept(graph);

            SaveDgmlAsTempFileAndOpen(graph);
        }

        /// <summary> Saves the <paramref name="graph"/> structure as dgml to a temp file,
        /// and attempts to open it with the system viewer for that file type (usually Visual Studio)  </summary>
        public static void SaveDgmlAsTempFileAndOpen<T>(this DgmlGraph<T> graph) where T : class
        {
            var tmpFile = Path.GetTempFileName();
            tmpFile += ".dgml";
            graph.GetDgmlDocument().Save(tmpFile);
            Process.Start(@"cmd.exe ", $"/c \"{tmpFile}\"")?.Dispose();
        }
    }


}

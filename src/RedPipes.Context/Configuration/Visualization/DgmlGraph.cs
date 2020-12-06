using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace RedPipes.Configuration.Visualization
{
    public class DgmlGraph<T> : Graph<T> where T : class
    {
        public XDocument GetDgmlDocument()
        {
            var doc = XDocument.Parse(DgmlTemplate);
            var xNodes = doc.Descendants(XName.Get("Nodes", DgmlNamespace)).First();
            var xLinks = doc.Descendants(XName.Get("Links", DgmlNamespace)).First();
            var xProps = doc.Descendants(XName.Get("Properties", DgmlNamespace)).First();
            var nodes = Nodes.OrderBy(x => x.Id).ToList();
            var nodeProperties = nodes.SelectMany(n => n.Labels.Keys).Distinct().ToList();
            var edgeProperties = nodes.SelectMany(n => n.OutEdges.SelectMany(e => e.Labels.Keys)).Distinct().ToList();
            xProps.Add(nodeProperties.Select(n => ToXProperty("Node", n)).ToArray());
            xProps.Add(edgeProperties.Select(n => ToXProperty("Edge", n)).ToArray());
            xNodes.Add(nodes.Select(ToXNode).ToArray());
            xLinks.Add(nodes.SelectMany(ToXLinks).ToArray());

            return doc;
        }

        private object ToXProperty(string prefix, string name)
        {
            return new XElement(XName.Get("Property", DgmlNamespace),
                new XAttribute(XName.Get("Id"), prefix + name),
                new XAttribute(XName.Get("Label"), name),
                new XAttribute(XName.Get("DataType"), typeof(string).FullName ?? "")
            );
        }

        private IEnumerable<object> ToXLinks(INode arg)
        {
            foreach (var e in arg.OutEdges.OrderBy(e => e.Id))
            {
                var attrs =
                    new[] {
                            new XAttribute(XName.Get("Source"), e.Source.Id),
                            new XAttribute(XName.Get("Target"), e.Target.Id),
                            new XAttribute(XName.Get("Label"), e.Labels.GetValueOrDefault(Keys.Name,"Next"))
                        }
                        .Concat(arg.Labels.Select(kv => new XAttribute(XName.Get("Edge" + kv.Key), kv.Value.ToString())))
                        .Cast<object>()
                        .ToArray();

                yield return new XElement(XName.Get("Link", DgmlNamespace), attrs);


            }
        }

        private object ToXNode(INode n)
        {
            var attrs =
                new[]
                    {
                        new XAttribute(XName.Get("Id"), n.Id),
                        new XAttribute(XName.Get("Label"),n.Labels.GetValueOrDefault(Keys.Name, n.Item.GetType().GetCSharpName())),
                    }
                    .Concat(n.Labels.Select(kv => new XAttribute(XName.Get("Node" + kv.Key), kv.Value.ToString())))
                    .Cast<object>()
                    .ToArray();

            return new XElement(XName.Get("Node", DgmlNamespace), attrs);
        }


        private const string DgmlNamespace = "http://schemas.microsoft.com/vs/2009/dgml";

        private static readonly string DgmlTemplate = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<DirectedGraph Title=""Pipe Layout"" xmlns=""{DgmlNamespace}"">
   <Nodes/>
   <Links/>
   <Categories/>
   <Properties/>
</DirectedGraph>";
    }
}

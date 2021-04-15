using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace RedPipes.Configuration.Visualization
{
    /// <summary> a directed graph implementation </summary>
    public class Graph<T> : IGraphBuilder<T> where T : class
    {
        #region Labeled

        class Labeled
        {
            public int Id { get; }

            public string Name
            {
                get
                {
                    Labels.TryGetValue(Keys.Name, out var label);
                    return label?.ToString() ?? $"{GetType().Name}  #{Id}";
                }
            }

            public IDictionary<string, object> Labels { get; }

            public Labeled(int id, IDictionary<string, object>? labels)
            {
                Id = id;
                Labels = new SortedDictionary<string, object>(StringComparer.Ordinal);
                AddLabels(labels);
            }

            public void AddLabels(IDictionary<string, object>? labels)
            {
                if (labels == null) return;
                foreach (var kv in labels)
                    Labels[kv.Key] = kv.Value;
            }

        }

        #endregion

        #region Node

        class Node : Labeled, INode
        {
            public Node(int id, [NotNull] object item) : base(id, null)
            {
                Item = item ?? throw new ArgumentNullException(nameof(item));
                OutEdges = new HashSet<IEdge>();
                InEdges = new HashSet<IEdge>();
            }

            public object Item { get; }

            public ISet<IEdge> OutEdges { get; }
            public ISet<IEdge> InEdges { get; }
        }

        #endregion

        #region Edge

        class Edge : Labeled, IEquatable<Edge>, IEdge
        {
            public INode Source { get; }
            public INode Target { get; }

            public Edge(int id, [NotNull] INode source, [NotNull] INode target, IDictionary<string, object>? labels) : base(id, labels)
            {
                Source = source ?? throw new ArgumentNullException(nameof(source));
                Target = target ?? throw new ArgumentNullException(nameof(target));
            }


            public void Remove()
            {
                Source.OutEdges.Remove(this);
                Target.InEdges.Remove(this);
            }

            public bool Equals(Edge other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Equals(Source, other.Source) && Equals(Target, other.Target);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((Edge)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
                }
            }

            public static bool operator ==(Edge left, Edge right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Edge left, Edge right)
            {
                return !Equals(left, right);
            }
        }

        #endregion


        private int _nextId;

        private readonly Dictionary<T, INode> _nodes;

        /// <summary> Initializes a new graph instance </summary>
        protected Graph()
        {
            _nodes = new Dictionary<T, INode>(ReferenceEqualityComparer<T>.Default);
        }

        /// <summary> the list of nodes in this graph </summary>
        public IReadOnlyCollection<INode> Nodes
        {
            get { return _nodes.Values; }
        }

        /// <summary> gets or adds a node that represents <paramref name="item"/> in this graph </summary>
        public INode GetOrAddNode(T item)
        {
            if (!_nodes.TryGetValue(item, out var node))
            {
                node = new Node(NextId(), item);
                _nodes[item] = node;
            }

            return node;
        }

        /// <summary> Adds an edge from <paramref name="source"/> to <paramref name="target"/>, with optional <paramref name="labels"/> </summary>
        public virtual bool AddEdge(T source, T target, IDictionary<string, object>? labels = null)
        {
            var src = GetOrAddNode(source);
            var tgt = GetOrAddNode(target);
            var edge = new Edge(NextId(), src, tgt, labels);
            return src.OutEdges.Add(edge) && tgt.InEdges.Add(edge);
        }

        private int NextId()
        {
            return Interlocked.Increment(ref _nextId);


        }
    }

    #region ReferenceEqualityComparer

    class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static IEqualityComparer<T> Default { get; } = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer() { }

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }

    #endregion

}

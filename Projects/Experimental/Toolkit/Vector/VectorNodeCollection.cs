using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a modifiable collection of control nodes owned by a <see cref="VectorSegment"/>.
    /// </summary>
    public sealed class VectorNodeCollection : IList<VectorNode>, IReadOnlyList<VectorNode>
    {
        private readonly List<VectorNode> _nodes = [];
        private readonly VectorSegment _segment;

        internal VectorNodeCollection(VectorSegment segment)
        {
            _segment = segment ?? throw new ArgumentNullException(nameof(segment));
        }

        public VectorNode this[int index]
        {
            get => _nodes[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                VectorNode current = _nodes[index];

                if (ReferenceEquals(current, value))
                    return;

                EnsureCanAttach(value);
                current.Segment = null;
                value.Segment = _segment;
                _nodes[index] = value;
            }
        }

        public int Count => _nodes.Count;
        public bool IsReadOnly => false;

        public void Add(VectorNode item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);
            item.Segment = _segment;
            _nodes.Add(item);
        }

        /// <summary>
        /// Adds the specified items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<VectorNode> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var range = new List<VectorNode>(items);

            for (int i = 0; i < range.Count; i++)
            {
                ArgumentNullException.ThrowIfNull(range[i]);
                EnsureCanAttach(range[i]);
            }

            for (int i = 0; i < range.Count; i++)
                Add(range[i]);
        }

        public void Clear()
        {
            for (int i = 0; i < _nodes.Count; i++)
                _nodes[i].Segment = null;

            _nodes.Clear();
        }

        public bool Contains(VectorNode item) => _nodes.Contains(item);
        public void CopyTo(VectorNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);
        public IEnumerator<VectorNode> GetEnumerator() => _nodes.GetEnumerator();
        public int IndexOf(VectorNode item) => _nodes.IndexOf(item);

        public void Insert(int index, VectorNode item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);
            item.Segment = _segment;
            _nodes.Insert(index, item);
        }

        public bool Remove(VectorNode item)
        {
            if (!_nodes.Remove(item))
                return false;

            item.Segment = null;
            return true;
        }

        public void RemoveAt(int index)
        {
            VectorNode item = _nodes[index];
            _nodes.RemoveAt(index);
            item.Segment = null;
        }

        public void Reverse() => _nodes.Reverse();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void EnsureCanAttach(VectorNode node)
        {
            if (node.Segment != null)
                throw new InvalidOperationException("The vector node already belongs to a vector segment.");
        }
    }
}

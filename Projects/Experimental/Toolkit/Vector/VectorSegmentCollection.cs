using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents the collection of vector segments owned by a <see cref="VectorPath"/>.
    /// </summary>
    public sealed class VectorSegmentCollection : IList<VectorSegment>, IReadOnlyList<VectorSegment>
    {
        private readonly List<VectorSegment> _segments = [];
        private readonly VectorPath _path;

        internal VectorSegmentCollection(VectorPath path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public VectorSegment this[int index]
        {
            get => _segments[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                VectorSegment current = _segments[index];

                if (ReferenceEquals(current, value))
                    return;

                EnsureCanAttach(value);
                current.Path = null;
                value.Path = _path;
                _segments[index] = value;
            }
        }

        public int Count => _segments.Count;
        public bool IsReadOnly => false;

        public void Add(VectorSegment item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);
            item.Path = _path;
            _segments.Add(item);
        }

        /// <summary>
        /// Adds the specified items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<VectorSegment> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var range = new List<VectorSegment>(items);

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
            for (int i = 0; i < _segments.Count; i++)
                _segments[i].Path = null;

            _segments.Clear();
        }

        public bool Contains(VectorSegment item) => _segments.Contains(item);
        public void CopyTo(VectorSegment[] array, int arrayIndex) => _segments.CopyTo(array, arrayIndex);
        public IEnumerator<VectorSegment> GetEnumerator() => _segments.GetEnumerator();
        public int IndexOf(VectorSegment item) => _segments.IndexOf(item);

        public void Insert(int index, VectorSegment item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);
            item.Path = _path;
            _segments.Insert(index, item);
        }

        public bool Remove(VectorSegment item)
        {
            if (!_segments.Remove(item))
                return false;

            item.Path = null;
            return true;
        }

        public void RemoveAt(int index)
        {
            VectorSegment item = _segments[index];
            _segments.RemoveAt(index);
            item.Path = null;
        }

        public void Reverse() => _segments.Reverse();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void EnsureCanAttach(VectorSegment segment)
        {
            if (segment.Path != null)
                throw new InvalidOperationException("The vector segment already belongs to a vector path.");
        }
    }
}

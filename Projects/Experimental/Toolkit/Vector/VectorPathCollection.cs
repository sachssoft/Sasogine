using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents the collection of vector paths owned by a <see cref="VectorShape"/>.
    /// </summary>
    public sealed class VectorPathCollection : IList<VectorPath>, IReadOnlyList<VectorPath>
    {
        private readonly List<VectorPath> _paths = [];
        private readonly VectorShape _shape;

        internal VectorPathCollection(VectorShape shape)
        {
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public VectorPath this[int index]
        {
            get => _paths[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                VectorPath current = _paths[index];

                if (ReferenceEquals(current, value))
                    return;

                EnsureCanAttach(value);
                current.Shape = null;
                value.Shape = _shape;
                _paths[index] = value;
            }
        }

        public int Count => _paths.Count;
        public bool IsReadOnly => false;

        public void Add(VectorPath item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);

            item.Shape = _shape;
            _paths.Add(item);
        }

        /// <summary>
        /// Adds the specified items to the collection.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public void AddRange(IEnumerable<VectorPath> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var range = new List<VectorPath>(items);

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
            for (int i = 0; i < _paths.Count; i++)
                _paths[i].Shape = null;

            _paths.Clear();
        }

        public bool Contains(VectorPath item) => _paths.Contains(item);
        public void CopyTo(VectorPath[] array, int arrayIndex) => _paths.CopyTo(array, arrayIndex);
        public IEnumerator<VectorPath> GetEnumerator() => _paths.GetEnumerator();
        public int IndexOf(VectorPath item) => _paths.IndexOf(item);

        public void Insert(int index, VectorPath item)
        {
            ArgumentNullException.ThrowIfNull(item);
            EnsureCanAttach(item);

            item.Shape = _shape;
            _paths.Insert(index, item);
        }

        public bool Remove(VectorPath item)
        {
            if (!_paths.Remove(item))
                return false;

            item.Shape = null;
            return true;
        }

        public void RemoveAt(int index)
        {
            VectorPath item = _paths[index];
            _paths.RemoveAt(index);
            item.Shape = null;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnsureCanAttach(VectorPath path)
        {
            if (path.Shape != null)
                throw new InvalidOperationException("The vector path already belongs to a vector shape.");
        }
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
//{
//    /// <summary>
//    /// Represents the collection of vector segments owned by a <see cref="VectorPath"/>.
//    /// </summary>
//    public sealed class VectorSegmentCollection : IList<IVectorSegment>, IReadOnlyList<IVectorSegment>
//    {
//        private readonly List<IVectorSegment> _segments = [];
//        private readonly VectorPath _path;

//        internal VectorSegmentCollection(VectorPath path)
//        {
//            _path = path ?? throw new ArgumentNullException(nameof(path));
//        }

//        public IVectorSegment this[int index]
//        {
//            get => _segments[index];
//            set
//            {
//                ArgumentNullException.ThrowIfNull(value);

//                IVectorSegment current = _segments[index];

//                if (ReferenceEquals(current, value))
//                    return;

//                EnsureCanAttach(value);
//                ((IVectorSegmentInternal)current).Owner = null;
//                ((IVectorSegmentInternal)value).Owner = _path;
//                _segments[index] = value;
//            }
//        }

//        public int Count => _segments.Count;
//        public bool IsReadOnly => false;

//        public void Add(IVectorSegment item)
//        {
//            ArgumentNullException.ThrowIfNull(item);
//            EnsureCanAttach(item);
//            ((IVectorSegmentInternal)item).Owner = _path;
//            _segments.Add(item);
//        }

//        /// <summary>
//        /// Adds the specified items to the collection.
//        /// </summary>
//        /// <param name="items">The items to add.</param>
//        public void AddRange(IEnumerable<IVectorSegment> items)
//        {
//            ArgumentNullException.ThrowIfNull(items);

//            var range = new List<IVectorSegment>(items);

//            for (int i = 0; i < range.Count; i++)
//            {
//                ArgumentNullException.ThrowIfNull(range[i]);
//                EnsureCanAttach(range[i]);
//            }

//            for (int i = 0; i < range.Count; i++)
//                Add(range[i]);
//        }

//        public void Clear()
//        {
//            for (int i = 0; i < _segments.Count; i++)
//                ((IVectorSegmentInternal)_segments[i]).Owner = null;

//            _segments.Clear();
//        }

//        public bool Contains(IVectorSegment item) => _segments.Contains(item);
//        public void CopyTo(IVectorSegment[] array, int arrayIndex) => _segments.CopyTo(array, arrayIndex);
//        public IEnumerator<IVectorSegment> GetEnumerator() => _segments.GetEnumerator();
//        public int IndexOf(IVectorSegment item) => _segments.IndexOf(item);

//        public void Insert(int index, IVectorSegment item)
//        {
//            ArgumentNullException.ThrowIfNull(item);
//            EnsureCanAttach(item);
//            ((IVectorSegmentInternal)item).Owner = _path;
//            _segments.Insert(index, item);
//        }

//        public bool Remove(IVectorSegment item)
//        {
//            if (!_segments.Remove(item))
//                return false;

//            ((IVectorSegmentInternal)item).Owner = null;
//            return true;
//        }

//        public void RemoveAt(int index)
//        {
//            IVectorSegment item = _segments[index];
//            _segments.RemoveAt(index);
//            ((IVectorSegmentInternal)item).Owner = null;
//        }

//        public void Reverse() => _segments.Reverse();

//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

//        private static void EnsureCanAttach(IVectorSegment segment)
//        {
//            if (segment.Path != null)
//                throw new InvalidOperationException("The vector segment already belongs to a vector path.");
//        }
//    }
//}

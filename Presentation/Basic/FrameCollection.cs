using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation
{
    public class FrameCollection : IList<FrameBase>
    {
        private readonly List<FrameBase> _frames = new List<FrameBase>();
        private readonly HashSet<FrameBase> _frameSet = new HashSet<FrameBase>(); // Für schnelle Contains/Lookup
        private FrameBase[] _cache = Array.Empty<FrameBase>();
        private FrameCollectionChangeResult _changeResultCache = FrameCollectionChangeResult.None;

        public FrameCollection(IFrameChildHost owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public IFrameChildHost Owner { get; }

        public int Count => _frames.Count;

        public bool IsReadOnly => false;

        public IReadOnlyList<FrameBase> VisibleSorted => _cache;

        public FrameBase this[int index]
        {
            get => _frames[index];
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (_frames[index] == value) return;

                var old = _frames[index];
                old.Parent = null;

                value.Parent?.ChildFrames.Remove(value);
                value.Parent = Owner as FrameBase;

                _frames[index] = value;

                RebuildCache();
                Owner.Invalidate();

                _changeResultCache = new FrameCollectionChangeResult(new[] { old, value }, FrameCollectionChangeType.Replaced);
            }
        }

        public void Add(FrameBase frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (frame == Owner) throw new InvalidOperationException("Cannot add self.");
            if (!_frameSet.Add(frame)) return; // HashSet verhindert Duplikate

            frame.Parent?.ChildFrames.Remove(frame);
            frame.Parent = Owner as FrameBase;

            _frames.Add(frame);
            RebuildCache();
            Owner.Invalidate();

            _changeResultCache = new FrameCollectionChangeResult(new[] { frame }, FrameCollectionChangeType.Added);
        }

        public void Insert(int index, FrameBase frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (frame == Owner) throw new InvalidOperationException("Cannot add self.");
            if (!_frameSet.Add(frame)) return;

            frame.Parent?.ChildFrames.Remove(frame);
            frame.Parent = Owner as FrameBase;

            _frames.Insert(index, frame);
            RebuildCache();
            Owner.Invalidate();

            _changeResultCache = new FrameCollectionChangeResult(new[] { frame }, FrameCollectionChangeType.Added);
        }

        public bool Remove(FrameBase frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (!_frameSet.Remove(frame)) return false;

            _frames.Remove(frame);
            frame.Parent = null;
            RebuildCache();
            Owner.Invalidate();

            _changeResultCache = new FrameCollectionChangeResult(new[] { frame }, FrameCollectionChangeType.Removed);
            return true;
        }

        public void RemoveAt(int index)
        {
            var f = _frames[index];
            _frames.RemoveAt(index);
            _frameSet.Remove(f);
            f.Parent = null;

            RebuildCache();
            Owner.Invalidate();

            _changeResultCache = new FrameCollectionChangeResult(new[] { f }, FrameCollectionChangeType.Removed);
        }

        public void Clear()
        {
            var old = _frames.ToArray();
            foreach (var f in _frames) f.Parent = null;
            _frames.Clear();
            _frameSet.Clear();
            _cache = Array.Empty<FrameBase>();

            Owner.Invalidate();
            _changeResultCache = new FrameCollectionChangeResult(old, FrameCollectionChangeType.Removed);
        }

        public bool Contains(FrameBase item) => _frameSet.Contains(item);

        public int IndexOf(FrameBase item) => _frames.IndexOf(item);

        public void CopyTo(FrameBase[] array, int arrayIndex) => _frames.CopyTo(array, arrayIndex);

        public IEnumerator<FrameBase> GetEnumerator() => _frames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Range Methoden mit minimalen Invalidate-Aufrufen
        public void AddRange(IEnumerable<FrameBase> frames)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            var addedList = new List<FrameBase>();
            foreach (var f in frames)
            {
                if (f != null && _frameSet.Add(f))
                {
                    f.Parent?.ChildFrames.Remove(f);
                    f.Parent = Owner as FrameBase;
                    _frames.Add(f);
                    addedList.Add(f);
                }
            }

            if (addedList.Count == 0) return;

            RebuildCache();
            Owner.Invalidate();
            _changeResultCache = new FrameCollectionChangeResult(addedList.ToArray(), FrameCollectionChangeType.Added);
        }

        public void InsertRange(int index, IEnumerable<FrameBase> frames)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            var addedList = new List<FrameBase>();
            int insertIndex = index;
            foreach (var f in frames)
            {
                if (f != null && _frameSet.Add(f))
                {
                    f.Parent?.ChildFrames.Remove(f);
                    f.Parent = Owner as FrameBase;
                    _frames.Insert(insertIndex++, f);
                    addedList.Add(f);
                }
            }

            if (addedList.Count == 0) return;

            RebuildCache();
            Owner.Invalidate();
            _changeResultCache = new FrameCollectionChangeResult(addedList.ToArray(), FrameCollectionChangeType.Added);
        }

        public void RemoveRange(IEnumerable<FrameBase> frames)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            var removedList = new List<FrameBase>();
            foreach (var f in frames)
            {
                if (f != null && _frameSet.Remove(f))
                {
                    _frames.Remove(f);
                    f.Parent = null;
                    removedList.Add(f);
                }
            }

            if (removedList.Count == 0) return;

            RebuildCache();
            Owner.Invalidate();
            _changeResultCache = new FrameCollectionChangeResult(removedList.ToArray(), FrameCollectionChangeType.Removed);
        }

        public FrameCollectionChangeResult ConsumeChange()
        {
            var tmp = _changeResultCache;
            _changeResultCache = FrameCollectionChangeResult.None;
            return tmp;
        }

        // High-Performance RebuildCache ohne LINQ
        internal void RebuildCache()
        {
            var newCache = new FrameBase[_frames.Count];

            // Frames kopieren
            for (int i = 0; i < _frames.Count; i++)
                newCache[i] = _frames[i];

            // Sortierung: zuerst Layer, dann ZIndex
            Array.Sort(newCache, (a, b) =>
            {
                int cmp = a.Layer.CompareTo(b.Layer);
                if (cmp != 0) return cmp;

                return a.ZIndex.CompareTo(b.ZIndex);
            });

            _cache = newCache;
        }
    }
}
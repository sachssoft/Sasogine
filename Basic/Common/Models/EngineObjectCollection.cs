using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    public class EngineObjectCollection<T> : ICollection<T>, IEngineObjectResolver
        where T : class, IEngineObject
    {
        private readonly List<T> _objects = new();

        public event EventHandler? Added;
        public event EventHandler? Removed;

        public int Count => _objects.Count;

        public bool IsReadOnly => false;

        public T? Find(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            foreach (var item in _objects)
            {
                if (string.Equals(id, item.Id, StringComparison.InvariantCulture))
                    return item;
            }

            return null;
        }

        IEngineObject? IEngineObjectResolver.Find(string id)
        {
            return Find(id);
        }

        public IEnumerable<T> FindAll(string? @class)
        {
            foreach (var item in _objects)
            {
                if (string.Equals(@class, item.Class, StringComparison.InvariantCulture))
                    yield return item;
            }
        }

        IEnumerable<IEngineObject> IEngineObjectResolver.FindAll(string? @class)
        {
            return FindAll(@class);
        }

        public bool TryGet(string? id, [MaybeNullWhen(false)] out T? result)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var item in _objects)
                {
                    if (string.Equals(id, item.Id, StringComparison.InvariantCulture))
                    {
                        result = item;
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        bool IEngineObjectResolver.TryGet(string? id, [MaybeNullWhen(false)] out IEngineObject? result)
        {
            result = null;

            if (TryGet(id, out var objectResult))
            {
                result = objectResult;
                return true;
            }

            return false;
        }

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            _objects.Add(item);
            Added?.Invoke(this, EventArgs.Empty);
        }

        public bool Remove(T item)
        {
            if (item == null) return false;

            bool removed = _objects.Remove(item);
            if (removed)
                Removed?.Invoke(this, EventArgs.Empty);

            return removed;
        }

        bool ICollection<T>.Remove(T item) => Remove(item);

        public void Clear()
        {
            if (_objects.Count == 0)
                return;

            _objects.Clear();
            Removed?.Invoke(this, EventArgs.Empty);
        }

        public bool Contains(T item)
        {
            return _objects.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _objects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
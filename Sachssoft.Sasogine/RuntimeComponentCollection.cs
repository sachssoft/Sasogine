using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine
{
    public sealed class RuntimeComponentCollection : IList<IComponent>
    {
        private IComponent[] _items;
        private int _count;

        public RuntimeComponentCollection(int capacity = 16)
        {
            _items = new IComponent[capacity];
            _count = 0;
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public IComponent this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public void Add(IComponent item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        public bool Remove(IComponent item)
        {
            if (item == null) return false;

            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_items[i], item))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            _count--;
            if (index < _count)
                Array.Copy(_items, index + 1, _items, index, _count - index);
            _items[_count] = null!;
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        public bool Contains(IComponent item)
        {
            for (int i = 0; i < _count; i++)
                if (ReferenceEquals(_items[i], item)) return true;
            return false;
        }

        public int IndexOf(IComponent item)
        {
            for (int i = 0; i < _count; i++)
                if (ReferenceEquals(_items[i], item)) return i;
            return -1;
        }

        public void Insert(int index, IComponent item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if ((uint)index > (uint)_count) throw new ArgumentOutOfRangeException(nameof(index));
            EnsureCapacity(_count + 1);
            if (index < _count)
                Array.Copy(_items, index, _items, index + 1, _count - index);
            _items[index] = item;
            _count++;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min) return;
            int newCapacity = _items.Length * 2;
            if (newCapacity < min) newCapacity = min;
            Array.Resize(ref _items, newCapacity);
        }

        public IEnumerator<IComponent> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _items[i];
        }

        public void CopyTo(IComponent[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _count) throw new ArgumentException("Destination array is too small.");

            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        public void LoadAll()
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i] is IResourceComponent resource && !resource.IsLoaded)
                {
                    resource.Load();
                }
            }
        }

        public void UnloadAll()
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i] is IResourceComponent resource && resource.IsLoaded)
                {
                    resource.Unload();
                }
            }
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Unused IList Methods (explicit implementation)

        bool ICollection<IComponent>.Remove(IComponent item) => Remove(item);
        #endregion
    }
}

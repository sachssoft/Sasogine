using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine
{
    public sealed class RuntimeComponentCollection : IList<IComponent>
    {
        private IComponent[] _items;
        private int _count;

        private IRuntimeComponent[] _runtimeCache;
        private int _runtimeCount;

        private IDrawableRuntimeComponent[] _drawableCache;
        private int _drawableCount;

        public RuntimeComponentCollection(int capacity = 16)
        {
            _items = new IComponent[capacity];
            _runtimeCache = new IRuntimeComponent[capacity];
            _drawableCache = new IDrawableRuntimeComponent[capacity];
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
                if (value == null) throw new ArgumentNullException(nameof(value));

                var old = _items[index];
                _items[index] = value;

                UpdateCacheReplace(old, value);
            }
        }

        public ReadOnlySpan<IRuntimeComponent> RuntimeComponents => _runtimeCache.AsSpan(0, _runtimeCount);

        public ReadOnlySpan<IDrawableRuntimeComponent> DrawableComponents => _drawableCache.AsSpan(0, _drawableCount);

        public void Add(IComponent item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
            UpdateCacheAdd(item);
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
            UpdateCacheAdd(item);
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

            var removed = _items[index];
            int last = _count - 1;

            if (index < last)
                Array.Copy(_items, index + 1, _items, index, last - index);

            _items[last] = null!;
            _count--;

            UpdateCacheRemove(removed);
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            Array.Clear(_runtimeCache, 0, _runtimeCount);
            Array.Clear(_drawableCache, 0, _drawableCount);
            _count = _runtimeCount = _drawableCount = 0;
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

        public void CopyTo(IComponent[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || array.Length - arrayIndex < _count)
                throw new ArgumentException("Destination array is too small.");

            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        public void LoadAll(GameBaseContext context)
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_runtimeCache[i] is IResourceComponent resource && !resource.IsLoaded)
                    resource.Load(context);
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource && !resource.IsLoaded)
                    resource.Load(context);
            }
        }

        public void UnloadAll()
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_runtimeCache[i] is IResourceComponent resource && resource.IsLoaded)
                    resource.Unload();
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource && resource.IsLoaded)
                    resource.Unload();
            }
        }

        public void ForEachRuntime(GameFrameContext context)
        {
            for (int i = 0; i < _runtimeCount; i++)
                _drawableCache[i].Update(context);
        }

        public void ForEachDrawable(GameFrameContext context)
        {
            for (int i = 0; i < _drawableCount; i++)
                _drawableCache[i].Draw(context);
        }

        public IEnumerator<IComponent> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _items[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min) return;

            int newCap = Math.Max(_items.Length * 2, min);
            Array.Resize(ref _items, newCap);
            Array.Resize(ref _runtimeCache, newCap);
            Array.Resize(ref _drawableCache, newCap);
        }

        #region Cache Management

        private void UpdateCacheAdd(IComponent item)
        {
            if (item is IRuntimeComponent r) _runtimeCache[_runtimeCount++] = r;
            if (item is IDrawableRuntimeComponent d) _drawableCache[_drawableCount++] = d;
        }

        private void UpdateCacheRemove(IComponent item)
        {
            if (item is IRuntimeComponent r) RemoveFromCache(_runtimeCache, ref _runtimeCount, r);
            if (item is IDrawableRuntimeComponent d) RemoveFromCache(_drawableCache, ref _drawableCount, d);
        }

        private void UpdateCacheReplace(IComponent oldItem, IComponent newItem)
        {
            UpdateCacheRemove(oldItem);
            UpdateCacheAdd(newItem);
        }

        private void RemoveFromCache<T>(T[] cache, ref int count, T value) where T : class
        {
            for (int i = 0; i < count; i++)
            {
                if (ReferenceEquals(cache[i], value))
                {
                    int last = count - 1;
                    cache[i] = cache[last];
                    cache[last] = null!;
                    count--;
                    break;
                }
            }
        }

        #endregion
    }
}

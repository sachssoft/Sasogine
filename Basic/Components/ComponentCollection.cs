using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components
{
    public sealed class ComponentCollection : IList<IComponent>
    {
        private IComponent[] _items;
        private int _count;

        private IUpdatableComponent[] _updatableCache;
        private int _runtimeCount;

        private IDrawableComponent[] _drawableCache;
        private int _drawableCount;

        public ComponentCollection(int capacity = 16)
        {
            _items = new IComponent[capacity];
            _updatableCache = new IUpdatableComponent[capacity];
            _drawableCache = new IDrawableComponent[capacity];
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

        public ReadOnlySpan<IUpdatableComponent> UpdatableComponents => _updatableCache.AsSpan(0, _runtimeCount);

        public ReadOnlySpan<IDrawableComponent> DrawableComponents => _drawableCache.AsSpan(0, _drawableCount);

        public T? FindOne<T>(int startIndex = 0) where T : class, IComponent
        {
            if ((uint)startIndex > (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            for (int i = startIndex; i < _count; i++)
            {
                if (_items[i] is T component)
                    return component;
            }

            return null;
        }

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
            Array.Clear(_updatableCache, 0, _runtimeCount);
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

        public void LoadAll()
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_updatableCache[i] is IResourceComponent resource && !resource.IsLoaded)
                    resource.Load();
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource && !resource.IsLoaded)
                    resource.Load();
            }
        }

        public void UnloadAll()
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_updatableCache[i] is IResourceComponent resource && resource.IsLoaded)
                    resource.Unload();
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource && resource.IsLoaded)
                    resource.Unload();
            }
        }

        public void UpdateForEach(SceneUpdateContext context)
        {
            for (int i = 0; i < _runtimeCount; i++)
                _updatableCache[i].Update(context);
        }

        public void DrawForEach(SceneDrawContext context)
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

            int newCap = int.Max(_items.Length * 2, min);
            Array.Resize(ref _items, newCap);
            Array.Resize(ref _updatableCache, newCap);
            Array.Resize(ref _drawableCache, newCap);
        }

        #region Cache Management

        private void UpdateCacheAdd(IComponent item)
        {
            if (item is IUpdatableComponent u) _updatableCache[_runtimeCount++] = u;
            if (item is IDrawableComponent d) _drawableCache[_drawableCount++] = d;
        }

        private void UpdateCacheRemove(IComponent item)
        {
            if (item is IUpdatableComponent u) RemoveFromCache(_updatableCache, ref _runtimeCount, u);
            if (item is IDrawableComponent d) RemoveFromCache(_drawableCache, ref _drawableCount, d);
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

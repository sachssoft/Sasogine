using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Components
{
    /// <summary>
    /// Provides a collection of components with optimized caches for
    /// updatable and drawable components.
    /// </summary>
    public sealed class ComponentCollection : IList<IComponent>, IList
    {
        private IComponent[] _items;
        private int _count;

        private IUpdatableComponent[] _updatableCache;
        private int _runtimeCount;

        private IDrawableComponent[] _drawableCache;
        private int _drawableCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentCollection"/> class
        /// with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the collection.
        /// </param>
        public ComponentCollection(int capacity = 16)
        {
            _items = new IComponent[capacity];
            _updatableCache = new IUpdatableComponent[capacity];
            _drawableCache = new IDrawableComponent[capacity];
        }

        /// <summary>
        /// Gets the number of components contained in the collection.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the component to get or set.
        /// </param>
        /// <returns>
        /// The component at the specified index.
        /// </returns>
        public IComponent this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _items[index];
            }
            set
            {
                if ((uint)index >= (uint)_count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                ArgumentNullException.ThrowIfNull(value);

                CheckDuplicateType(value, index);

                var old = _items[index];
                _items[index] = value;

                UpdateCacheReplace(old, value);
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = GetComponent(value);
        }

        /// <summary>
        /// Gets the components that can be updated.
        /// </summary>
        public ReadOnlySpan<IUpdatableComponent> UpdatableComponents
            => _updatableCache.AsSpan(0, _runtimeCount);

        /// <summary>
        /// Gets the components that can be drawn.
        /// </summary>
        public ReadOnlySpan<IDrawableComponent> DrawableComponents
            => _drawableCache.AsSpan(0, _drawableCount);

        /// <summary>
        /// Finds the first component of the specified type, starting at the
        /// specified index.
        /// </summary>
        /// <typeparam name="T">
        /// The component type to locate.
        /// </typeparam>
        /// <param name="startIndex">
        /// The zero-based index at which the search begins.
        /// </param>
        /// <returns>
        /// The first matching component, or <see langword="null"/> if no
        /// matching component was found.
        /// </returns>
        public T? FindOne<T>(int startIndex = 0)
            where T : class, IComponent
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

        /// <summary>
        /// Attempts to get a component with the specified runtime type.
        /// </summary>
        /// <param name="componentType">
        /// The exact runtime type of the component to locate.
        /// </param>
        /// <param name="component">
        /// When this method returns, contains the matching component if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching component was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet(
            Type componentType,
            [MaybeNullWhen(false)] out IComponent? component)
        {
            component = null;

            for (int i = 0; i < _count; i++)
            {
                var item = _items[i];

                if (item.GetType() == componentType)
                {
                    component = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to get a component assignable to the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The component type to locate.
        /// </typeparam>
        /// <param name="component">
        /// When this method returns, contains the matching component if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a matching component was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet<T>(
            [MaybeNullWhen(false)] out T? component)
            where T : class, IComponent
        {
            component = null;

            for (int i = 0; i < _count; i++)
            {
                if (_items[i] is T item)
                {
                    component = item;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a component to the collection.
        /// </summary>
        /// <param name="item">
        /// The component to add.
        /// </param>
        public void Add(IComponent item)
        {
            ArgumentNullException.ThrowIfNull(item);

            CheckDuplicateType(item);

            EnsureCapacity(_count + 1);
            _items[_count++] = item;

            UpdateCacheAdd(item);
        }

        int IList.Add(object? value)
        {
            Add(GetComponent(value));
            return _count - 1;
        }

        /// <summary>
        /// Inserts a component at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the component should be inserted.
        /// </param>
        /// <param name="item">
        /// The component to insert.
        /// </param>
        public void Insert(int index, IComponent item)
        {
            ArgumentNullException.ThrowIfNull(item);

            if ((uint)index > (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            CheckDuplicateType(item);

            EnsureCapacity(_count + 1);

            if (index < _count)
            {
                Array.Copy(
                    _items,
                    index,
                    _items,
                    index + 1,
                    _count - index);
            }

            _items[index] = item;
            _count++;

            UpdateCacheAdd(item);
        }

        void IList.Insert(int index, object? value)
            => Insert(index, GetComponent(value));

        /// <summary>
        /// Removes the specified component from the collection.
        /// </summary>
        /// <param name="item">
        /// The component to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the component was removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Remove(IComponent item)
        {
            if (item == null)
                return false;

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

        void IList.Remove(object? value)
        {
            if (value is IComponent component)
                Remove(component);
        }

        /// <summary>
        /// Removes the component at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the component to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var removed = _items[index];
            int last = _count - 1;

            if (index < last)
            {
                Array.Copy(
                    _items,
                    index + 1,
                    _items,
                    index,
                    last - index);
            }

            _items[last] = null!;
            _count--;

            UpdateCacheRemove(removed);
        }

        /// <summary>
        /// Removes all components from the collection.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            Array.Clear(_updatableCache, 0, _runtimeCount);
            Array.Clear(_drawableCache, 0, _drawableCount);

            _count = 0;
            _runtimeCount = 0;
            _drawableCount = 0;
        }

        /// <summary>
        /// Determines whether the collection contains the specified component.
        /// </summary>
        /// <param name="item">
        /// The component to locate.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the component is contained in the collection;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(IComponent item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_items[i], item))
                    return true;
            }

            return false;
        }

        bool IList.Contains(object? value)
            => value is IComponent component && Contains(component);

        /// <summary>
        /// Determines the index of the specified component.
        /// </summary>
        /// <param name="item">
        /// The component to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the component if found;
        /// otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(IComponent item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_items[i], item))
                    return i;
            }

            return -1;
        }

        int IList.IndexOf(object? value)
            => value is IComponent component
                ? IndexOf(component)
                : -1;

        /// <summary>
        /// Copies the components to the specified array, starting at the
        /// specified index.
        /// </summary>
        /// <param name="array">
        /// The destination array.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in the destination array at which copying begins.
        /// </param>
        public void CopyTo(IComponent[] array, int arrayIndex)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (arrayIndex < 0 || array.Length - arrayIndex < _count)
                throw new ArgumentException("Destination array is too small.");

            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ArgumentNullException.ThrowIfNull(array);

            if (array.Rank != 1)
                throw new ArgumentException(
                    "Only single-dimensional arrays are supported.",
                    nameof(array));

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (array.Length - index < _count)
                throw new ArgumentException(
                    "Destination array is too small.",
                    nameof(array));

            Array.Copy(_items, 0, array, index, _count);
        }

        /// <summary>
        /// Loads all resource components that are not already loaded.
        /// </summary>
        public void LoadAll()
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_updatableCache[i] is IResourceComponent resource &&
                    !resource.IsLoaded)
                {
                    resource.Load();
                }
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource &&
                    !resource.IsLoaded)
                {
                    resource.Load();
                }
            }
        }

        /// <summary>
        /// Unloads all loaded resource components.
        /// </summary>
        public void UnloadAll()
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                if (_updatableCache[i] is IResourceComponent resource &&
                    resource.IsLoaded)
                {
                    resource.Unload();
                }
            }

            for (int i = 0; i < _drawableCount; i++)
            {
                if (_drawableCache[i] is IResourceComponent resource &&
                    resource.IsLoaded)
                {
                    resource.Unload();
                }
            }
        }

        /// <summary>
        /// Updates each updatable component that is not excluded by the
        /// specified filter.
        /// </summary>
        /// <param name="context">
        /// The current scene update context.
        /// </param>
        /// <param name="filter">
        /// An optional function that returns <see langword="true"/> to exclude
        /// a component from the update operation.
        /// </param>
        public void UpdateForEach(
            SceneUpdateContext context,
            Func<IUpdatableComponent, bool>? filter = null)
        {
            for (int i = 0; i < _runtimeCount; i++)
            {
                var updatable = _updatableCache[i];

                if (filter == null || !filter(updatable))
                    updatable.Update(context);
            }
        }

        /// <summary>
        /// Draws each drawable component that is not excluded by the
        /// specified filter.
        /// </summary>
        /// <param name="context">
        /// The current scene draw context.
        /// </param>
        /// <param name="filter">
        /// An optional function that returns <see langword="true"/> to exclude
        /// a component from the draw operation.
        /// </param>
        public void DrawForEach(
            SceneDrawContext context,
            Func<IDrawableComponent, bool>? filter = null)
        {
            for (int i = 0; i < _drawableCount; i++)
            {
                var drawable = _drawableCache[i];

                if (filter == null || !filter(drawable))
                    drawable.Draw(context);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the components.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection.
        /// </returns>
        public IEnumerator<IComponent> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _items[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void CheckDuplicateType(
            IComponent item,
            int ignoreIndex = -1)
        {
            var type = item.GetType();

            for (int i = 0; i < _count; i++)
            {
                if (i == ignoreIndex)
                    continue;

                if (_items[i].GetType() == type)
                {
                    throw new InvalidOperationException(
                        $"Component type {type.Name} already exists.");
                }
            }
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min)
                return;

            int newCapacity = int.Max(_items.Length * 2, min);

            Array.Resize(ref _items, newCapacity);
            Array.Resize(ref _updatableCache, newCapacity);
            Array.Resize(ref _drawableCache, newCapacity);
        }

        private static IComponent GetComponent(object? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value is not IComponent component)
            {
                throw new ArgumentException(
                    $"Value must implement {nameof(IComponent)}.",
                    nameof(value));
            }

            return component;
        }

        #region Cache Management

        private void UpdateCacheAdd(IComponent item)
        {
            if (item is IUpdatableComponent updatable)
                _updatableCache[_runtimeCount++] = updatable;

            if (item is IDrawableComponent drawable)
                _drawableCache[_drawableCount++] = drawable;
        }

        private void UpdateCacheRemove(IComponent item)
        {
            if (item is IUpdatableComponent updatable)
            {
                RemoveFromCache(
                    _updatableCache,
                    ref _runtimeCount,
                    updatable);
            }

            if (item is IDrawableComponent drawable)
            {
                RemoveFromCache(
                    _drawableCache,
                    ref _drawableCount,
                    drawable);
            }
        }

        private void UpdateCacheReplace(
            IComponent oldItem,
            IComponent newItem)
        {
            UpdateCacheRemove(oldItem);
            UpdateCacheAdd(newItem);
        }

        private static void RemoveFromCache<T>(
            T[] cache,
            ref int count,
            T value)
            where T : class
        {
            for (int i = 0; i < count; i++)
            {
                if (!ReferenceEquals(cache[i], value))
                    continue;

                int last = count - 1;

                cache[i] = cache[last];
                cache[last] = null!;
                count--;

                break;
            }
        }

        #endregion
    }
}
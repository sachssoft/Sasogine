using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Provides a collection of entities with support for ordered loading,
    /// updating, drawing, and unloading.
    /// </summary>
    public class EntityCollection : IList<IEntity>, IList
    {
        private readonly List<IEntity> _entities = new();
        private readonly List<IEntity> _sortedCache = new();

        private bool _cacheDirty = true;

        /// <summary>
        /// Gets the entities in render and update order.
        /// Entities that do not implement <see cref="IOrderedEntity"/>
        /// are placed at the end of the collection.
        /// </summary>
        public IReadOnlyList<IEntity> OrderedEntities
        {
            get
            {
                UpdateCache();
                return _sortedCache;
            }
        }

        /// <summary>
        /// Gets or sets the entity at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the entity to get or set.
        /// </param>
        /// <returns>
        /// The entity at the specified index.
        /// </returns>
        public IEntity this[int index]
        {
            get => _entities[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                _entities[index] = value;
                _cacheDirty = true;
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set => this[index] = GetEntity(value);
        }

        /// <summary>
        /// Gets the number of entities contained in the collection.
        /// </summary>
        public int Count => _entities.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => ((ICollection)_entities).SyncRoot;

        /// <summary>
        /// Adds an entity to the collection.
        /// </summary>
        /// <param name="item">
        /// The entity to add.
        /// </param>
        public void Add(IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _entities.Add(item);
            _cacheDirty = true;
        }

        int IList.Add(object? value)
        {
            Add(GetEntity(value));
            return Count - 1;
        }

        /// <summary>
        /// Removes all entities from the collection.
        /// </summary>
        public void Clear()
        {
            _entities.Clear();
            _sortedCache.Clear();
            _cacheDirty = true;
        }

        /// <summary>
        /// Determines whether the collection contains the specified entity.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the entity is contained in the collection;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(IEntity item)
            => _entities.Contains(item);

        bool IList.Contains(object? value)
            => value is IEntity entity && Contains(entity);

        /// <summary>
        /// Copies the entities to the specified array, starting at the specified index.
        /// </summary>
        /// <param name="array">
        /// The destination array.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in the destination array at which copying begins.
        /// </param>
        public void CopyTo(IEntity[] array, int arrayIndex)
            => _entities.CopyTo(array, arrayIndex);

        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)_entities).CopyTo(array, index);

        /// <summary>
        /// Returns an enumerator that iterates through the entities.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection.
        /// </returns>
        public IEnumerator<IEntity> GetEnumerator()
            => _entities.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Determines the index of the specified entity.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the entity if found;
        /// otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(IEntity item)
            => _entities.IndexOf(item);

        int IList.IndexOf(object? value)
            => value is IEntity entity ? IndexOf(entity) : -1;

        /// <summary>
        /// Inserts an entity at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the entity should be inserted.
        /// </param>
        /// <param name="item">
        /// The entity to insert.
        /// </param>
        public void Insert(int index, IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            _entities.Insert(index, item);
            _cacheDirty = true;
        }

        void IList.Insert(int index, object? value)
            => Insert(index, GetEntity(value));

        /// <summary>
        /// Removes the specified entity from the collection.
        /// </summary>
        /// <param name="item">
        /// The entity to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the entity was removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Remove(IEntity item)
        {
            bool removed = _entities.Remove(item);

            if (removed)
                _cacheDirty = true;

            return removed;
        }

        void IList.Remove(object? value)
        {
            if (value is IEntity entity)
                Remove(entity);
        }

        /// <summary>
        /// Removes the entity at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the entity to remove.
        /// </param>
        public void RemoveAt(int index)
        {
            _entities.RemoveAt(index);
            _cacheDirty = true;
        }

        /// <summary>
        /// Loads all entities in their configured order.
        /// </summary>
        public void Load()
        {
            ForEachOrdered(entity => entity.Load());
        }

        /// <summary>
        /// Asynchronously loads all entities in their configured order.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous load operation.
        /// </returns>
        public async Task LoadAsync()
        {
            UpdateCache();

            foreach (var entity in _sortedCache)
                await entity.LoadAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unloads all entities in reverse configured order.
        /// </summary>
        public void Unload()
        {
            UpdateCache();

            for (int i = _sortedCache.Count - 1; i >= 0; i--)
                _sortedCache[i].Unload();
        }

        /// <summary>
        /// Updates all entities that implement <see cref="IUpdatableEntity"/>.
        /// </summary>
        /// <param name="context">
        /// The current scene update context.
        /// </param>
        public void Update(SceneUpdateContext context)
        {
            ForEachOrdered(entity =>
            {
                if (entity is IUpdatableEntity updatable)
                    updatable.Update(context);
            });
        }

        /// <summary>
        /// Draws all entities that implement <see cref="IDrawableEntity"/>.
        /// </summary>
        /// <param name="context">
        /// The current scene draw context.
        /// </param>
        public void Draw(SceneDrawContext context)
        {
            ForEachOrdered(entity =>
            {
                if (entity is IDrawableEntity drawable)
                    drawable.Draw(context);
            });
        }

        private void ForEachOrdered(Action<IEntity> action)
        {
            UpdateCache();

            foreach (var entity in _sortedCache)
                action(entity);
        }

        private void UpdateCache()
        {
            if (!_cacheDirty)
                return;

            _sortedCache.Clear();
            _sortedCache.AddRange(_entities);

            _sortedCache.Sort((a, b) =>
            {
                int oa = a is IOrderedEntity orderedA
                    ? orderedA.Order
                    : int.MaxValue;

                int ob = b is IOrderedEntity orderedB
                    ? orderedB.Order
                    : int.MaxValue;

                int result = oa.CompareTo(ob);

                if (result != 0)
                    return result;

                return _entities.IndexOf(a).CompareTo(
                    _entities.IndexOf(b));
            });

            _cacheDirty = false;
        }

        private static IEntity GetEntity(object? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value is not IEntity entity)
            {
                throw new ArgumentException(
                    $"Value must implement {nameof(IEntity)}.",
                    nameof(value));
            }

            return entity;
        }
    }
}
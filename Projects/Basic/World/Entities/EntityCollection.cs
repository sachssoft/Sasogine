using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Provides a collection of unique entity instances with support for
    /// initialization, ordered loading, updating, drawing, and unloading.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The context type used to initialize entities.
    /// </typeparam>
    /// <remarks>
    /// Each entity instance can occur only once in the collection.
    ///
    /// When a <see cref="Context"/> is assigned, entities that implement
    /// <see cref="IInitializableEntity{TEntityContext}"/> are initialized
    /// automatically when they are added and deinitialized when they are removed.
    ///
    /// A <see langword="null"/> context disables automatic initialization.
    /// </remarks>
    public class EntityCollection<TEntityContext> :
        IList<IEntity>,
        IList
        where TEntityContext : class, IEntityContext
    {
        private readonly List<IEntity> _entities = new();
        private readonly List<IEntity> _sortedCache = new();

        private TEntityContext? _context;
        private bool _cacheDirty = true;

        /// <summary>
        /// Gets or sets the context used to initialize entities in the collection.
        /// </summary>
        /// <value>
        /// The entity context, or <see langword="null"/> if automatic
        /// initialization is disabled.
        /// </value>
        /// <remarks>
        /// Changing the context deinitializes all applicable entities using the
        /// previous context state.
        ///
        /// If the new context is not <see langword="null"/>, entities that
        /// implement <see cref="IInitializableEntity{TEntityContext}"/> are
        /// initialized using the new context.
        /// </remarks>
        public TEntityContext? Context
        {
            get => _context;
            set
            {
                if (ReferenceEquals(_context, value))
                    return;

                if (_context is not null)
                {
                    for (int i = _entities.Count - 1; i >= 0; i--)
                        DeinitializeEntity(_entities[i]);
                }

                _context = value;

                if (_context is not null)
                {
                    foreach (var entity in _entities)
                        InitializeEntity(entity);
                }
            }
        }

        /// <summary>
        /// Gets the entities in their configured render and update order.
        /// </summary>
        /// <value>
        /// A read-only list containing the entities in their configured order.
        /// </value>
        /// <remarks>
        /// Entities that implement <see cref="IOrderedEntity"/> are ordered by
        /// their <see cref="IOrderedEntity.Order"/> value.
        ///
        /// Entities that do not implement <see cref="IOrderedEntity"/> are placed
        /// after ordered entities.
        ///
        /// Entities with the same order retain their collection order.
        /// </remarks>
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is already contained in the collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the valid range of the collection.
        /// </exception>
        public IEntity this[int index]
        {
            get => _entities[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                var previous = _entities[index];

                if (ReferenceEquals(previous, value))
                    return;

                EnsureUnique(value);

                DeinitializeEntity(previous);
                InitializeEntity(value);

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
        /// <value>
        /// Always <see langword="false"/>.
        /// </value>
        public bool IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
            => ((ICollection)_entities).SyncRoot;

        /// <summary>
        /// Adds an entity to the collection.
        /// </summary>
        /// <param name="item">
        /// The entity to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection.
        /// </exception>
        /// <remarks>
        /// If a <see cref="Context"/> is assigned and the entity implements
        /// <see cref="IInitializableEntity{TEntityContext}"/>, it is initialized
        /// before being added.
        /// </remarks>
        public void Add(IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EnsureUnique(item);
            InitializeEntity(item);

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
        /// <remarks>
        /// Applicable entities are deinitialized in reverse collection order
        /// before the collection is cleared.
        /// </remarks>
        public void Clear()
        {
            if (_context is not null)
            {
                for (int i = _entities.Count - 1; i >= 0; i--)
                    DeinitializeEntity(_entities[i]);
            }

            _entities.Clear();
            _sortedCache.Clear();

            _cacheDirty = true;
        }

        /// <summary>
        /// Determines whether the collection contains the specified entity
        /// instance.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the entity instance is contained in the
        /// collection; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Entity identity is determined by reference.
        /// </remarks>
        public bool Contains(IEntity item)
            => IndexOfReference(item) >= 0;

        bool IList.Contains(object? value)
            => value is IEntity entity && Contains(entity);

        /// <summary>
        /// Copies the entities to the specified array, starting at the specified
        /// array index.
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
        /// Determines the index of the specified entity instance.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the entity if found; otherwise, <c>-1</c>.
        /// </returns>
        /// <remarks>
        /// Entity identity is determined by reference rather than by
        /// <see cref="object.Equals(object?)"/>.
        /// </remarks>
        public int IndexOf(IEntity item)
            => IndexOfReference(item);

        int IList.IndexOf(object? value)
            => value is IEntity entity
                ? IndexOf(entity)
                : -1;

        /// <summary>
        /// Inserts an entity into the collection at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the entity should be inserted.
        /// </param>
        /// <param name="item">
        /// The entity to insert.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the valid insertion range.
        /// </exception>
        public void Insert(int index, IEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EnsureUnique(item);
            InitializeEntity(item);

            _entities.Insert(index, item);
            _cacheDirty = true;
        }

        void IList.Insert(int index, object? value)
            => Insert(index, GetEntity(value));

        /// <summary>
        /// Removes the specified entity instance from the collection.
        /// </summary>
        /// <param name="item">
        /// The entity to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the entity was found and removed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The entity is deinitialized before it is removed when a
        /// <see cref="Context"/> is available and the entity implements
        /// <see cref="IInitializableEntity{TEntityContext}"/>.
        /// </remarks>
        public bool Remove(IEntity item)
        {
            int index = IndexOfReference(item);

            if (index < 0)
                return false;

            var entity = _entities[index];

            DeinitializeEntity(entity);

            _entities.RemoveAt(index);
            _cacheDirty = true;

            return true;
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the valid range of the collection.
        /// </exception>
        public void RemoveAt(int index)
        {
            var entity = _entities[index];

            DeinitializeEntity(entity);

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
        /// Updates all entities that implement <see cref="IUpdatableEntity"/>
        /// in their configured order.
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
        /// Draws all entities that implement <see cref="IDrawableEntity"/>
        /// in their configured order.
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

        /// <summary>
        /// Initializes the specified entity when an entity context is available.
        /// </summary>
        /// <param name="entity">
        /// The entity to initialize.
        /// </param>
        private void InitializeEntity(IEntity entity)
        {
            if (_context is not null &&
                entity is IInitializableEntity<TEntityContext> initializable)
            {
                initializable.Initialize(_context);
            }
        }

        /// <summary>
        /// Deinitializes the specified entity when an entity context is available.
        /// </summary>
        /// <param name="entity">
        /// The entity to deinitialize.
        /// </param>
        private void DeinitializeEntity(IEntity entity)
        {
            if (_context is not null &&
                entity is IInitializableEntity<TEntityContext> initializable)
            {
                initializable.Deinitialize();
            }
        }

        /// <summary>
        /// Ensures that the specified entity instance is not already contained
        /// in the collection.
        /// </summary>
        /// <param name="entity">
        /// The entity to validate.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The entity is already contained in the collection.
        /// </exception>
        private void EnsureUnique(IEntity entity)
        {
            if (IndexOfReference(entity) >= 0)
            {
                throw new ArgumentException(
                    "The entity is already contained in the collection.",
                    nameof(entity));
            }
        }

        /// <summary>
        /// Determines the index of the specified entity using reference identity.
        /// </summary>
        /// <param name="entity">
        /// The entity instance to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the entity if found; otherwise, <c>-1</c>.
        /// </returns>
        private int IndexOfReference(IEntity entity)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (ReferenceEquals(_entities[i], entity))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Executes the specified action for each entity in configured order.
        /// </summary>
        /// <param name="action">
        /// The action to execute.
        /// </param>
        private void ForEachOrdered(Action<IEntity> action)
        {
            UpdateCache();

            foreach (var entity in _sortedCache)
                action(entity);
        }

        /// <summary>
        /// Rebuilds the ordered entity cache when necessary.
        /// </summary>
        private void UpdateCache()
        {
            if (!_cacheDirty)
                return;

            _sortedCache.Clear();
            _sortedCache.AddRange(_entities);

            _sortedCache.Sort((a, b) =>
            {
                int orderA = a is IOrderedEntity orderedA
                    ? orderedA.Order
                    : int.MaxValue;

                int orderB = b is IOrderedEntity orderedB
                    ? orderedB.Order
                    : int.MaxValue;

                int result = orderA.CompareTo(orderB);

                if (result != 0)
                    return result;

                return IndexOfReference(a).CompareTo(
                    IndexOfReference(b));
            });

            _cacheDirty = false;
        }

        /// <summary>
        /// Converts the specified object to an entity.
        /// </summary>
        /// <param name="value">
        /// The object to convert.
        /// </param>
        /// <returns>
        /// The converted entity.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> does not implement <see cref="IEntity"/>.
        /// </exception>
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
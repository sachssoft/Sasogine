using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Provides a strongly typed observable collection of unique entity instances
    /// with support for initialization, resolution, ordered loading, updating,
    /// drawing, and unloading.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type contained in the collection.
    /// </typeparam>
    /// <typeparam name="TEntityContext">
    /// The context type used to initialize entities.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Each entity instance can occur only once in the collection. Non-empty
    /// entity identifiers must also be unique.
    /// </para>
    /// <para>
    /// When a <see cref="Context"/> is assigned, entities that implement
    /// <see cref="IInitializableEntity{TEntityContext}"/> are initialized
    /// automatically when they are added and deinitialized when they are removed.
    /// </para>
    /// <para>
    /// Collection changes are reported through
    /// <see cref="INotifyCollectionChanged"/> and
    /// <see cref="INotifyPropertyChanged"/>.
    /// </para>
    /// </remarks>
    public class EntityCollection<TEntity, TEntityContext> :
        IList<TEntity>,
        IList,
        IEngineObjectResolver,
        INotifyCollectionChanged,
        INotifyPropertyChanged
        where TEntity : class, IEntity
        where TEntityContext : class, IEntityContext
    {
        private readonly List<TEntity> _entities = new();
        private readonly List<TEntity> _sortedCache = new();

        private TEntityContext? _context;
        private bool _cacheDirty = true;

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the context used to initialize entities in the collection.
        /// </summary>
        /// <value>
        /// The entity context, or <see langword="null"/> if automatic entity
        /// initialization is disabled.
        /// </value>
        /// <remarks>
        /// When the context changes, all currently initialized entities are
        /// deinitialized using the previous context state.
        ///
        /// If the new context is not <see langword="null"/>, all entities that
        /// implement <see cref="IInitializableEntity{TEntityContext}"/> are
        /// initialized using the new context.
        ///
        /// Setting the context to <see langword="null"/> deinitializes all
        /// applicable entities without reinitializing them.
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

                OnPropertyChanged(nameof(Context));
            }
        }

        /// <summary>
        /// Gets the entities in their configured render and update order.
        /// </summary>
        /// <value>
        /// A read-only list containing the entities in their current configured
        /// order.
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
        public IReadOnlyList<TEntity> OrderedEntities
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
        /// <paramref name="value"/> or its identifier is already contained in
        /// the collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is outside the valid range of the collection.
        /// </exception>
        public TEntity this[int index]
        {
            get => _entities[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                TEntity previous = _entities[index];

                if (ReferenceEquals(previous, value))
                    return;

                EnsureUnique(value, previous);

                DeinitializeEntity(previous);
                InitializeEntity(value);

                _entities[index] = value;
                _cacheDirty = true;

                OnPropertyChanged("Item[]");
                OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        value,
                        previous,
                        index));
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

        object ICollection.SyncRoot => ((ICollection)_entities).SyncRoot;

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
        /// The entity instance or its identifier is already contained in the
        /// collection.
        /// </exception>
        public void Add(TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EnsureUnique(item);
            InitializeEntity(item);

            int index = _entities.Count;

            _entities.Add(item);
            _cacheDirty = true;

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
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
        /// If a <see cref="Context"/> is assigned, all applicable entities are
        /// deinitialized in reverse collection order before the collection is
        /// cleared.
        /// </remarks>
        public void Clear()
        {
            if (_entities.Count == 0)
                return;

            if (_context is not null)
            {
                for (int i = _entities.Count - 1; i >= 0; i--)
                    DeinitializeEntity(_entities[i]);
            }

            _entities.Clear();
            _sortedCache.Clear();
            _cacheDirty = true;

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Determines whether the collection contains the specified entity
        /// instance.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified entity instance is contained
        /// in the collection; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Entity identity is determined by reference rather than by
        /// <see cref="object.Equals(object?)"/>.
        /// </remarks>
        public bool Contains(TEntity item)
        {
            return IndexOfReference(item) >= 0;
        }

        bool IList.Contains(object? value)
        {
            return value is TEntity entity && Contains(entity);
        }

        /// <summary>
        /// Copies the entities to the specified array.
        /// </summary>
        /// <param name="array">
        /// The destination array.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in the destination array at which copying begins.
        /// </param>
        public void CopyTo(TEntity[] array, int arrayIndex)
        {
            _entities.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_entities).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator for the collection.
        /// </returns>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return _entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines the index of the specified entity instance.
        /// </summary>
        /// <param name="item">
        /// The entity to locate.
        /// </param>
        /// <returns>
        /// The zero-based index of the entity if found; otherwise, <c>-1</c>.
        /// </returns>
        public int IndexOf(TEntity item)
        {
            return IndexOfReference(item);
        }

        int IList.IndexOf(object? value)
        {
            return value is TEntity entity ? IndexOf(entity) : -1;
        }

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
        /// The entity instance or its identifier is already contained in the
        /// collection.
        /// </exception>
        public void Insert(int index, TEntity item)
        {
            ArgumentNullException.ThrowIfNull(item);

            EnsureUnique(item);
            InitializeEntity(item);

            _entities.Insert(index, item);
            _cacheDirty = true;

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
        }

        void IList.Insert(int index, object? value)
        {
            Insert(index, GetEntity(value));
        }

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
        public bool Remove(TEntity item)
        {
            int index = IndexOfReference(item);

            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        void IList.Remove(object? value)
        {
            if (value is TEntity entity)
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
            TEntity entity = _entities[index];

            DeinitializeEntity(entity);

            _entities.RemoveAt(index);
            _cacheDirty = true;

            OnPropertyChanged(nameof(Count));
            OnPropertyChanged("Item[]");
            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    entity,
                    index));
        }

        /// <summary>
        /// Finds an entity with the specified identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the entity to find.
        /// </param>
        /// <returns>
        /// The matching entity, or <see langword="null"/> if no entity was found.
        /// </returns>
        public TEntity? Find(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return default;

            foreach (TEntity entity in _entities)
            {
                if (entity.Id == id)
                    return entity;
            }

            return default;
        }

        /// <summary>
        /// Finds an entity of the specified type with the specified identifier.
        /// </summary>
        /// <typeparam name="T">
        /// The entity type to find.
        /// </typeparam>
        /// <param name="id">
        /// The identifier of the entity to find.
        /// </param>
        /// <returns>
        /// The matching entity, or <see langword="null"/> if no entity was found.
        /// </returns>
        public T? Find<T>(string? id)
            where T : class, TEntity
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (TEntity entity in _entities)
            {
                if (entity.Id == id && entity is T typedEntity)
                    return typedEntity;
            }

            return null;
        }

        /// <summary>
        /// Finds all entities with the specified class.
        /// </summary>
        /// <param name="class">
        /// The class of the entities to find.
        /// </param>
        /// <returns>
        /// All matching entities.
        /// </returns>
        public IEnumerable<TEntity> FindAll(string? @class)
        {
            foreach (TEntity entity in _entities)
            {
                if (entity.Class == @class)
                    yield return entity;
            }
        }

        /// <summary>
        /// Finds all entities of the specified type with the specified class.
        /// </summary>
        /// <typeparam name="T">
        /// The entity type to find.
        /// </typeparam>
        /// <param name="class">
        /// The class of the entities to find.
        /// </param>
        /// <returns>
        /// All matching entities.
        /// </returns>
        public IEnumerable<T> FindAll<T>(string? @class)
            where T : class, TEntity
        {
            foreach (TEntity entity in _entities)
            {
                if (entity.Class == @class && entity is T typedEntity)
                    yield return typedEntity;
            }
        }

        /// <summary>
        /// Attempts to find an entity with the specified identifier.
        /// </summary>
        /// <param name="id">
        /// The identifier of the entity to find.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the matching entity if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an entity was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGet(string? id, out TEntity? result)
        {
            result = Find(id);
            return result is not null;
        }

        /// <summary>
        /// Attempts to find an entity of the specified type with the specified
        /// identifier.
        /// </summary>
        /// <typeparam name="T">
        /// The entity type to find.
        /// </typeparam>
        /// <param name="id">
        /// The identifier of the entity to find.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the matching entity if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an entity was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGet<T>(string? id, out T? result)
            where T : class, TEntity
        {
            result = Find<T>(id);
            return result is not null;
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
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The event data describing the collection change.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        IEngineReferenceable? IEngineObjectResolver.Find(string? id)
        {
            return Find(id);
        }

        IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(string? @class)
        {
            foreach (TEntity entity in FindAll(@class))
                yield return entity;
        }

        bool IEngineObjectResolver.TryGet(
            string? id,
            out IEngineReferenceable? result)
        {
            TEntity? entity = Find(id);

            result = entity;
            return entity is not null;
        }

        private void InitializeEntity(TEntity entity)
        {
            if (_context is not null &&
                entity is IInitializableEntity<TEntityContext> initializable)
            {
                initializable.Initialize(_context);
            }
        }

        private void DeinitializeEntity(TEntity entity)
        {
            if (_context is not null &&
                entity is IInitializableEntity<TEntityContext> initializable)
            {
                initializable.Deinitialize();
            }
        }

        private void EnsureUnique(
            TEntity entity,
            TEntity? excludedEntity = null)
        {
            foreach (TEntity existing in _entities)
            {
                if (ReferenceEquals(existing, excludedEntity))
                    continue;

                if (ReferenceEquals(existing, entity))
                {
                    throw new ArgumentException(
                        "The entity is already contained in the collection.",
                        nameof(entity));
                }

                if (!string.IsNullOrEmpty(entity.Id) && existing.Id == entity.Id)
                {
                    throw new ArgumentException(
                        $"An entity with the identifier '{entity.Id}' already exists.",
                        nameof(entity));
                }
            }
        }

        private int IndexOfReference(TEntity entity)
        {
            for (int i = 0; i < _entities.Count; i++)
            {
                if (ReferenceEquals(_entities[i], entity))
                    return i;
            }

            return -1;
        }

        private void ForEachOrdered(Action<TEntity> action)
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
                int orderA = a is IOrderedEntity orderedA
                    ? orderedA.Order
                    : int.MaxValue;

                int orderB = b is IOrderedEntity orderedB
                    ? orderedB.Order
                    : int.MaxValue;

                int result = orderA.CompareTo(orderB);

                if (result != 0)
                    return result;

                return IndexOfReference(a).CompareTo(IndexOfReference(b));
            });

            _cacheDirty = false;
        }

        private static TEntity GetEntity(object? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value is not TEntity entity)
            {
                throw new ArgumentException(
                    $"Value must be of type {typeof(TEntity).FullName}.",
                    nameof(value));
            }

            return entity;
        }
    }
}
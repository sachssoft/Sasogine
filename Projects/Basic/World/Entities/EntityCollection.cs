using Sachssoft.Engine.Common.Collections;
using Sachssoft.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Engine.World
{
    /// <summary>
    /// Represents a contextual collection of entities with support for
    /// reference resolution, ordered loading, updating, drawing, and unloading.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The entity context type used to initialize entities.
    /// </typeparam>
    /// <remarks>
    /// Entity identifiers are tracked by the underlying
    /// <see cref="ReferencableCollection{T}"/> and must be unique within
    /// the collection.
    ///
    /// Entity lifecycle management is provided by
    /// <see cref="ContextualReferencableCollection{T, TContext}"/>.
    /// When the collection is initialized with a context, existing and
    /// subsequently added initializable entities are initialized automatically.
    /// Removed, replaced, or cleared entities are deinitialized automatically.
    ///
    /// Entities that implement <see cref="IOrderedEntity"/> are processed
    /// according to their configured order. Other entities are processed after
    /// ordered entities.
    /// </remarks>
    public class EntityCollection<TEntityContext> :
        ContextualReferencableCollection<IEntity, TEntityContext>
        where TEntityContext : class, IEntityContext
    {
        private readonly List<IEntity> _sortedCache = new();
        private bool _cacheDirty = true;

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="EntityCollection{TEntityContext}"/> class.
        /// </summary>
        public EntityCollection()
        {
        }

        /// <summary>
        /// Initializes a new, empty instance of the
        /// <see cref="EntityCollection{TEntityContext}"/> class with the
        /// specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial number of entities that the collection can contain
        /// without resizing.
        /// </param>
        public EntityCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="EntityCollection{TEntityContext}"/> class containing
        /// the specified entities.
        /// </summary>
        /// <param name="entities">
        /// The entities to add to the collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entities"/> is <see langword="null"/>.
        /// </exception>
        public EntityCollection(IEnumerable<IEntity> entities)
            : base(entities)
        {
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
        public IReadOnlyList<IEntity> OrderedEntities
        {
            get
            {
                UpdateCache();
                return _sortedCache;
            }
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
        /// The matching entity, or <see langword="null"/> if no matching
        /// entity exists.
        /// </returns>
        public T? Find<T>(string? id)
            where T : class, IEntity
        {
            return Find(id) as T;
        }

        /// <summary>
        /// Finds all entities of the specified type with the specified class.
        /// </summary>
        /// <typeparam name="T">
        /// The entity type to find.
        /// </typeparam>
        /// <param name="class">
        /// The entity class to search for.
        /// </param>
        /// <returns>
        /// An enumerable containing all matching entities.
        /// </returns>
        public IEnumerable<T> FindAll<T>(string? @class)
            where T : class, IEntity
        {
            foreach (IEntity entity in this)
            {
                if (entity is T typedEntity &&
                    string.Equals(
                        entity.Class,
                        @class,
                        StringComparison.Ordinal))
                {
                    yield return typedEntity;
                }
            }
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
        /// <see langword="true"/> if a matching entity was found;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGet<T>(string? id, out T? result)
            where T : class, IEntity
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

            foreach (IEntity entity in _sortedCache)
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
        /// Marks the ordered entity cache as dirty after an entity has been
        /// inserted.
        /// </summary>
        /// <param name="index">
        /// The index at which the entity was inserted.
        /// </param>
        /// <param name="item">
        /// The inserted entity.
        /// </param>
        protected override void OnInserted(int index, IEntity item)
        {
            _cacheDirty = true;
            base.OnInserted(index, item);
        }

        /// <summary>
        /// Marks the ordered entity cache as dirty after an entity has been
        /// replaced.
        /// </summary>
        /// <param name="index">
        /// The index of the replaced entity.
        /// </param>
        /// <param name="oldItem">
        /// The previous entity.
        /// </param>
        /// <param name="newItem">
        /// The replacement entity.
        /// </param>
        protected override void OnSet(
            int index,
            IEntity oldItem,
            IEntity newItem)
        {
            _cacheDirty = true;
            base.OnSet(index, oldItem, newItem);
        }

        /// <summary>
        /// Marks the ordered entity cache as dirty after an entity has been
        /// removed.
        /// </summary>
        /// <param name="index">
        /// The previous index of the removed entity.
        /// </param>
        /// <param name="item">
        /// The removed entity.
        /// </param>
        protected override void OnRemoved(int index, IEntity item)
        {
            _cacheDirty = true;
            base.OnRemoved(index, item);
        }

        /// <summary>
        /// Clears the ordered entity cache after the collection has been cleared.
        /// </summary>
        protected override void OnCleared()
        {
            _sortedCache.Clear();
            _cacheDirty = true;
            base.OnCleared();
        }

        /// <summary>
        /// Executes the specified action for all entities in their configured order.
        /// </summary>
        /// <param name="action">
        /// The action to execute.
        /// </param>
        private void ForEachOrdered(Action<IEntity> action)
        {
            UpdateCache();

            foreach (IEntity entity in _sortedCache)
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
            _sortedCache.AddRange(this);

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

                return IndexOf(a).CompareTo(IndexOf(b));
            });

            _cacheDirty = false;
        }
    }
}
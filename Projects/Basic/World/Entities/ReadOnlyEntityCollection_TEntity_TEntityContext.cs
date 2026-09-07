using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Represents a strongly typed read-only collection of entities with support
    /// for resolution, ordered loading, updating, drawing, and unloading.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type contained in the collection.
    /// </typeparam>
    /// <typeparam name="TEntityContext">
    /// The context type associated with the entities.
    /// </typeparam>
    public sealed class ReadOnlyEntityCollection<TEntity, TEntityContext> :
        IReadOnlyList<TEntity>,
        IEngineObjectResolver
        where TEntity : class, IEntity
        where TEntityContext : class, IEntityContext
    {
        private readonly TEntity[] _entities;
        private readonly TEntity[] _orderedEntities;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ReadOnlyEntityCollection{TEntity, TEntityContext}"/> class.
        /// </summary>
        /// <param name="entities">
        /// The entities used to create the collection.
        /// </param>
        /// <param name="context">
        /// The optional entity context associated with the collection.
        /// </param>
        public ReadOnlyEntityCollection(
            IEnumerable<TEntity> entities,
            TEntityContext? context = null)
        {
            ArgumentNullException.ThrowIfNull(entities);

            _entities = [.. entities];
            _orderedEntities = CreateOrderedSnapshot(_entities);

            Context = context;
        }

        /// <summary>
        /// Gets the entity context associated with the collection.
        /// </summary>
        public TEntityContext? Context { get; }

        /// <summary>
        /// Gets the number of entities contained in the collection.
        /// </summary>
        public int Count => _entities.Length;

        /// <summary>
        /// Gets the entities in their configured processing order.
        /// </summary>
        public IReadOnlyList<TEntity> OrderedEntities => _orderedEntities;

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        public TEntity this[int index] => _entities[index];

        /// <summary>
        /// Determines whether the collection contains the specified entity.
        /// </summary>
        public bool Contains(TEntity item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Returns the index of the specified entity.
        /// </summary>
        public int IndexOf(TEntity item)
        {
            for (int i = 0; i < _entities.Length; i++)
            {
                if (ReferenceEquals(_entities[i], item))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds an entity with the specified identifier.
        /// </summary>
        public TEntity? Find(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (TEntity entity in _entities)
            {
                if (entity.Id == id)
                    return entity;
            }

            return null;
        }

        /// <summary>
        /// Finds an entity of the specified type with the specified identifier.
        /// </summary>
        public T? Find<T>(string? id)
            where T : class, IEntity
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
        public IEnumerable<T> FindAll<T>(string? @class)
            where T : class, IEntity
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
        public bool TryGet(string? id, out TEntity? result)
        {
            result = Find(id);
            return result is not null;
        }

        /// <summary>
        /// Attempts to find an entity of the specified type with the specified
        /// identifier.
        /// </summary>
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
            foreach (TEntity entity in _orderedEntities)
                entity.Load();
        }

        /// <summary>
        /// Asynchronously loads all entities in their configured order.
        /// </summary>
        public async Task LoadAsync()
        {
            foreach (TEntity entity in _orderedEntities)
                await entity.LoadAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Unloads all entities in reverse configured order.
        /// </summary>
        public void Unload()
        {
            for (int i = _orderedEntities.Length - 1; i >= 0; i--)
                _orderedEntities[i].Unload();
        }

        /// <summary>
        /// Updates all updatable entities in their configured order.
        /// </summary>
        public void Update(SceneUpdateContext context)
        {
            foreach (TEntity entity in _orderedEntities)
            {
                if (entity is IUpdatableEntity updatable)
                    updatable.Update(context);
            }
        }

        /// <summary>
        /// Draws all drawable entities in their configured order.
        /// </summary>
        public void Draw(SceneDrawContext context)
        {
            foreach (TEntity entity in _orderedEntities)
            {
                if (entity is IDrawableEntity drawable)
                    drawable.Draw(context);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the entities.
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return ((IEnumerable<TEntity>)_entities).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEngineReferenceable? IEngineObjectResolver.Find(string? id)
        {
            return (IEngineReferenceable?)Find(id);
        }

        IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(
            string? @class)
        {
            foreach (TEntity entity in FindAll(@class))
                yield return (IEngineReferenceable)entity;
        }

        bool IEngineObjectResolver.TryGet(
            string? id,
            out IEngineReferenceable? result)
        {
            TEntity? entity = Find(id);

            result = entity;
            return entity is not null;
        }

        private static TEntity[] CreateOrderedSnapshot(
            TEntity[] entities)
        {
            TEntity[] result = (TEntity[])entities.Clone();

            Array.Sort(
                result,
                (a, b) =>
                {
                    int orderA = a is IOrderedEntity orderedA
                        ? orderedA.Order
                        : int.MaxValue;

                    int orderB = b is IOrderedEntity orderedB
                        ? orderedB.Order
                        : int.MaxValue;

                    return orderA.CompareTo(orderB);
                });

            return result;
        }
    }
}
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Represents a read-only collection of entities with support for
    /// resolution, ordered loading, updating, drawing, and unloading.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The context type associated with the entities.
    /// </typeparam>
    public sealed class ReadOnlyEntityCollection<TEntityContext> :
        IReadOnlyList<IEntity>,
        IEngineObjectResolver
        where TEntityContext : class, IEntityContext
    {
        private readonly IEntity[] _entities;
        private readonly IEntity[] _orderedEntities;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ReadOnlyEntityCollection{TEntityContext}"/> class.
        /// </summary>
        /// <param name="entities">
        /// The entities used to create the collection.
        /// </param>
        /// <param name="context">
        /// The optional entity context associated with the collection.
        /// </param>
        public ReadOnlyEntityCollection(
            IEnumerable<IEntity> entities,
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
        public IReadOnlyList<IEntity> OrderedEntities => _orderedEntities;

        /// <summary>
        /// Gets the entity at the specified index.
        /// </summary>
        public IEntity this[int index] => _entities[index];

        /// <summary>
        /// Determines whether the collection contains the specified entity.
        /// </summary>
        public bool Contains(IEntity item)
        {
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Returns the index of the specified entity.
        /// </summary>
        public int IndexOf(IEntity item)
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
        public IEntity? Find(string? id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (IEntity entity in _entities)
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

            foreach (IEntity entity in _entities)
            {
                if (entity.Id == id && entity is T typedEntity)
                    return typedEntity;
            }

            return null;
        }

        /// <summary>
        /// Finds all entities with the specified class.
        /// </summary>
        public IEnumerable<IEntity> FindAll(string? @class)
        {
            foreach (IEntity entity in _entities)
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
            foreach (IEntity entity in _entities)
            {
                if (entity.Class == @class && entity is T typedEntity)
                    yield return typedEntity;
            }
        }

        /// <summary>
        /// Attempts to find an entity with the specified identifier.
        /// </summary>
        public bool TryGet(string? id, out IEntity? result)
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
            foreach (IEntity entity in _orderedEntities)
                entity.Load();
        }

        /// <summary>
        /// Asynchronously loads all entities in their configured order.
        /// </summary>
        public async Task LoadAsync()
        {
            foreach (IEntity entity in _orderedEntities)
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
            foreach (IEntity entity in _orderedEntities)
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
            foreach (IEntity entity in _orderedEntities)
            {
                if (entity is IDrawableEntity drawable)
                    drawable.Draw(context);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the entities.
        /// </summary>
        public IEnumerator<IEntity> GetEnumerator()
        {
            return ((IEnumerable<IEntity>)_entities).GetEnumerator();
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
            foreach (IEntity entity in FindAll(@class))
                yield return (IEngineReferenceable)entity;
        }

        bool IEngineObjectResolver.TryGet(
            string? id,
            out IEngineReferenceable? result)
        {
            IEntity? entity = Find(id);

            result = (IEngineReferenceable?)entity;
            return entity != null;
        }

        private static IEntity[] CreateOrderedSnapshot(
            IEntity[] entities)
        {
            IEntity[] result = (IEntity[])entities.Clone();

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
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World;

/// <summary>
/// Provides extension methods for loading, unloading, updating, drawing,
/// filtering, and ordering collections of entities.
/// </summary>
public static class EntityCollectionExtensions
{
    /// <summary>
    /// Initializes all entities that support initialization using the specified
    /// entity context type.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The type of context used to initialize the entities.
    /// </typeparam>
    /// <param name="entities">
    /// The entities to initialize.
    /// </param>
    /// <param name="context">
    /// The context used to initialize the entities.
    /// </param>
    public static void InitializeAll<TEntityContext>(
        this IEnumerable<IEntity> entities,
        TEntityContext context)
        where TEntityContext : IEntityContext
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in entities)
        {
            if (entity is IInitializableEntity<TEntityContext> initializable)
                initializable.Initialize(context);
        }
    }

    /// <summary>
    /// Deinitializes all entities that support initialization using the specified
    /// entity context type.
    /// </summary>
    /// <typeparam name="TEntityContext">
    /// The entity context type associated with the entities.
    /// </typeparam>
    /// <param name="entities">
    /// The entities to deinitialize.
    /// </param>
    public static void DeinitializeAll<TEntityContext>(
        this IEnumerable<IEntity> entities)
        where TEntityContext : IEntityContext
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            if (entity is IInitializableEntity<TEntityContext> initializable)
                initializable.Deinitialize();
        }
    }

    /// <summary>
    /// Loads all entities in the collection.
    /// </summary>
    /// <param name="entities">The entities to load.</param>
    public static void LoadAll(this IEnumerable<IEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
            entity.Load();
    }

    /// <summary>
    /// Loads all entities in the collection asynchronously.
    /// </summary>
    /// <param name="entities">The entities to load.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous load operation.
    /// </returns>
    public static async Task LoadAllAsync(
        this IEnumerable<IEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await entity.LoadAsync(/*cancellationToken*/)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Unloads all entities in the collection.
    /// </summary>
    /// <param name="entities">The entities to unload.</param>
    public static void UnloadAll(this IEnumerable<IEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
            entity.Unload();
    }

    /// <summary>
    /// Loads the entity with the specified identifier.
    /// </summary>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <returns>The loaded entity.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity with the specified identifier was found.
    /// </exception>
    public static IEntity Load(
        this IEnumerable<IEntity> entities,
        string id)
    {
        var entity = Find(entities, id);

        entity.Load();

        return entity;
    }

    /// <summary>
    /// Loads the entity with the specified identifier asynchronously.
    /// </summary>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the
    /// loaded entity.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity with the specified identifier was found.
    /// </exception>
    public static async Task<IEntity> LoadAsync(
        this IEnumerable<IEntity> entities,
        string id,
        CancellationToken cancellationToken = default)
    {
        var entity = Find(entities, id);

        await entity.LoadAsync(/*cancellationToken*/)
            .ConfigureAwait(false);

        return entity;
    }

    /// <summary>
    /// Unloads the entity with the specified identifier.
    /// </summary>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to unload.</param>
    /// <returns>The unloaded entity.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity with the specified identifier was found.
    /// </exception>
    public static IEntity Unload(
        this IEnumerable<IEntity> entities,
        string id)
    {
        var entity = Find(entities, id);

        entity.Unload();

        return entity;
    }

    /// <summary>
    /// Loads the entity with the specified identifier as the requested type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type to search for.
    /// </typeparam>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <returns>The loaded entity.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity of the requested type with the specified identifier was found.
    /// </exception>
    public static TEntity LoadClass<TEntity>(
        this IEnumerable<IEntity> entities,
        string id)
        where TEntity : class, IEntity
    {
        var entity = FindClass<TEntity>(entities, id);

        entity.Load();

        return entity;
    }

    /// <summary>
    /// Loads the entity with the specified identifier as the requested type
    /// asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type to search for.
    /// </typeparam>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to load.</param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation and containing the
    /// loaded entity.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity of the requested type with the specified identifier was found.
    /// </exception>
    public static async Task<TEntity> LoadClassAsync<TEntity>(
        this IEnumerable<IEntity> entities,
        string id,
        CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        var entity = FindClass<TEntity>(entities, id);

        await entity.LoadAsync(/*cancellationToken*/)
            .ConfigureAwait(false);

        return entity;
    }

    /// <summary>
    /// Unloads the entity with the specified identifier as the requested type.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity type to search for.
    /// </typeparam>
    /// <param name="entities">The entities to search.</param>
    /// <param name="id">The identifier of the entity to unload.</param>
    /// <returns>The unloaded entity.</returns>
    /// <exception cref="KeyNotFoundException">
    /// No entity of the requested type with the specified identifier was found.
    /// </exception>
    public static TEntity UnloadClass<TEntity>(
        this IEnumerable<IEntity> entities,
        string id)
        where TEntity : class, IEntity
    {
        var entity = FindClass<TEntity>(entities, id);

        entity.Unload();

        return entity;
    }

    /// <summary>
    /// Updates all entities that support updating.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <param name="context">
    /// The scene update context used for the update operation.
    /// </param>
    public static void UpdateAll(
        this IEnumerable<IEntity> entities,
        SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in entities)
        {
            if (entity is IUpdatableEntity updatable)
                updatable.Update(context);
        }
    }

    /// <summary>
    /// Draws all entities that support drawing.
    /// </summary>
    /// <param name="entities">The entities to draw.</param>
    /// <param name="context">
    /// The scene draw context used for the draw operation.
    /// </param>
    public static void DrawAll(
        this IEnumerable<IEntity> entities,
        SceneDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(context);

        foreach (var entity in entities)
        {
            if (entity is IDrawableEntity drawable)
                drawable.Draw(context);
        }
    }

    /// <summary>
    /// Returns all entities sorted by their order.
    /// </summary>
    /// <param name="entities">The entities to order.</param>
    /// <param name="sortDirection">
    /// The direction in which the entities are ordered.
    /// </param>
    /// <param name="fallbackOrder">
    /// The order value used for entities that do not implement
    /// <see cref="IOrderedEntity"/>.
    /// </param>
    /// <returns>
    /// All entities sorted by their effective order.
    /// </returns>
    public static IEnumerable<IEntity> GetOrdered(
        this IEnumerable<IEntity> entities,
        SortDirection sortDirection = SortDirection.Ascending,
        int fallbackOrder = 0)
    {
        ArgumentNullException.ThrowIfNull(entities);

        int GetOrder(IEntity entity) =>
            entity is IOrderedEntity ordered
                ? ordered.Order
                : fallbackOrder;

        return sortDirection switch
        {
            SortDirection.Ascending => entities.OrderBy(GetOrder),
            SortDirection.Descending => entities.OrderByDescending(GetOrder),
            _ => throw new ArgumentOutOfRangeException(nameof(sortDirection))
        };
    }

    /// <summary>
    /// Returns all entities that provide executable script logic.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <returns>
    /// An enumerable containing all entities that implement
    /// <see cref="IScriptEntity"/>.
    /// </returns>
    public static IEnumerable<IEntity> GetExecutables(
        this IEnumerable<IEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        return entities.Where(entity => entity is IScriptEntity);
    }

    private static IEntity Find(
        IEnumerable<IEntity> entities,
        string id)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        foreach (var entity in entities)
        {
            if (string.Equals(entity.Id, id, StringComparison.Ordinal))
                return entity;
        }

        throw new KeyNotFoundException(
            $"Entity with identifier '{id}' was not found.");
    }

    private static TEntity FindClass<TEntity>(
        IEnumerable<IEntity> entities,
        string id)
        where TEntity : class, IEntity
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        foreach (var entity in entities)
        {
            if (entity is TEntity typedEntity &&
                string.Equals(entity.Id, id, StringComparison.Ordinal))
            {
                return typedEntity;
            }
        }

        throw new KeyNotFoundException(
            $"Entity of type '{typeof(TEntity).FullName}' with identifier '{id}' was not found.");
    }
}
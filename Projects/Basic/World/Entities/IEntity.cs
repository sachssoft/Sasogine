using Sachssoft.Sasogine.Common;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines the base contract for entities used by the Sasogine world system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An entity represents an identifiable engine object that participates in
    /// the loading and unloading lifecycle.
    /// </para>
    /// <para>
    /// <see cref="Integrity"/> represents the persistent health or validity state
    /// of the entity, while <see cref="ActivityState"/> represents its current
    /// runtime activity.
    /// </para>
    /// <para>
    /// Entities that require update or drawing behavior can additionally
    /// implement the corresponding runtime interfaces.
    /// </para>
    /// </remarks>
    public interface IEntity : IEngineReferenceable
    {
        /// <summary>
        /// Occurs after the entity has been successfully loaded.
        /// </summary>
        event EventHandler? Loaded;

        /// <summary>
        /// Occurs after the entity has been unloaded.
        /// </summary>
        event EventHandler? Unloaded;

        /// <summary>
        /// Occurs when the integrity state of the entity changes.
        /// </summary>
        event EventHandler? StatusChanged;

        /// <summary>
        /// Occurs when the activity state of the entity changes.
        /// </summary>
        event EventHandler? ActivityStateChanged;

        /// <summary>
        /// Gets the unique identifier of the entity.
        /// </summary>
        /// <value>
        /// The entity identifier, or <see langword="null"/> if no identifier
        /// has been assigned.
        /// </value>
        string? Id { get; }

        /// <summary>
        /// Gets the class associated with the entity.
        /// </summary>
        /// <value>
        /// The entity class, or <see langword="null"/> if no class has been
        /// assigned.
        /// </value>
        string? Class { get; }

        /// <summary>
        /// Gets the optional data context associated with the entity.
        /// </summary>
        /// <value>
        /// The data context, or <see langword="null"/> if no data context is
        /// associated with the entity.
        /// </value>
        object? DataContext { get; }

        /// <summary>
        /// Gets the current integrity state of the entity.
        /// </summary>
        /// <value>
        /// The persistent health or validity state of the entity.
        /// </value>
        EntityIntegrity Integrity { get; }

        /// <summary>
        /// Gets the current activity state of the entity.
        /// </summary>
        /// <value>
        /// The temporary runtime activity state of the entity.
        /// </value>
        ActivityState ActivityState { get; }

        /// <summary>
        /// Loads the entity and its required resources.
        /// </summary>
        void Load();

        /// <summary>
        /// Asynchronously loads the entity and its required resources.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous loading operation.
        /// </returns>
        Task LoadAsync();

        /// <summary>
        /// Unloads the entity and releases resources associated with it.
        /// </summary>
        void Unload();
    }
}
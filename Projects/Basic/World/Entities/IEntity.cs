using Sachssoft.Sasogine.Common;
using System;
using System.Threading;
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
    /// The loading state is represented independently by <see cref="IsLoaded"/>.
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
        event EventHandler? IntegrityChanged;

        /// <summary>
        /// Occurs when the activity state of the entity changes.
        /// </summary>
        event EventHandler? ActivityStateChanged;

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
        /// Gets a value indicating whether the entity is currently loaded.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the entity is loaded; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool IsLoaded { get; }

        /// <summary>
        /// Loads the entity and its required resources.
        /// </summary>
        void Load();

        /// <summary>
        /// Asynchronously loads the entity and its required resources.
        /// </summary>
        /// <param name="cancellationToken">
        /// A token that can be used to cancel the loading operation.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous loading operation.
        /// </returns>
        Task LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Unloads the entity and releases resources associated with it.
        /// </summary>
        void Unload();
    }
}
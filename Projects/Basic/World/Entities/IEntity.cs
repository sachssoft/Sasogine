using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.World
{
    /// <summary>
    /// Defines the base contract for entities used by the engine world system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An entity represents an identifiable engine object that participates in
    /// the engine object lifecycle.
    /// </para>
    /// <para>
    /// <see cref="Integrity"/> represents the persistent integrity state of the
    /// entity, while <see cref="ActivityState"/> represents its current runtime
    /// activity state.
    /// </para>
    /// <para>
    /// The loading state is managed independently through the engine object
    /// lifecycle.
    /// </para>
    /// <para>
    /// Entities can additionally implement runtime interfaces for specialized
    /// update, rendering, or other engine behavior.
    /// </para>
    /// </remarks>
    public interface IEntity : IEngineReferenceable, IEngineObject
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
        /// Gets the current integrity state of the entity.
        /// </summary>
        /// <value>
        /// The persistent integrity state of the entity.
        /// </value>
        EntityIntegrity Integrity { get; }

        /// <summary>
        /// Gets the current activity state of the entity.
        /// </summary>
        /// <value>
        /// The current runtime activity state of the entity.
        /// </value>
        ActivityState ActivityState { get; }
    }
}
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Base interface for all assets (e.g., Texture2D, Model, Sound).
    /// Provides lifecycle events, synchronous/asynchronous loading, and error handling.
    /// </summary>
    public interface IAsset :
        IEngineObject,
        IInitializableEngineObject,
        IAssemblyContract
    {
        /// <summary>
        /// Gets the relative path of the asset within the content or project structure.
        /// </summary>
        string? RelativePath { get; }

        /// <summary>
        /// Occurs after the asset instance has been successfully loaded.
        /// </summary>
        event EventHandler? Loaded;

        /// <summary>
        /// Occurs after the asset instance has been unloaded.
        /// </summary>
        event EventHandler? Unloaded;

        /// <summary>
        /// Occurs when the source used by the asset changes.
        /// </summary>
        event EventHandler? LoaderSourceChanged;

        /// <summary>
        /// Occurs when the runtime asset instance changes.
        /// </summary>
        event EventHandler? InstanceChanged;

        /// <summary>
        /// Gets a value indicating whether an error occurred while processing the asset.
        /// </summary>
        bool HasError { get; }

        /// <summary>
        /// Gets the most recent exception associated with the asset.
        /// </summary>
        /// <value>
        /// The captured exception, or <see langword="null"/> if no error exists.
        /// </value>
        Exception? Exception { get; }

        /// <summary>
        /// Gets or sets the resource source used to provide the asset data.
        /// </summary>
        ResourceSourceBase? LoaderSource { get; set; }

        /// <summary>
        /// Gets a value indicating whether a runtime asset instance is currently available.
        /// </summary>
        bool HasInstance { get; }

        /// <summary>
        /// Gets the currently loaded runtime asset instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The asset does not currently contain a loaded runtime instance.
        /// </exception>
        object Instance { get; }

        /// <summary>
        /// Initializes the asset using the specified asset context.
        /// </summary>
        /// <param name="context">
        /// The asset context providing runtime dependencies.
        /// </param>
        void Initialize(AssetContext context);

        void IAssemblyContract.Initialize()
        {
            // Für Dritte nicht implementierbar
        }
    }
}
using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Base interface for all assets (e.g., Texture2D, Model, Sound).
    /// Provides lifecycle events, synchronous/asynchronous loading, and error handling.
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// Fired after the asset instance has been successfully loaded.
        /// </summary>
        event EventHandler? Loaded;

        /// <summary>
        /// Fired after the asset instance has been unloaded.
        /// </summary>
        event EventHandler? Unloaded;

        /// <summary>
        /// Fired when the source/loader of the asset has been changed.
        /// </summary>
        event EventHandler? LoaderSourceChanged;

        /// <summary>
        /// Fired when the asset instance changes (e.g., loaded, reloaded, unloaded).
        /// </summary>
        event EventHandler? InstanceChanged;

        /// <summary>
        /// True if the asset instance is currently loaded.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// True if an error occurred during loading or building.
        /// </summary>
        bool HasError { get; }

        /// <summary>
        /// Returns the exception that occurred during loading or building, if any.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// The loader/source used to provide the asset stream.
        /// Can be replaced to reload a different source.
        /// </summary>
        LoaderBase? LoaderSource { get; set; }

        /// <summary>
        /// The currently loaded asset instance.
        /// </summary>
        object? Instance { get; }

        /// <summary>
        /// Synchronously loads the asset if not already loaded.
        /// </summary>
        void Load();

        /// <summary>
        /// Asynchronously loads the asset if not already loaded.
        /// </summary>
        Task LoadAsync();

        /// <summary>
        /// Unloads the asset instance and disposes it if necessary.
        /// </summary>
        void Unload();
    }
}
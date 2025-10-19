using Sachssoft.Inspection;
using Sachssoft.Sasogine.Containers;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Provides a generic base implementation for assets that can be loaded from an <see cref="IAssetSource"/>.
    /// Handles loading, unloading, building, and destruction of assets, and exposes lifecycle events.
    /// </summary>
    /// <typeparam name="T">The concrete type of the asset to load.</typeparam>
    public abstract class AssetBase<T> : NotifyingElement, IAsset where T : class
    {
        private IAssetSource? _source;
        private bool _isLoaded;
        private T? _instance;

        /// <summary>
        /// Occurs after the asset has been successfully loaded.
        /// </summary>
        public event EventHandler? Loaded;

        /// <summary>
        /// Occurs after the asset has been unloaded.
        /// </summary>
        public event EventHandler? Unloaded;

        /// <summary>
        /// Occurs when the <see cref="Source"/> property has been changed.
        /// </summary>
        public event EventHandler? SourceChanged;

        /// <summary>
        /// Occurs after the asset has been built by <see cref="Build(Stream)"/>.
        /// </summary>
        public event EventHandler? AssetBuilt;

        /// <summary>
        /// Loads the asset from the current <see cref="Source"/>.
        /// If the asset is already loaded, this method does nothing.
        /// </summary>
        public void Load()
        {
            if (_isLoaded)
                return;

            if (_source != null)
            {
                using var stream = _source.Open();
                _instance = Build(stream);
                AssetBuilt?.Invoke(this, EventArgs.Empty);
            }

            _isLoaded = true;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Ensures the asset is loaded and returns it.
        /// If the asset is not already loaded, this method loads it first.
        /// </summary>
        /// <returns>The loaded asset instance, or <c>null</c> if loading failed.</returns>
        public T? LoadDirect()
        {
            if (_instance == null)
                Load();
            return _instance;
        }

        /// <summary>
        /// Unloads the asset and calls <see cref="Destroy(T)"/> if necessary.
        /// If the asset is not loaded, this method does nothing.
        /// </summary>
        public void Unload()
        {
            if (!_isLoaded)
                return;

            if (_instance != null)
            {
                try
                {
                    Destroy(_instance);
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
                finally
                {
                    _instance = null;
                }
            }

            _isLoaded = false;
            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Builds an asset instance from the specified data stream.
        /// Override this method in derived classes to create a concrete asset object.
        /// </summary>
        /// <param name="stream">The stream containing the asset data.</param>
        /// <returns>The created asset instance, or <c>null</c> if building failed.</returns>
        protected virtual T? Build(Stream stream) => null;

        /// <summary>
        /// Destroys a previously built asset instance.
        /// The default implementation disposes the asset if it implements <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="asset">The asset instance to destroy.</param>
        protected virtual void Destroy(T asset)
        {
            if (asset is IDisposable disposable)
                disposable.Dispose();
        }

        /// <summary>
        /// Gets the loaded asset instance.
        /// </summary>
        public T? Instance => _instance;

        object? IAsset.Instance => _instance;

        /// <summary>
        /// Gets or sets the source from which the asset is loaded.
        /// Changing the source automatically unloads the current asset
        /// and loads the new one (if not <c>null</c>).
        /// </summary>
        public IAssetSource? Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    Unload();
                    _source = value;
                    SourceChanged?.Invoke(this, EventArgs.Empty);

                    if (_source != null && AutoLoad)
                        Load();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool AutoLoad { get; set; } = false;

        /// <summary>
        /// Gets a value indicating whether the asset is currently loaded.
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// Gets a value indicating whether an error occurred during loading or processing.
        /// Derived classes can set this flag if <see cref="Build(Stream)"/> fails.
        /// </summary>
        public bool IsError => Exception != null;

        /// <summary>
        /// Gets the exception that occurred during loading or building the asset, if any.
        /// </summary>
        public Exception? Exception { get; protected set; }
    }
}

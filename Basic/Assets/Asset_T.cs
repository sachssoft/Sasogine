using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Base class for assets (e.g., Texture2D, Sound, Model) with support for
    /// synchronous/asynchronous loading, unloading, and event notifications.
    /// </summary>
    /// <typeparam name="T">Concrete type of the asset instance</typeparam>
    public abstract class AssetBase<T, TDefinition> : EngineObject<TDefinition>, IAsset
        where T : class
        where TDefinition : class, IAssetDefinition, new()
    {
        //private AssetDefinitionRegistry _registry = null!;
        //private TDefinition _definition = null!;
        private bool _loaded;          // True if the asset is loaded
        private T? _instance;          // The loaded asset instance
        private readonly object _sync = new(); // Lock object for thread-safety
        private ResourceSourceBase? _loaderSource;      // Backing field for LoaderSource

        /// <summary>
        /// Fired after the asset instance has been successfully loaded.
        /// </summary>
        public event EventHandler? Loaded;

        /// <summary>
        /// Fired after the asset instance has been unloaded.
        /// </summary>
        public event EventHandler? Unloaded;

        /// <summary>
        /// Fired when the LoaderSource property is changed.
        /// </summary>
        public event EventHandler? LoaderSourceChanged;

        /// <summary>
        /// Fired when the asset instance changes (e.g., loaded, reloaded, unloaded).
        /// </summary>
        public event EventHandler? InstanceChanged;

        public AssetBase() { }

        public string? RelativePath { get; }

        /// <summary>
        /// True if an error occurred during loading or building the asset.
        /// </summary>
        public bool HasError => Exception != null;

        /// <summary>
        /// Stores the exception that occurred during loading or building, if any.
        /// </summary>
        public Exception? Exception { get; protected set; }

        /// <summary>
        /// The loader/source used to provide the asset stream.
        /// Can be replaced to reload a different source.
        /// Fires <see cref="LoaderSourceChanged"/> when changed.
        /// </summary>
        public ResourceSourceBase? LoaderSource
        {
            get => _loaderSource;
            set
            {
                if (!ReferenceEquals(_loaderSource, value))
                {
                    _loaderSource = value;
                    LoaderSourceChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the loaded asset instance.
        /// </summary>
        public T? Instance => _instance;

        object? IAsset.Instance => _instance;

        IEngineObjectDefinition? IEngineObject.Definition => Definition;

        /// <summary>
        /// Synchronously loads the asset if not already loaded.
        /// Fires Loaded and InstanceChanged events.
        /// </summary>
        public override void Load()
        {
            lock (_sync)
            {
                if (_loaded)
                    return;

                if (_loaderSource == null)
                    throw new InvalidOperationException("LoaderSource is not set.");

                try
                {
                    _instance = Build(_loaderSource.GetStream());
                    Exception = null;
                    _loaded = true;
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    _instance = default;
                    _loaded = false;
                }

                Loaded?.Invoke(this, EventArgs.Empty);
                InstanceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Asynchronously loads the asset if not already loaded.
        /// Fires Loaded and InstanceChanged events.
        /// </summary>
        public override async Task LoadAsync()
        {
            if (_loaded)
                return;

            if (_loaderSource == null)
                throw new InvalidOperationException("LoaderSource is not set.");

            try
            {
                var stream = await _loaderSource.GetStreamAsync().ConfigureAwait(false);
                lock (_sync)
                {
                    _instance = Build(stream);
                    Exception = null;
                    _loaded = true;
                }

                Loaded?.Invoke(this, EventArgs.Empty);
                InstanceChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                lock (_sync)
                {
                    Exception = ex;
                    _instance = default;
                    _loaded = false;
                }
            }
        }

        /// <summary>
        /// Ensures the asset is loaded and returns it synchronously.
        /// </summary>
        public T? GetOrLoad()
        {
            if (_instance == null)
                Load();
            return _instance;
        }

        /// <summary>
        /// Ensures the asset is loaded and returns it asynchronously.
        /// </summary>
        public async Task<T?> GetOrLoadAsync()
        {
            if (_instance == null)
                await LoadAsync().ConfigureAwait(false);
            return _instance;
        }

        /// <summary>
        /// Unloads the asset instance and disposes it if necessary.
        /// Fires Unloaded and InstanceChanged events.
        /// </summary>
        public override void Unload()
        {
            lock (_sync)
            {
                if (!_loaded)
                    return;

                //if (Definition != null)
                // ...  

                if (_instance != null)
                {
                    try
                    {
                        DisposeInstance(_instance);
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;
                    }
                    finally
                    {
                        _instance = default;
                    }
                }

                _loaded = false;

                Unloaded?.Invoke(this, EventArgs.Empty);
                InstanceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Builds an asset instance from the provided stream.
        /// Override in derived classes to create concrete asset objects.
        /// </summary>
        protected virtual T? Build(Stream stream) => default;

        /// <summary>
        /// Destroys or disposes a previously built asset instance.
        /// Default implementation disposes if it implements IDisposable.
        /// </summary>
        protected virtual void DisposeInstance(T asset)
        {
            if (asset is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
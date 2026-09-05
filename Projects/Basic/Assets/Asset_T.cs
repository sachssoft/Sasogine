using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Provides a base implementation for assets that support synchronous
/// and asynchronous loading, unloading, and instance notifications.
/// </summary>
/// <typeparam name="T">
/// The runtime type of the loaded asset instance.
/// </typeparam>
/// <typeparam name="TDefinition">
/// The type of definition used to configure the asset.
/// </typeparam>
public abstract class AssetBase<T, TDefinition> :
    EngineObject<TDefinition>,
    IAsset
    where T : class
    where TDefinition : class, IAssetDefinition
{
    private readonly object _sync = new();

    private ResourceSourceBase? _loaderSource;
    private T? _instance;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="AssetBase{T, TDefinition}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the asset.
    /// </param>
    protected AssetBase(TDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Occurs after the asset instance has been successfully loaded.
    /// </summary>
    public event EventHandler? Loaded;

    /// <summary>
    /// Occurs after the asset instance has been unloaded.
    /// </summary>
    public event EventHandler? Unloaded;

    /// <summary>
    /// Occurs when <see cref="LoaderSource"/> changes.
    /// </summary>
    public event EventHandler? LoaderSourceChanged;

    /// <summary>
    /// Occurs when the loaded asset instance changes.
    /// </summary>
    public event EventHandler? InstanceChanged;

    /// <summary>
    /// Gets the relative path associated with the asset, if available.
    /// </summary>
    public string? RelativePath { get; }

    /// <summary>
    /// Gets a value indicating whether an error occurred while loading,
    /// building, or unloading the asset.
    /// </summary>
    public bool HasError => Exception != null;

    /// <summary>
    /// Gets the exception that occurred while loading, building,
    /// or unloading the asset, if any.
    /// </summary>
    public Exception? Exception { get; protected set; }

    /// <summary>
    /// Gets or sets the resource source used to provide the asset data.
    /// </summary>
    public ResourceSourceBase? LoaderSource
    {
        get => _loaderSource;
        set
        {
            if (ReferenceEquals(_loaderSource, value))
                return;

            _loaderSource = value;

            OnLoaderSourceChanged();
        }
    }

    /// <summary>
    /// Gets the currently loaded asset instance.
    /// </summary>
    public T? Instance => _instance;

    object? IAsset.Instance => _instance;

    /// <summary>
    /// Ensures that the asset is loaded and returns its instance.
    /// </summary>
    /// <returns>
    /// The loaded asset instance, or <see langword="null"/> if loading
    /// did not produce an instance.
    /// </returns>
    public T? GetOrLoad()
    {
        if (!IsLoaded)
            Load();

        return _instance;
    }

    /// <summary>
    /// Ensures that the asset is loaded asynchronously and returns its instance.
    /// </summary>
    /// <returns>
    /// A task containing the loaded asset instance, or <see langword="null"/>
    /// if loading did not produce an instance.
    /// </returns>
    public async Task<T?> GetOrLoadAsync()
    {
        if (!IsLoaded)
            await LoadAsync().ConfigureAwait(false);

        return _instance;
    }

    /// <summary>
    /// Loads and builds the asset instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="LoaderSource"/> is not set.
    /// </exception>
    protected override void OnLoad()
    {
        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException(
                "LoaderSource is not set.");

        try
        {
            using Stream stream = source.GetStream();

            T? instance = Build(stream);

            lock (_sync)
            {
                _instance = instance;
                Exception = null;
            }

            OnLoaded();
            OnInstanceChanged();
        }
        catch (Exception exception)
        {
            lock (_sync)
            {
                _instance = null;
                Exception = exception;
            }

            throw;
        }
    }

    /// <summary>
    /// Asynchronously loads and builds the asset instance.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous loading operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="LoaderSource"/> is not set.
    /// </exception>
    protected override async Task OnLoadAsync()
    {
        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException(
                "LoaderSource is not set.");

        try
        {
            using Stream stream =
                await source.GetStreamAsync().ConfigureAwait(false);

            T? instance = Build(stream);

            lock (_sync)
            {
                _instance = instance;
                Exception = null;
            }

            OnLoaded();
            OnInstanceChanged();
        }
        catch (Exception exception)
        {
            lock (_sync)
            {
                _instance = null;
                Exception = exception;
            }

            throw;
        }
    }

    /// <summary>
    /// Unloads and disposes the current asset instance.
    /// </summary>
    protected override void OnUnload()
    {
        T? instance;

        lock (_sync)
        {
            instance = _instance;
            _instance = null;
        }

        if (instance != null)
        {
            try
            {
                DisposeInstance(instance);
                Exception = null;
            }
            catch (Exception exception)
            {
                Exception = exception;
                throw;
            }
        }

        OnUnloaded();
        OnInstanceChanged();
    }

    /// <summary>
    /// Builds an asset instance from the specified resource stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the asset data.
    /// </param>
    /// <returns>
    /// The constructed asset instance.
    /// </returns>
    protected virtual T? Build(Stream stream)
    {
        return default;
    }

    /// <summary>
    /// Releases a previously built asset instance.
    /// </summary>
    /// <param name="asset">
    /// The asset instance to release.
    /// </param>
    /// <remarks>
    /// The default implementation disposes the instance when it implements
    /// <see cref="IDisposable"/>.
    /// </remarks>
    protected virtual void DisposeInstance(T asset)
    {
        if (asset is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Raises the <see cref="Loaded"/> event.
    /// </summary>
    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Unloaded"/> event.
    /// </summary>
    protected virtual void OnUnloaded()
    {
        Unloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="LoaderSourceChanged"/> event.
    /// </summary>
    protected virtual void OnLoaderSourceChanged()
    {
        LoaderSourceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="InstanceChanged"/> event.
    /// </summary>
    protected virtual void OnInstanceChanged()
    {
        InstanceChanged?.Invoke(this, EventArgs.Empty);
    }
}
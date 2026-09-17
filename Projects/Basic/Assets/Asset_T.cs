using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Resources;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Provides a base implementation for managed assets that support synchronous
/// and asynchronous loading, unloading, runtime instance management, and error
/// state tracking.
/// </summary>
/// <typeparam name="T">
/// The runtime type of the loaded asset instance.
/// </typeparam>
/// <typeparam name="TDefinition">
/// The definition type used to configure the asset.
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="AssetBase{T, TDefinition}"/> loads asset data from a
/// <see cref="ResourceSourceBase"/> and builds a managed runtime instance.
/// </para>
/// <para>
/// Loading and unloading errors are exposed through <see cref="Exception"/>,
/// <see cref="HasError"/>, and the <see cref="Error"/> event.
/// </para>
/// <para>
/// When <see cref="ThrowOnError"/> is enabled, errors are rethrown after
/// <see cref="OnError(Exception)"/> has been invoked.
/// </para>
/// </remarks>
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
    protected AssetBase(
        TDefinition definition)
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
    /// Occurs when the loaded runtime instance changes.
    /// </summary>
    public event EventHandler? InstanceChanged;

    /// <summary>
    /// Occurs when an error is encountered while processing the asset.
    /// </summary>
    public event EventHandler<AssetErrorEventArgs>? Error;

    /// <summary>
    /// Gets the relative path associated with the asset, if available.
    /// </summary>
    public string? RelativePath { get; }

    /// <summary>
    /// Gets a value indicating whether an error is currently associated with
    /// the asset.
    /// </summary>
    public bool HasError => Exception != null;

    /// <summary>
    /// Gets the most recent exception associated with the asset.
    /// </summary>
    /// <value>
    /// The captured exception, or <see langword="null"/> if no error is
    /// currently associated with the asset.
    /// </value>
    public Exception? Exception { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether asset errors are rethrown after
    /// they have been reported through <see cref="OnError(Exception)"/>.
    /// </summary>
    public bool ThrowOnError { get; set; }

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
    /// Gets the currently loaded runtime instance.
    /// </summary>
    public T? Instance => _instance;

    object? IAsset.Instance => _instance;

    /// <summary>
    /// Ensures that the asset is loaded and returns its runtime instance.
    /// </summary>
    public T? GetOrLoad()
    {
        if (!IsLoaded)
            Load();

        return _instance;
    }

    /// <summary>
    /// Ensures that the asset is loaded asynchronously and returns its runtime
    /// instance.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the loading operation.
    /// </param>
    /// <returns>
    /// A task containing the loaded asset instance, or <see langword="null"/>
    /// if loading did not produce an instance.
    /// </returns>
    public async Task<T?> GetOrLoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsLoaded)
            await LoadAsync(cancellationToken).ConfigureAwait(false);

        return _instance;
    }

    /// <summary>
    /// Loads the asset data and builds the runtime instance.
    /// </summary>
    protected override void OnLoad()
    {
        try
        {
            LoadCore();
        }
        catch (Exception exception)
        {
            OnError(exception);

            if (ThrowOnError)
                throw;
        }
    }

    /// <summary>
    /// Asynchronously loads the asset data and builds the runtime instance.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the loading operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous loading operation.
    /// </returns>
    protected override async Task OnLoadAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await LoadCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            OnError(exception);

            if (ThrowOnError)
                throw;
        }
    }

    /// <summary>
    /// Unloads and releases the current runtime asset instance.
    /// </summary>
    protected override void OnUnload()
    {
        try
        {
            UnloadCore();
        }
        catch (Exception exception)
        {
            OnError(exception);

            if (ThrowOnError)
                throw;
        }
    }

    /// <summary>
    /// Builds a runtime asset instance from the specified resource stream.
    /// </summary>
    protected virtual T? Build(Stream stream)
    {
        return default;
    }

    /// <summary>
    /// Releases a previously built runtime asset instance.
    /// </summary>
    protected virtual void DisposeInstance(T asset)
    {
        if (asset is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Handles an error that occurred while processing the asset and raises the
    /// <see cref="Error"/> event.
    /// </summary>
    protected virtual void OnError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_sync)
        {
            Exception = exception;
            _instance = null;
        }

        Error?.Invoke(this, new AssetErrorEventArgs(exception));
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

    private void LoadCore()
    {
        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException("LoaderSource is not set.");

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

    private async Task LoadCoreAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException("LoaderSource is not set.");

        using Stream stream = await source.GetStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        T? instance = Build(stream);

        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _instance = instance;
            Exception = null;
        }

        OnLoaded();
        OnInstanceChanged();
    }

    private void UnloadCore()
    {
        T? instance;

        lock (_sync)
        {
            instance = _instance;
            _instance = null;
        }

        if (instance != null)
            DisposeInstance(instance);

        Exception = null;

        OnUnloaded();
        OnInstanceChanged();
    }
}
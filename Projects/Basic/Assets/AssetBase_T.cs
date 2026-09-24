using Sachssoft.Engine;
using Sachssoft.Engine.Collections;
using Sachssoft.Engine.Resources;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Provides a base implementation for managed assets that support synchronous
/// and asynchronous loading, unloading, runtime instance management, and error
/// state tracking.
/// </summary>
/// <typeparam name="T">The runtime type of the loaded asset instance.</typeparam>
/// <typeparam name="TDefinition">
/// The definition type used to configure the asset.
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="AssetBase{T, TDefinition}"/> loads asset data from a
/// <see cref="ResourceSourceBase"/> and builds a managed runtime instance.
/// </para>
/// <para>
/// Runtime dependencies are provided through an <see cref="AssetContext"/>
/// when the asset is initialized.
/// </para>
/// <para>
/// An asset must be initialized before it can be loaded.
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
    /// <param name="definition">The definition associated with the asset.</param>
    protected AssetBase(TDefinition definition)
        : base(
            definition,
            id: definition.Id,
            @class: definition.Class)
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
    /// Gets a value indicating whether an error is currently associated
    /// with the asset.
    /// </summary>
    public bool HasError => Exception is not null;

    /// <summary>
    /// Gets the most recent exception associated with the asset.
    /// </summary>
    /// <value>
    /// The captured exception, or <see langword="null"/> if no error exists.
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
    /// Gets a value indicating whether a runtime instance is currently available.
    /// </summary>
    public bool HasInstance => _instance is not null;

    /// <summary>
    /// Gets the currently loaded runtime instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The asset does not currently contain a loaded runtime instance.
    /// If loading previously failed, the original error is available through
    /// <see cref="System.Exception.InnerException"/>.
    /// </exception>
    public T Instance => _instance ?? throw CreateInstanceException();

    object IAsset.Instance => Instance;

    /// <summary>
    /// Gets a value indicating whether the asset is currently initialized.
    /// </summary>
    public bool IsInitialized => Context is not null;

    /// <summary>
    /// Gets the runtime context associated with this asset.
    /// </summary>
    /// <value>
    /// The current asset context, or <see langword="null"/> if the asset
    /// has not been initialized.
    /// </value>
    protected AssetContext? Context { get; private set; }

    /// <summary>
    /// Initializes the asset using the specified asset context.
    /// </summary>
    /// <param name="context">
    /// The asset context providing runtime dependencies.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Initialize(AssetContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsInitialized)
            return;

        Context = context;
        context.Source.ReferenceChanged += SourceReferenceChanged;
        OnInitialize(context);
    }

    void IInitializableEngineObject.Initialize(IEngineObjectContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is not AssetContext assetContext)
        {
            throw new ArgumentException(
                $"The context must be of type '{typeof(AssetContext).FullName}'.",
                nameof(context));
        }

        Initialize(assetContext);
    }

    /// <summary>
    /// Deinitializes the asset and releases its initialization-specific state.
    /// </summary>
    public void Deinitialize()
    {
        if (!IsInitialized)
            return;

        AssetContext context = Context!;

        OnDeinitialize();
        context.Source.ReferenceChanged -= SourceReferenceChanged;
        Context = null;
    }

    /// <summary>
    /// Called when the asset is initialized.
    /// </summary>
    /// <param name="context">
    /// The asset context providing runtime dependencies.
    /// </param>
    protected virtual void OnInitialize(AssetContext context)
    {
    }

    /// <summary>
    /// Called when the asset is deinitialized.
    /// </summary>
    protected virtual void OnDeinitialize()
    {
    }

    /// <summary>
    /// Called when a reference in the associated asset collection changes.
    /// </summary>
    /// <param name="asset">The asset affected by the reference change.</param>
    protected virtual void OnReferenceChanged(IAsset asset)
    {
    }

    /// <summary>
    /// Ensures that the asset is loaded and returns its runtime instance.
    /// </summary>
    /// <returns>The loaded runtime instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// The asset has not been initialized, loading did not produce a runtime
    /// instance, or loading previously failed.
    /// </exception>
    public T GetOrLoad()
    {
        if (!IsLoaded)
            Load();

        return _instance ?? throw CreateInstanceException();
    }

    /// <summary>
    /// Ensures that the asset is loaded asynchronously and returns its
    /// runtime instance.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token used to cancel the loading operation.
    /// </param>
    /// <returns>A task containing the loaded runtime instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// The asset has not been initialized, loading did not produce a runtime
    /// instance, or loading previously failed.
    /// </exception>
    public async Task<T> GetOrLoadAsync(
        CancellationToken cancellationToken = default)
    {
        if (!IsLoaded)
            await LoadAsync(cancellationToken).ConfigureAwait(false);

        return _instance ?? throw CreateInstanceException();
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
    /// A token used to cancel the loading operation.
    /// </param>
    /// <returns>A task representing the asynchronous loading operation.</returns>
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
    /// <param name="stream">The stream containing the asset data.</param>
    /// <returns>The created runtime instance.</returns>
    protected abstract T Build(Stream stream);

    /// <summary>
    /// Releases a previously built runtime asset instance.
    /// </summary>
    /// <param name="asset">The runtime instance to release.</param>
    protected virtual void DisposeInstance(T asset)
    {
        if (asset is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>
    /// Handles an error that occurred while processing the asset and raises
    /// the <see cref="Error"/> event.
    /// </summary>
    /// <param name="exception">The exception associated with the error.</param>
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

    private void SourceReferenceChanged(
        object? sender,
        ReferenceChangedEventArgs<IAsset> e)
    {
        if (ReferenceEquals(e.Item, this))
            return;

        OnReferenceChanged(e.Item);
    }

    private InvalidOperationException CreateInstanceException()
    {
        return HasError
            ? new InvalidOperationException(
                "The asset instance is unavailable because the asset failed to load.",
                Exception)
            : new InvalidOperationException(
                "The asset instance is not loaded.");
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException(
                "The asset must be initialized before it can be loaded.");
        }
    }

    private void LoadCore()
    {
        EnsureInitialized();

        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException(
                "LoaderSource is not set.");

        using Stream stream = source.GetStream();

        T instance = Build(stream) ??
            throw new InvalidOperationException(
                "The asset builder returned a null instance.");

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
        EnsureInitialized();

        ResourceSourceBase source = _loaderSource ??
            throw new InvalidOperationException(
                "LoaderSource is not set.");

        using Stream stream =
            await source.GetStreamAsync(cancellationToken)
                .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        T instance = Build(stream) ??
            throw new InvalidOperationException(
                "The asset builder returned a null instance.");

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

        if (instance is not null)
            DisposeInstance(instance);

        Exception = null;

        OnUnloaded();
        OnInstanceChanged();
    }
}
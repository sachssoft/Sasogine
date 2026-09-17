using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Provides storage and lifecycle management for game assets.
/// </summary>
public class AssetStore : IReadOnlyAssetStore
{
    private readonly GameApplicationBase _gameApplication;
    private readonly AssetCollection _assets;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetStore"/> class.
    /// </summary>
    /// <param name="application">
    /// The game application associated with the asset store.
    /// </param>
    public AssetStore(GameApplicationBase application)
    {
        ArgumentNullException.ThrowIfNull(application);

        _gameApplication = application;

        _assets = [];

        IEnumerable<IAsset>? integratedAssets = CreateIntegratedAssets();

        if (integratedAssets is not null)
        {
            foreach (IAsset asset in integratedAssets)
                _assets.Add(asset);
        }
    }

    /// <summary>
    /// Gets the game application associated with the asset store.
    /// </summary>
    public GameApplicationBase GameApplication => _gameApplication;

    /// <summary>
    /// Gets the graphics device associated with the game application.
    /// </summary>
    public GraphicsDevice GraphicsDevice => _gameApplication.GraphicsDevice;

    /// <summary>
    /// Gets the number of assets contained in the store.
    /// </summary>
    public int Count => _assets.Count;

    /// <summary>
    /// Gets the assets contained in the store.
    /// </summary>
    public IReadOnlyCollection<IAsset> Assets => _assets;

    /// <summary>
    /// Gets the underlying asset collection.
    /// </summary>
    protected AssetCollection AssetCollection => _assets;

    private IApplicationContext? _applicationContext;

    /// <summary>
    /// Gets the application context associated with the asset store.
    /// </summary>
    public IApplicationContext ApplicationContext =>
        _applicationContext
        ?? throw new InvalidOperationException(
            "The asset store has not been initialized.");

    /// <summary>
    /// Gets a value indicating whether the asset store has been initialized.
    /// </summary>
    public bool IsInitialized => _applicationContext is not null;

    /// <summary>
    /// Initializes the asset store using the specified application context.
    /// </summary>
    /// <param name="context">
    /// The application context used by the asset store.
    /// </param>
    public virtual void Initialize(IApplicationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_applicationContext is not null)
        {
            throw new InvalidOperationException(
                "The asset store has already been initialized.");
        }

        _applicationContext = context;

        foreach (IAsset asset in CreateIntegratedAssets())
            Add(asset);

        OnInitialized(context);
    }

    // ---------------------------------------------------------------------
    // Add
    // ---------------------------------------------------------------------

    /// <summary>
    /// Adds an asset to the store.
    /// </summary>
    /// <param name="asset">
    /// The asset to add.
    /// </param>
    public virtual void Add(IAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        _assets.Add(asset);

        OnAssetAdded(asset);
    }

    /// <summary>
    /// Adds an asset to the store and assigns its loader source.
    /// </summary>
    /// <param name="asset">
    /// The asset to add.
    /// </param>
    /// <param name="loaderSource">
    /// The resource source used to load the asset.
    /// </param>
    public virtual void Add(
        IAsset asset,
        Sasogine.Resources.ResourceSourceBase loaderSource)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(loaderSource);

        asset.LoaderSource = loaderSource;

        Add(asset);
    }

    /// <summary>
    /// Adds an asset to the store and loads it immediately.
    /// </summary>
    /// <param name="asset">
    /// The asset to add and load.
    /// </param>
    public virtual void AddAndLoad(IAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        Add(asset);
        LoadAsset(asset);
    }

    /// <summary>
    /// Adds an asset to the store, assigns its loader source,
    /// and loads it immediately.
    /// </summary>
    /// <param name="asset">
    /// The asset to add and load.
    /// </param>
    /// <param name="loaderSource">
    /// The resource source used to load the asset.
    /// </param>
    public virtual void AddAndLoad(
        IAsset asset,
        Sasogine.Resources.ResourceSourceBase loaderSource)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(loaderSource);

        asset.LoaderSource = loaderSource;

        AddAndLoad(asset);
    }

    // ---------------------------------------------------------------------
    // Remove
    // ---------------------------------------------------------------------

    /// <summary>
    /// Removes the specified asset from the store.
    /// </summary>
    /// <param name="asset">
    /// The asset to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool Remove(IAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        if (!_assets.Remove(asset))
            return false;

        OnAssetRemoved(asset);

        return true;
    }

    /// <summary>
    /// Removes the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public virtual bool Remove(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        IAsset? asset = _assets.Find(id);

        return asset is not null && Remove(asset);
    }

    /// <summary>
    /// Removes all assets from the store.
    /// </summary>
    public virtual void Clear()
    {
        IAsset[] assets = [.. _assets];

        _assets.Clear();

        foreach (IAsset asset in assets)
            OnAssetRemoved(asset);
    }

    // ---------------------------------------------------------------------
    // Contains
    // ---------------------------------------------------------------------

    /// <summary>
    /// Determines whether an asset with the specified identifier exists.
    /// </summary>
    public virtual bool Contains(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Find(id) is not null;
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier
    /// is assignable to the specified type.
    /// </summary>
    public virtual bool Contains(
        Type assetType,
        string id)
    {
        ValidateAssetType(assetType);
        ArgumentException.ThrowIfNullOrEmpty(id);

        IAsset? asset = _assets.Find(id);

        return asset is not null &&
               assetType.IsInstanceOfType(asset);
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier
    /// is assignable to <typeparamref name="TAsset"/>.
    /// </summary>
    public virtual bool Contains<TAsset>(string id)
        where TAsset : class, IAsset
    {
        return Contains(typeof(TAsset), id);
    }

    // ---------------------------------------------------------------------
    // Get
    // ---------------------------------------------------------------------

    /// <summary>
    /// Gets the asset with the specified identifier.
    /// </summary>
    public virtual IAsset Get(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Find(id)
            ?? throw new KeyNotFoundException(
                $"Asset '{id}' was not found.");
    }

    /// <summary>
    /// Gets the asset with the specified identifier
    /// and verifies its type.
    /// </summary>
    public virtual IAsset Get(
        Type assetType,
        string id)
    {
        ValidateAssetType(assetType);

        IAsset asset = Get(id);

        if (!assetType.IsInstanceOfType(asset))
        {
            throw new InvalidCastException(
                $"Asset '{id}' is of type " +
                $"'{asset.GetType().FullName}' and cannot be assigned to " +
                $"'{assetType.FullName}'.");
        }

        return asset;
    }

    /// <summary>
    /// Gets the asset with the specified identifier.
    /// </summary>
    public virtual TAsset Get<TAsset>(string id)
        where TAsset : class, IAsset
    {
        return (TAsset)Get(typeof(TAsset), id);
    }

    // ---------------------------------------------------------------------
    // TryGet
    // ---------------------------------------------------------------------

    /// <summary>
    /// Attempts to get the asset with the specified identifier.
    /// </summary>
    public virtual bool TryGet(
        string id,
        out IAsset? asset)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.TryGet(id, out asset);
    }

    /// <summary>
    /// Attempts to get the asset with the specified identifier
    /// and type.
    /// </summary>
    public virtual bool TryGet(
        Type assetType,
        string id,
        out IAsset? asset)
    {
        ValidateAssetType(assetType);
        ArgumentException.ThrowIfNullOrEmpty(id);

        if (_assets.TryGet(id, out IAsset? value) &&
            value is not null &&
            assetType.IsInstanceOfType(value))
        {
            asset = value;
            return true;
        }

        asset = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the asset with the specified identifier.
    /// </summary>
    public virtual bool TryGet<TAsset>(
        string id,
        out TAsset? asset)
        where TAsset : class, IAsset
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.TryGet(id, out asset);
    }

    // ---------------------------------------------------------------------
    // GetAll
    // ---------------------------------------------------------------------

    /// <summary>
    /// Gets all assets contained in the store.
    /// </summary>
    public virtual IEnumerable<IAsset> GetAll()
    {
        return _assets.ToArray();
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    public virtual IEnumerable<IAsset> GetAll(Type assetType)
    {
        ValidateAssetType(assetType);

        return _assets
            .Where(assetType.IsInstanceOfType)
            .ToArray();
    }

    /// <summary>
    /// Gets all assets assignable to
    /// <typeparamref name="TAsset"/>.
    /// </summary>
    public virtual IEnumerable<TAsset> GetAll<TAsset>()
        where TAsset : class, IAsset
    {
        return _assets
            .OfType<TAsset>()
            .ToArray();
    }

    // ---------------------------------------------------------------------
    // Load
    // ---------------------------------------------------------------------

    /// <summary>
    /// Loads the asset with the specified identifier.
    /// </summary>
    public virtual void LoadAsset(string id)
    {
        LoadAsset(Get(id));
    }

    /// <summary>
    /// Loads the specified asset.
    /// </summary>
    public virtual void LoadAsset(IAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        OnLoadingAsset(asset);

        asset.Load();

        OnAssetLoaded(asset);
    }

    /// <summary>
    /// Loads all assets contained in the store.
    /// </summary>
    public virtual void LoadAll()
    {
        foreach (IAsset asset in GetAssetSnapshot())
            LoadAsset(asset);
    }

    /// <summary>
    /// Loads all assets assignable to the specified type.
    /// </summary>
    public virtual void LoadAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in GetAssetSnapshot())
        {
            if (assetType.IsInstanceOfType(asset))
                LoadAsset(asset);
        }
    }

    /// <summary>
    /// Loads all assets assignable to
    /// <typeparamref name="TAsset"/>.
    /// </summary>
    public virtual void LoadAll<TAsset>()
        where TAsset : class, IAsset
    {
        LoadAll(typeof(TAsset));
    }

    // ---------------------------------------------------------------------
    // Unload
    // ---------------------------------------------------------------------

    /// <summary>
    /// Unloads the asset with the specified identifier.
    /// </summary>
    public virtual void UnloadAsset(string id)
    {
        UnloadAsset(Get(id));
    }

    /// <summary>
    /// Unloads the specified asset.
    /// </summary>
    public virtual void UnloadAsset(IAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        OnUnloadingAsset(asset);

        asset.Unload();

        OnAssetUnloaded(asset);
    }

    /// <summary>
    /// Unloads all assets contained in the store.
    /// </summary>
    public virtual void UnloadAll()
    {
        foreach (IAsset asset in GetAssetSnapshot())
            UnloadAsset(asset);
    }

    /// <summary>
    /// Unloads all assets assignable to the specified type.
    /// </summary>
    public virtual void UnloadAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in GetAssetSnapshot())
        {
            if (assetType.IsInstanceOfType(asset))
                UnloadAsset(asset);
        }
    }

    /// <summary>
    /// Unloads all assets assignable to
    /// <typeparamref name="TAsset"/>.
    /// </summary>
    public virtual void UnloadAll<TAsset>()
        where TAsset : class, IAsset
    {
        UnloadAll(typeof(TAsset));
    }

    // ---------------------------------------------------------------------
    // Protected
    // ---------------------------------------------------------------------

    /// <summary>
    /// Called after the asset store has been initialized.
    /// </summary>
    protected virtual void OnInitialized(IApplicationContext context)
    {
    }

    /// <summary>
    /// Creates the assets that are integrated into this asset store.
    /// </summary>
    /// <returns>
    /// The integrated assets.
    /// </returns>
    protected virtual IEnumerable<IAsset> CreateIntegratedAssets()
    {
        yield break;
    }

    /// <summary>
    /// Gets a snapshot of the currently stored assets.
    /// </summary>
    protected virtual IAsset[] GetAssetSnapshot()
    {
        return [.. _assets];
    }

    /// <summary>
    /// Validates that the specified type represents an asset type.
    /// </summary>
    protected virtual void ValidateAssetType(Type assetType)
    {
        ArgumentNullException.ThrowIfNull(assetType);

        if (!typeof(IAsset).IsAssignableFrom(assetType))
        {
            throw new ArgumentException(
                $"Type '{assetType.FullName}' does not implement " +
                $"'{typeof(IAsset).FullName}'.",
                nameof(assetType));
        }
    }

    /// <summary>
    /// Called after an asset has been added.
    /// </summary>
    protected virtual void OnAssetAdded(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been removed.
    /// </summary>
    protected virtual void OnAssetRemoved(IAsset asset)
    {
    }

    /// <summary>
    /// Called immediately before an asset is loaded.
    /// </summary>
    protected virtual void OnLoadingAsset(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been loaded.
    /// </summary>
    protected virtual void OnAssetLoaded(IAsset asset)
    {
    }

    /// <summary>
    /// Called immediately before an asset is unloaded.
    /// </summary>
    protected virtual void OnUnloadingAsset(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been unloaded.
    /// </summary>
    protected virtual void OnAssetUnloaded(IAsset asset)
    {
    }

    // ---------------------------------------------------------------------
    // Enumeration
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the assets.
    /// </summary>
    public IEnumerator<IAsset> GetEnumerator()
    {
        return _assets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
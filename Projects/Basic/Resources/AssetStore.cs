using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Provides storage and lifecycle management for game assets.
/// </summary>
public class AssetStore : IReadOnlyAssetStore
{
    private readonly GameApplicationBase _gameApplication;
    private readonly AssetCollection _assets;
    private IApplicationContext? _applicationContext;

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

        AssetContext assetContext = new(
            _assets,
            GraphicsDevice);

        _assets.Initialize(assetContext);

        OnInitialized(context);
    }

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
        ResourceSourceBase loaderSource)
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
        ResourceSourceBase loaderSource)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(loaderSource);

        asset.LoaderSource = loaderSource;
        AddAndLoad(asset);
    }

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

    /// <summary>
    /// Determines whether an asset with the specified identifier exists.
    /// </summary>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool Contains(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Find(id) is not null;
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier is assignable
    /// to the specified type.
    /// </summary>
    /// <param name="assetType">
    /// The asset type.
    /// </param>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool Contains(Type assetType, string id)
    {
        ValidateAssetType(assetType);
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Contains(assetType, id);
    }

    /// <summary>
    /// Determines whether an asset with the specified identifier is assignable
    /// to <typeparamref name="TAsset"/>.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type.
    /// </typeparam>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool Contains<TAsset>(string id)
        where TAsset : class, IAsset
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Contains<TAsset>(id);
    }

    /// <summary>
    /// Gets the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// The matching asset.
    /// </returns>
    public virtual IAsset Get(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.Find(id)
            ?? throw new KeyNotFoundException(
                $"Asset '{id}' was not found.");
    }

    /// <summary>
    /// Gets the asset with the specified identifier and verifies its type.
    /// </summary>
    /// <param name="assetType">
    /// The required asset type.
    /// </param>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// The matching asset.
    /// </returns>
    public virtual IAsset Get(Type assetType, string id)
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
    /// <typeparam name="TAsset">
    /// The required asset type.
    /// </typeparam>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <returns>
    /// The matching asset.
    /// </returns>
    public virtual TAsset Get<TAsset>(string id)
        where TAsset : class, IAsset
    {
        return (TAsset)Get(typeof(TAsset), id);
    }

    /// <summary>
    /// Attempts to get the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <param name="asset">
    /// The matching asset if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool TryGet(
        string id,
        out IAsset? asset)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.TryGet(id, out asset);
    }

    /// <summary>
    /// Attempts to get an asset with the specified identifier and type.
    /// </summary>
    /// <param name="assetType">
    /// The required asset type.
    /// </param>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <param name="asset">
    /// The matching asset if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool TryGet(
        Type assetType,
        string id,
        out IAsset? asset)
    {
        ValidateAssetType(assetType);
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.TryGet(assetType, id, out asset);
    }

    /// <summary>
    /// Attempts to get an asset with the specified identifier.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The required asset type.
    /// </typeparam>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    /// <param name="asset">
    /// The matching asset if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public virtual bool TryGet<TAsset>(
        string id,
        out TAsset? asset)
        where TAsset : class, IAsset
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        return _assets.TryGet(id, out asset);
    }

    /// <summary>
    /// Gets all assets contained in the store.
    /// </summary>
    /// <returns>
    /// All assets contained in the store.
    /// </returns>
    public virtual IEnumerable<IAsset> GetAll()
    {
        return _assets.ToArray();
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    /// <param name="assetType">
    /// The asset type.
    /// </param>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public virtual IEnumerable<IAsset> GetAll(Type assetType)
    {
        ValidateAssetType(assetType);

        return _assets.GetAll(assetType).ToArray();
    }

    /// <summary>
    /// Gets all assets assignable to <typeparamref name="TAsset"/>.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type.
    /// </typeparam>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public virtual IEnumerable<TAsset> GetAll<TAsset>()
        where TAsset : class, IAsset
    {
        return _assets.GetAll<TAsset>().ToArray();
    }

    /// <summary>
    /// Loads the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    public virtual void LoadAsset(string id)
    {
        LoadAsset(Get(id));
    }

    /// <summary>
    /// Loads the specified asset.
    /// </summary>
    /// <param name="asset">
    /// The asset to load.
    /// </param>
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
        foreach (IAsset asset in this)
            LoadAsset(asset);
    }

    /// <summary>
    /// Loads all assets assignable to the specified type.
    /// </summary>
    /// <param name="assetType">
    /// The asset type.
    /// </param>
    public virtual void LoadAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in this)
        {
            if (assetType.IsInstanceOfType(asset))
                LoadAsset(asset);
        }
    }

    /// <summary>
    /// Loads all assets assignable to <typeparamref name="TAsset"/>.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type.
    /// </typeparam>
    public virtual void LoadAll<TAsset>()
        where TAsset : class, IAsset
    {
        LoadAll(typeof(TAsset));
    }

    /// <summary>
    /// Unloads the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The asset identifier.
    /// </param>
    public virtual void UnloadAsset(string id)
    {
        UnloadAsset(Get(id));
    }

    /// <summary>
    /// Unloads the specified asset.
    /// </summary>
    /// <param name="asset">
    /// The asset to unload.
    /// </param>
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
        foreach (IAsset asset in this)
            UnloadAsset(asset);
    }

    /// <summary>
    /// Unloads all assets assignable to the specified type.
    /// </summary>
    /// <param name="assetType">
    /// The asset type.
    /// </param>
    public virtual void UnloadAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in this)
        {
            if (assetType.IsInstanceOfType(asset))
                UnloadAsset(asset);
        }
    }

    /// <summary>
    /// Unloads all assets assignable to <typeparamref name="TAsset"/>.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type.
    /// </typeparam>
    public virtual void UnloadAll<TAsset>()
        where TAsset : class, IAsset
    {
        UnloadAll(typeof(TAsset));
    }

    /// <summary>
    /// Called after the asset store has been initialized.
    /// </summary>
    /// <param name="context">
    /// The application context used to initialize the store.
    /// </param>
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
    /// Validates that the specified type represents an asset type.
    /// </summary>
    /// <param name="assetType">
    /// The type to validate.
    /// </param>
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
    /// <param name="asset">
    /// The added asset.
    /// </param>
    protected virtual void OnAssetAdded(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been removed.
    /// </summary>
    /// <param name="asset">
    /// The removed asset.
    /// </param>
    protected virtual void OnAssetRemoved(IAsset asset)
    {
    }

    /// <summary>
    /// Called immediately before an asset is loaded.
    /// </summary>
    /// <param name="asset">
    /// The asset being loaded.
    /// </param>
    protected virtual void OnLoadingAsset(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been loaded.
    /// </summary>
    /// <param name="asset">
    /// The loaded asset.
    /// </param>
    protected virtual void OnAssetLoaded(IAsset asset)
    {
    }

    /// <summary>
    /// Called immediately before an asset is unloaded.
    /// </summary>
    /// <param name="asset">
    /// The asset being unloaded.
    /// </param>
    protected virtual void OnUnloadingAsset(IAsset asset)
    {
    }

    /// <summary>
    /// Called after an asset has been unloaded.
    /// </summary>
    /// <param name="asset">
    /// The unloaded asset.
    /// </param>
    protected virtual void OnAssetUnloaded(IAsset asset)
    {
    }

    /// <summary>
    /// Returns an enumerator that iterates through the assets.
    /// </summary>
    /// <returns>
    /// An enumerator for the assets.
    /// </returns>
    public IEnumerator<IAsset> GetEnumerator()
    {
        return _assets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
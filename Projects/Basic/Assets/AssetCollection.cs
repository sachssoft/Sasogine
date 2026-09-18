using Sachssoft.Sasogine.Common.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Represents an ordered and trackable collection of assets with
/// reference resolution and automatic asset lifecycle management.
/// </summary>
/// <remarks>
/// Asset identifiers are tracked by the underlying
/// <see cref="ReferencableCollection{T}"/> and must be unique within
/// the collection.
///
/// When an <see cref="AssetContext"/> is associated with the collection,
/// contained assets are initialized automatically. Assets added afterwards
/// are initialized immediately. Assets are deinitialized when they are
/// removed, replaced, or when the collection is cleared.
/// </remarks>
public class AssetCollection : ReferencableCollection<IAsset>
{
    private AssetContext? _context;

    /// <summary>
    /// Initializes a new, empty instance of the
    /// <see cref="AssetCollection"/> class.
    /// </summary>
    public AssetCollection()
    {
    }

    /// <summary>
    /// Initializes a new, empty instance of the
    /// <see cref="AssetCollection"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">
    /// The initial number of assets that the collection can contain
    /// without resizing.
    /// </param>
    public AssetCollection(int capacity)
        : base(capacity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetCollection"/>
    /// class containing the specified assets.
    /// </summary>
    /// <param name="assets">
    /// The assets to add to the collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assets"/> is <see langword="null"/>.
    /// </exception>
    public AssetCollection(IEnumerable<IAsset> assets)
        : base(assets)
    {
    }

    /// <summary>
    /// Gets the asset context associated with this collection.
    /// </summary>
    /// <value>
    /// The associated asset context, or <see langword="null"/> if no
    /// context has been assigned.
    /// </value>
    public AssetContext? Context => _context;

    /// <summary>
    /// Determines whether an asset of the specified type with the
    /// specified identifier exists in the collection.
    /// </summary>
    /// <param name="assetType">
    /// The asset type to search for.
    /// </param>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(
        Type assetType,
        string? id)
    {
        return Find(assetType, id) is not null;
    }

    /// <summary>
    /// Determines whether an asset of the specified type with the
    /// specified identifier exists in the collection.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type to search for.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains<TAsset>(string? id)
        where TAsset : class, IAsset
    {
        return Find<TAsset>(id) is not null;
    }

    /// <summary>
    /// Finds an asset of the specified type with the specified identifier.
    /// </summary>
    /// <param name="assetType">
    /// The asset type to search for.
    /// </param>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no matching
    /// asset exists.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assetType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="assetType"/> does not implement
    /// <see cref="IAsset"/>.
    /// </exception>
    public IAsset? Find(
        Type assetType,
        string? id)
    {
        ValidateAssetType(assetType);

        IAsset? asset = Find(id);

        if (asset is null)
            return null;

        return assetType.IsInstanceOfType(asset)
            ? asset
            : null;
    }

    /// <summary>
    /// Finds an asset of the specified type with the specified identifier.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type to search for.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no matching
    /// asset exists.
    /// </returns>
    public TAsset? Find<TAsset>(string? id)
        where TAsset : class, IAsset
    {
        return Find(id) as TAsset;
    }

    /// <summary>
    /// Finds all assets of the specified type with the specified class.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type to search for.
    /// </typeparam>
    /// <param name="class">
    /// The asset class to search for.
    /// </param>
    /// <returns>
    /// An enumerable containing all matching assets.
    /// </returns>
    public IEnumerable<TAsset> FindAll<TAsset>(string? @class)
        where TAsset : class, IAsset
    {
        foreach (IAsset asset in this)
        {
            if (asset is TAsset typedAsset &&
                string.Equals(
                    asset.Class,
                    @class,
                    StringComparison.Ordinal))
            {
                yield return typedAsset;
            }
        }
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    /// <param name="assetType">
    /// The asset type to search for.
    /// </param>
    /// <returns>
    /// An enumerable containing all assets assignable to the specified type.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assetType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="assetType"/> does not implement
    /// <see cref="IAsset"/>.
    /// </exception>
    public IEnumerable<IAsset> GetAll(Type assetType)
    {
        ValidateAssetType(assetType);

        foreach (IAsset asset in this)
        {
            if (assetType.IsInstanceOfType(asset))
                yield return asset;
        }
    }

    /// <summary>
    /// Gets all assets assignable to the specified type.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type to search for.
    /// </typeparam>
    /// <returns>
    /// An enumerable containing all assets assignable to the specified type.
    /// </returns>
    public IEnumerable<TAsset> GetAll<TAsset>()
        where TAsset : class, IAsset
    {
        foreach (IAsset asset in this)
        {
            if (asset is TAsset typedAsset)
                yield return typedAsset;
        }
    }

    /// <summary>
    /// Attempts to find an asset of the specified type with the
    /// specified identifier.
    /// </summary>
    /// <param name="assetType">
    /// The asset type to search for.
    /// </param>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(
        Type assetType,
        string? id,
        [NotNullWhen(true)] out IAsset? result)
    {
        result = Find(assetType, id);
        return result is not null;
    }

    /// <summary>
    /// Attempts to find an asset of the specified type with the
    /// specified identifier.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The asset type to search for.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet<TAsset>(
        string? id,
        [NotNullWhen(true)] out TAsset? result)
        where TAsset : class, IAsset
    {
        result = Find<TAsset>(id);
        return result is not null;
    }

    /// <summary>
    /// Removes the asset with the specified identifier from the collection.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found and removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(string? id)
    {
        IAsset? asset = Find(id);

        if (asset is null)
            return false;

        return Remove(asset);
    }

    /// <summary>
    /// Associates the specified asset context with this collection and
    /// initializes all assets currently contained in the collection.
    /// </summary>
    /// <param name="context">
    /// The asset context to associate with this collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A context is already associated with this collection or the
    /// specified context references another asset collection.
    /// </exception>
    internal void SetContext(AssetContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_context is not null)
        {
            throw new InvalidOperationException(
                "The asset collection already has an asset context.");
        }

        if (!ReferenceEquals(context.Source, this))
        {
            throw new InvalidOperationException(
                "The asset context source does not reference this collection.");
        }

        _context = context;

        foreach (IAsset asset in this)
            asset.Initialize(context);
    }

    /// <summary>
    /// Validates and initializes an asset before it is inserted into
    /// the collection.
    /// </summary>
    /// <param name="index">
    /// The index at which the asset will be inserted.
    /// </param>
    /// <param name="item">
    /// The asset to insert.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the insertion may continue;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnInserting(
        int index,
        IAsset item)
    {
        if (!base.OnInserting(index, item))
            return false;

        if (_context is not null)
            item.Initialize(_context);

        return true;
    }

    /// <summary>
    /// Validates a replacement and updates the initialization state
    /// of the affected assets.
    /// </summary>
    /// <param name="index">
    /// The index of the asset being replaced.
    /// </param>
    /// <param name="oldItem">
    /// The existing asset.
    /// </param>
    /// <param name="newItem">
    /// The replacement asset.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the replacement may continue;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnSetting(
        int index,
        IAsset oldItem,
        IAsset newItem)
    {
        if (!base.OnSetting(index, oldItem, newItem))
            return false;

        if (oldItem.IsInitialized)
            oldItem.Deinitialize();

        if (_context is not null)
            newItem.Initialize(_context);

        return true;
    }

    /// <summary>
    /// Deinitializes an asset after it has been removed from the collection.
    /// </summary>
    /// <param name="index">
    /// The previous index of the removed asset.
    /// </param>
    /// <param name="item">
    /// The removed asset.
    /// </param>
    protected override void OnRemoved(
        int index,
        IAsset item)
    {
        if (item.IsInitialized)
            item.Deinitialize();

        base.OnRemoved(index, item);
    }

    /// <summary>
    /// Prepares the collection for clearing by deinitializing all
    /// contained assets.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the collection may be cleared;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected override bool OnClearing()
    {
        if (!base.OnClearing())
            return false;

        foreach (IAsset asset in this)
        {
            if (asset.IsInitialized)
                asset.Deinitialize();
        }

        return true;
    }

    /// <summary>
    /// Validates that the specified type represents an asset type.
    /// </summary>
    /// <param name="assetType">
    /// The type to validate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assetType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="assetType"/> does not implement
    /// <see cref="IAsset"/>.
    /// </exception>
    private static void ValidateAssetType(Type assetType)
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
}
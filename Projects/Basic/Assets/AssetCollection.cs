using Sachssoft.Engine.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Represents an ordered and trackable collection of assets with
/// reference resolution and context-based lifecycle management.
/// </summary>
/// <remarks>
/// Asset identifiers are tracked by the underlying
/// <see cref="ReferencableCollection{T}"/> and must be unique within
/// the collection.
///
/// Asset lifecycle management is provided by
/// <see cref="ContextualReferencableCollection{T, TContext}"/>.
/// When the collection is initialized with an <see cref="AssetContext"/>,
/// existing and subsequently added assets are initialized automatically.
/// Removed, replaced, or cleared assets are deinitialized automatically.
/// </remarks>
public class AssetCollection :
    ContextualReferencableCollection<IAsset, AssetContext>
{
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
    public bool Contains(Type assetType, string? id)
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
    public IAsset? Find(Type assetType, string? id)
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
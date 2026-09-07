using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Represents an immutable collection of assets that supports asset resolution
/// by identifier or class.
/// </summary>
/// <remarks>
/// The collection creates an immutable snapshot of the supplied assets when it
/// is constructed. Subsequent changes to the source collection are not reflected
/// by this collection.
/// </remarks>
public sealed class ReadOnlyAssetCollection :
    IReadOnlyList<IAsset>,
    IEngineObjectResolver
{
    private readonly IAsset[] _items;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ReadOnlyAssetCollection"/> class.
    /// </summary>
    /// <param name="items">
    /// The assets used to create the immutable collection.
    /// </param>
    public ReadOnlyAssetCollection(
        IEnumerable<IAsset> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items = items is IAsset[] array
            ? (IAsset[])array.Clone()
            : [.. items];
    }

    /// <summary>
    /// Gets the number of assets contained in the collection.
    /// </summary>
    public int Count => _items.Length;

    /// <summary>
    /// Gets the asset at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the asset to get.
    /// </param>
    public IAsset this[int index] => _items[index];

    /// <summary>
    /// Determines whether the collection contains the specified asset instance.
    /// </summary>
    /// <param name="item">
    /// The asset to locate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset is contained in the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(IAsset item)
    {
        return IndexOf(item) >= 0;
    }

    /// <summary>
    /// Returns the index of the specified asset instance.
    /// </summary>
    /// <param name="item">
    /// The asset to locate.
    /// </param>
    /// <returns>
    /// The zero-based index of the asset if found; otherwise, <c>-1</c>.
    /// </returns>
    public int IndexOf(IAsset item)
    {
        for (int i = 0; i < _items.Length; i++)
        {
            if (ReferenceEquals(_items[i], item))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Finds an asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no asset was found.
    /// </returns>
    public IAsset? Find(string? id)
    {
        if (id == null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (asset.Id == id)
                return asset;
        }

        return null;
    }

    /// <summary>
    /// Finds an asset of the specified type with the specified identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <returns>
    /// The matching asset, or <see langword="null"/> if no asset was found.
    /// </returns>
    public T? Find<T>(string? id)
        where T : class, IAsset
    {
        if (id == null)
            return null;

        foreach (IAsset asset in _items)
        {
            if (asset.Id == id && asset is T typedAsset)
                return typedAsset;
        }

        return null;
    }

    /// <summary>
    /// Finds all assets with the specified class.
    /// </summary>
    /// <param name="class">
    /// The class of the assets to find.
    /// </param>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public IEnumerable<IAsset> FindAll(string? @class)
    {
        foreach (IAsset asset in _items)
        {
            if (asset.Class == @class)
                yield return asset;
        }
    }

    /// <summary>
    /// Finds all assets of the specified type with the specified class.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="class">
    /// The class of the assets to find.
    /// </param>
    /// <returns>
    /// All matching assets.
    /// </returns>
    public IEnumerable<T> FindAll<T>(string? @class)
        where T : class, IAsset
    {
        foreach (IAsset asset in _items)
        {
            if (asset.Class == @class && asset is T typedAsset)
                yield return typedAsset;
        }
    }

    /// <summary>
    /// Attempts to find an asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGet(string? id, out IAsset? result)
    {
        result = Find(id);
        return result != null;
    }

    /// <summary>
    /// Attempts to find an asset of the specified type with the specified
    /// identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The asset type to find.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset to find.
    /// </param>
    /// <param name="result">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGet<T>(string? id, out T? result)
        where T : class, IAsset
    {
        result = Find<T>(id);
        return result != null;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the assets.
    /// </summary>
    public IEnumerator<IAsset> GetEnumerator()
    {
        return ((IEnumerable<IAsset>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEngineReferenceable? IEngineObjectResolver.Find(string? id)
    {
        return Find(id);
    }

    IEnumerable<IEngineReferenceable> IEngineObjectResolver.FindAll(
        string? @class)
    {
        return FindAll(@class);
    }

    bool IEngineObjectResolver.TryGet(
        string? id,
        out IEngineReferenceable? result)
    {
        IAsset? asset = Find(id);
        result = asset;

        return asset != null;
    }
}
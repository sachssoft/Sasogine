using System;
using System.Collections.Generic;
using System.Linq;
using Sachssoft.Observables;
using Sachssoft.Sasogine.Containers;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Assets;

public static class AssetsExtension
{
    /// <summary>
    /// Generische Hauptvariante für alle Collection-Typen.
    /// Null-safe, optionaler Prepare-Callback.
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        IEnumerable<IAsset>? enumerable,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        asset?.Unload();
        var collection = enumerable ?? Enumerable.Empty<IAsset>();
        asset = association.Find(collection);

        if (asset != null && !asset.IsLoaded)
        {
            prepare?.Invoke(asset);
            asset.Load();
        }

        return asset?.IsLoaded == true ? asset.Instance : null;
    }

    /// <summary>
    /// ProjectedPackageAssetCollection
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        ProjectedPackageAssetCollection? packageAssetCollection,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        var collection = packageAssetCollection?.GetAll()
                                               .Select(x => x.Asset);
        return GetAssetInstance<TAsset, T>(association, ref asset, collection, prepare);
    }

    /// <summary>
    /// IDictionary<string, IAsset>
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        IDictionary<string, IAsset>? assetDictionary,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        var collection = assetDictionary?.Values;
        return GetAssetInstance<TAsset, T>(association, ref asset, collection, prepare);
    }

    /// <summary>
    /// IReadOnlyDictionary<string, IAsset>
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        IReadOnlyDictionary<string, IAsset>? assetDictionary,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        var collection = assetDictionary?.Values;
        return GetAssetInstance<TAsset, T>(association, ref asset, collection, prepare);
    }

    /// <summary>
    /// IDictionary<string, IAssetSource>
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        IDictionary<string, IAssetSource>? assetSourceDictionary,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        var collection = assetSourceDictionary?.Values.Select(x => x.Asset);
        return GetAssetInstance<TAsset, T>(association, ref asset, collection, prepare);
    }

    /// <summary>
    /// IReadOnlyDictionary<string, IAssetSource>
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Association<TAsset> association,
        ref TAsset? asset,
        IReadOnlyDictionary<string, IAssetSource>? assetSourceDictionary,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        var collection = assetSourceDictionary?.Values.Select(x => x.Asset);
        return GetAssetInstance<TAsset, T>(association, ref asset, collection, prepare);
    }
}

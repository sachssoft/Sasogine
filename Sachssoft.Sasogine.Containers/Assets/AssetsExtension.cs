using System;
using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Containers;

namespace Sachssoft.Sasogine.Assets;

public static class AssetsExtension
{
    /// <summary>
    /// Generische Hauptvariante für alle Collection-Typen.
    /// Null-safe, optionaler Prepare-Callback.
    /// </summary>
    public static T? GetAssetInstance<TAsset, T>(
        this Reference<IAssetSource, IAssetSourceCollection> reference,
        ref TAsset? asset,
        IAssetSourceCollection? itemSource,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        asset?.Unload();
        reference.ItemsSource = itemSource;

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
        this Reference<IAssetSource, IAssetSourceCollection> reference,
        ref TAsset? asset,
        ProjectedPackageAssetCollection? packageAssetCollection,
        Action<TAsset>? prepare = null)
        where TAsset : AssetBase<T>
        where T : class
    {
        return GetAssetInstance<TAsset, T>(reference, ref asset, packageAssetCollection, prepare);
    }

    ///// <summary>
    ///// IDictionary<string, IAsset>
    ///// </summary>
    //public static T? GetAssetInstance<TAsset, T>(
    //    this Reference<IAssetSource, IAssetSourceCollection> reference,
    //    ref TAsset? asset,
    //    IDictionary<string, IAsset>? assetDictionary,
    //    Action<TAsset>? prepare = null)
    //    where TAsset : AssetBase<T>
    //    where T : class
    //{
    //    return GetAssetInstance<TAsset, T>(reference, ref asset, assetDictionary, prepare);
    //}

    ///// <summary>
    ///// IReadOnlyDictionary<string, IAsset>
    ///// </summary>
    //public static T? GetAssetInstance<TAsset, T>(
    //    this Reference<IAssetSource, IAssetSourceCollection> reference,
    //    ref TAsset? asset,
    //    IReadOnlyDictionary<string, IAsset>? assetDictionary,
    //    Action<TAsset>? prepare = null)
    //    where TAsset : AssetBase<T>
    //    where T : class
    //{
    //    var collection = assetDictionary?.Values;
    //    return GetAssetInstance<TAsset, T>(reference, ref asset, assetDictionary, prepare);
    //}

    ///// <summary>
    ///// IDictionary<string, IAssetSource>
    ///// </summary>
    //public static T? GetAssetInstance<TAsset, T>(
    //    this Reference<IAssetSource, IAssetSourceCollection> reference,
    //    ref TAsset? asset,
    //    IDictionary<string, IAssetSource>? assetSourceDictionary,
    //    Action<TAsset>? prepare = null)
    //    where TAsset : AssetBase<T>
    //    where T : class
    //{
    //    var collection = assetSourceDictionary?.Values.Select(x => x.Asset);
    //    return GetAssetInstance<TAsset, T>(reference, ref asset, assetSourceDictionary, prepare);
    //}

    ///// <summary>
    ///// IReadOnlyDictionary<string, IAssetSource>
    ///// </summary>
    //public static T? GetAssetInstance<TAsset, T>(
    //    this Reference<IAssetSource, IAssetSourceCollection> reference,
    //    ref TAsset? asset,
    //    IReadOnlyDictionary<string, IAssetSource>? assetSourceDictionary,
    //    Action<TAsset>? prepare = null)
    //    where TAsset : AssetBase<T>
    //    where T : class
    //{
    //    var collection = assetSourceDictionary?.Values.Select(x => x.Asset);
    //    return GetAssetInstance<TAsset, T>(reference, ref asset, assetSourceDictionary, prepare);
    //}
}

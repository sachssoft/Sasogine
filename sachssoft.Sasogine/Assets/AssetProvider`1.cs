using System;
using System.ComponentModel;
using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine.Assets;

// Asset-Träger
public abstract class AssetProvider<T> : GameObject, IAssetProvider
{
    public AssetProvider()
    {
    }

    public AssetProvider(string? id)
    {
        ID = id;
    }

    public static void RaiseChange<TAsset, T>(
        IAssetCollectionProvider? provider,
        ref TAsset? asset,
        ref Association<TAsset>? field,
        Association<TAsset>? value,
        PropertyChangedEventHandler? changed_event = null,
        Action<TAsset>? load = null,
        Action<TAsset>? unload = null) where TAsset : AssetProvider<T>
    {
        if (asset != null)
        {
            if (changed_event != null)
            {
                asset.PropertyChanged -= changed_event;
            }

            unload?.Invoke(asset);
            asset = null;
        }

        field = value;

        if (provider != null)
        {
            asset = provider.GetAsset<TAsset>(value);

            if (asset != null)
            {
                load?.Invoke(asset);

                if (changed_event != null)
                {
                    asset.PropertyChanged += changed_event;
                }
            }
        }
    }

    public T? Asset
    {
        get;
        protected set;
    }

    public bool IsError
    {
        get;
        protected set;
    }

    object? IAssetProvider.GetAsset() => Asset;
}

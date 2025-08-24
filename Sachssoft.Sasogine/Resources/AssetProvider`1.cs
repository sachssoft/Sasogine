using Sachssoft.Observables;
using Sachssoft.Runtime;
using Sachssoft.Sasogine.Elements;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Resources;

public abstract class AssetProvider<T> : NotifyingElement, IAssetProvider
{
    public AssetProvider()
    {
    }

    public AssetProvider(string? id)
    {
        ID = id;
    }

    public static void RaiseChange<TAsset, TAssetData>(
        IAssetCollectionProvider? provider,
        ref TAsset? asset,
        ref Association<TAsset>? field,
        Association<TAsset>? value,
        PropertyChangedEventHandler? changed_event = null,
        Action<TAsset>? load = null,
        Action<TAsset>? unload = null) where TAsset : AssetProvider<TAssetData>
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
            asset = provider.GetAsset(value);

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

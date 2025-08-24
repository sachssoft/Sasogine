using System.Xml;
using Sachssoft.Observables;
using Sachssoft.Runtime;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Resources;

public static class AssetsExtension
{

    public static IAssetProvider? GetAsset(this IAssetCollectionProvider provider, IAssociation? assoc)
    {
        return assoc?.Find(provider.Assets) as IAssetProvider;
    }

    public static TAssetProvider? GetAsset<TAssetProvider>(this IAssetCollectionProvider provider, Association<TAssetProvider>? assoc) where TAssetProvider : NotifyingObject, IAssetProvider
    {
        return assoc?.Find(provider.Assets);
    }

}

using System.Xml;
using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine.Assets;

public static class AssetsExtension
{

    public static IAssetProvider? GetAsset(this IAssetCollectionProvider provider, IAssociation? assoc)
    {
        return assoc?.Find(provider.Assets) as IAssetProvider;
    }

    public static TAssetProvider? GetAsset<TAssetProvider>(this IAssetCollectionProvider provider, Association<TAssetProvider>? assoc) where TAssetProvider : GameObject, IAssetProvider
    {
        return assoc?.Find(provider.Assets);
    }

}

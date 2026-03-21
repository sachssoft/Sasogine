using Sachssoft.Sasofly.Inspection;

namespace Sachssoft.Sasogine.Containers
{
    public class AssetReference<TAsset> : Reference<TAsset, IAssetCollection>
        where TAsset : class, IIdentifiable
    {
    }
}

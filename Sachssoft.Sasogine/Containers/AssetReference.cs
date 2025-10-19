using Sachssoft.Inspection;

namespace Sachssoft.Sasogine.Containers
{
    public class AssetReference<TAsset> : Reference<TAsset, IAssetCollection>
        where TAsset : class, IIdentifiable
    {
    }
}

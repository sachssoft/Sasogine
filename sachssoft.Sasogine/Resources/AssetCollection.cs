using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Resources;

public class AssetCollection : GameObjectCollection<IAssetProvider>
{
    public AssetCollection()
    {
    }

    public AssetCollection(GameObject? owner) : base(owner)
    {
    }

    public AssetCollection(GameObject? owner, string? id_generator_prefix) : base(owner, id_generator_prefix)
    {
    }
}

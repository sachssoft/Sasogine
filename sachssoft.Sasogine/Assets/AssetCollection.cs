using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine.Assets;

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

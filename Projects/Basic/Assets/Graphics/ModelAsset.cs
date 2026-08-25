using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class ModelAsset : AssetBase<Model, ModelAssetDefinition>
    {
        public ModelAsset() : base(new ModelAssetDefinition()) { }

        public ModelAsset(ModelAssetDefinition definition) : base(definition) { }
    }
}

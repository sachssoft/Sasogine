using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public class EffectAssetDefinition : AssetDefinitionBase<EffectAsset>
    {

        public Template<IEffectAdapter>? Template { get; set; }

    }
}

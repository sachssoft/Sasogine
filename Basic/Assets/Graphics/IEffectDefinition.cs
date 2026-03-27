
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public interface IEffectDefinition : IAssetDefinition
    {

        ElementTemplate<IEffectAdapter> Template { get; set; }

    }
}

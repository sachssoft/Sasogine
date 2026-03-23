using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Components;

namespace Sachssoft.Sasogine.Rendering
{
    public interface IParallaxLayerComponent : IDrawableRuntimeComponent
    {
        void SetDrawOrder(int index);
    }
}

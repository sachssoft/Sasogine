using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public interface IFollowParallaxLayerDefinition : IParallaxLayerDefinition
    {
        Vector2 ScrollSpeed { get; set; }

        Vector2 Spacing { get; set; }

        Vector2 Factor { get; set; }
    }
}
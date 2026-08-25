using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Parallaxes
{
    public class ScrollParallaxLayerDefinition : ParallaxLayerDefinitionBase
    {
        public Vector2 ScrollSpeed { get; set; }

        public Vector2 Spacing { get; set; }

        public Vector2 Factor { get; set; }
    }
}
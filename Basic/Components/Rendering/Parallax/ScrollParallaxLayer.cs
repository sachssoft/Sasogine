using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class ScrollParallaxLayer : ParallaxLayerBase<ScrollParallaxLayerDefinition>
    {
        protected override ScrollParallaxLayerDefinition CreateDefinition()
        {
            return new ScrollParallaxLayerDefinition();
        }

        public override void Draw(RuntimeViewportContext context)
        {
        }

        public override void Update(RuntimeContext context)
        {
        }
    }
}

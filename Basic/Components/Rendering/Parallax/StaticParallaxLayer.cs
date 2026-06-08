using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class StaticParallax : ParallaxLayerBase<StaticParallaxLayerDefinition>
    {
        protected override StaticParallaxLayerDefinition CreateDefinition()
        {
            return new StaticParallaxLayerDefinition();
        }

        public override void Draw(RuntimeViewportContext context)
        {
        }

        public override void Update(RuntimeContext context)
        {
        }
    }
}

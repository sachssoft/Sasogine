using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class ScrollParallaxLayer : ParallaxLayerBase<ScrollParallaxLayerDefinition>
    {
        protected override ScrollParallaxLayerDefinition ResolveDefinition()
        {
            return new ScrollParallaxLayerDefinition();
        }

        public override void Update(SceneUpdateContext context)
        {
        }

        public override void Draw(SceneDrawContext context)
        {
        }
    }
}

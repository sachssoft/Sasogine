using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class StaticParallax : ParallaxLayerBase<StaticParallaxLayerDefinition>
    {
        protected override StaticParallaxLayerDefinition ResolveDefinition()
        {
            return new StaticParallaxLayerDefinition();
        }

        public override void Update(SceneUpdateContext context)
        {
        }

        public override void Draw(SceneDrawContext context)
        {
        }
    }
}

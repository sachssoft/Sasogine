using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class FollowParallaxLayer : ParallaxLayerBase<FollowParallaxLayerDefinition>
    {
        protected override FollowParallaxLayerDefinition ResolveDefinition()
        {
            return new FollowParallaxLayerDefinition();
        }

        public override void Draw(RuntimeViewportContext context)
        {
        }

        public override void Update(RuntimeContext context)
        {
        }
    }
}

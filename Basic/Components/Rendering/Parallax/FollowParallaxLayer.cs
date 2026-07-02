using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class FollowParallaxLayer : ParallaxLayerBase<FollowParallaxLayerDefinition>
    {
        protected override FollowParallaxLayerDefinition ResolveDefinition()
        {
            return new FollowParallaxLayerDefinition();
        }

        public override void Update(SceneUpdateContext context)
        {
        }

        public override void Draw(SceneDrawContext context)
        {
        }
    }
}

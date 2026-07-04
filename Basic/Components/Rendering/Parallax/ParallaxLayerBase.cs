using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public abstract class ParallaxLayerBase<TDefinition> : ResourceComponentBase<TDefinition>
        where TDefinition : ParallaxLayerDefinitionBase, new()
    {
        private int _parallaxIndex;

        public abstract void Update(SceneUpdateContext context);

        public abstract void Draw(SceneDrawContext context);

        internal protected float GetDeepnessFactor(float cameraDepth)
        {
            return _parallaxIndex / cameraDepth;
        }

        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            _parallaxIndex = Definition.Index;
        }
    }
}

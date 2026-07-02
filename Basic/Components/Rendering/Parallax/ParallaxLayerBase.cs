using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public abstract class ParallaxLayerBase<TDefinition> : ResourceComponentBase<TDefinition>
        where TDefinition : ParallaxLayerDefinitionBase
    {
        private int _parallaxIndex;

        public abstract void Update(SceneUpdateContext context);

        public abstract void Draw(SceneDrawContext context);

        internal protected float GetDeepnessFactor(float cameraDepth)
        {
            return _parallaxIndex / cameraDepth;
        }

        public override void ApplyDefinition()
        {
            base.ApplyDefinition();

            _parallaxIndex = Definition.Index;
        }

        public override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);

            switch (key)
            {
                case nameof(ParallaxLayerDefinitionBase.Index):
                    _parallaxIndex = Definition.Index;
                    break;
            }
        }
    }
}

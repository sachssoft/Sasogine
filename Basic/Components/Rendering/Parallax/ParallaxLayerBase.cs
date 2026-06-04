using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public abstract class ParallaxLayerBase<TDefinition> : ComponentBase<TDefinition>
        where TDefinition : class, IParallaxLayerDefinition
    {
        private int _parallaxIndex;

        public abstract void Draw(RuntimeViewportContext context);

        public abstract void Update(RuntimeContext context);

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
                case nameof(IParallaxLayerDefinition.Index):
                    _parallaxIndex = Definition.Index;
                    break;
            }
        }
    }
}

using Sachssoft.Sasogine.Presentation;

namespace Sachssoft.Sasogine.Components.Rendering
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

        protected override void ApplyDefinition()
        {
            base.ApplyDefinition();

            _parallaxIndex = Definition.Index;
        }

        protected override void ApplyDefinitionChange(string? key)
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

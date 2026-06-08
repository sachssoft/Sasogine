using Sachssoft.Sasogine.Scenes;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class ParallaxComponent : ComponentBase<ParallaxDefinition>, IDrawableRuntimeComponent
    {
        public ParallaxComponent() { }

        public List<IParallaxLayerComponent> Layers { get; } = new List<IParallaxLayerComponent>();

        protected override ParallaxDefinition CreateDefinition()
        {
            return new ParallaxDefinition();
        }

        public void Update(RuntimeContext context)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                layer.SetDrawOrder(i);
                layer.Update(context);
            }
        }

        public void Draw(RuntimeViewportContext context)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                layer.Update(context);
            }
        }

        public override void ApplyDefinition()
        {
            base.ApplyDefinition();
        }

        public override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);
        }
    }
}

using Sachssoft.Sasogine.Scenes;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    public class ParallaxComponent : ResourceComponentBase<ParallaxDefinition>, IDrawableComponent
    {
        public ParallaxComponent() { }

        public List<IParallaxLayerComponent> Layers { get; } = new List<IParallaxLayerComponent>();

        public bool IsVisible { get; set; }

        protected override ParallaxDefinition ResolveDefinition()
        {
            return new ParallaxDefinition();
        }

        public void Update(SceneUpdateContext context)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];
                layer.SetDrawOrder(i);

                if (layer.IsEnabled)
                    layer.Update(context);
            }
        }

        public void Draw(SceneDrawContext context)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var layer = Layers[i];

                if (layer.IsVisible)
                    layer.Draw(context);
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

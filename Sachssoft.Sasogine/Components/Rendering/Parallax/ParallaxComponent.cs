using Sachssoft.Sasogine.Presentation;
using System.Collections.ObjectModel;

namespace Sachssoft.Sasogine.Components.Rendering
{
    public sealed class ParallaxComponent : ComponentBase<IParallaxDefinition>, IDrawableRuntimeComponent
    {
        public ParallaxComponent() { }

        public ObservableCollection<IParallaxLayerComponent> Layers { get; } = new ObservableCollection<IParallaxLayerComponent>();

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

        protected override void ApplyDefinition()
        {
            base.ApplyDefinition();
        }

        protected override void ApplyDefinitionChange(string? key)
        {
            base.ApplyDefinitionChange(key);
        }
    }
}

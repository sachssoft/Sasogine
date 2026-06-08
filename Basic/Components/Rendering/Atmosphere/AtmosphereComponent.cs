using Sachssoft.Sasogine.Components.Rendering.Parallax;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components.Rendering.Atmosphere
{
    public class AtmosphereComponent : ComponentBase<AtmosphereDefinition>, IDrawableRuntimeComponent
    {
        // Mehrere RenderTargets sollten möglichst vermieden werden,
        // da jeder RenderTarget-Wechsel zusätzlichen GPU-Overhead verursacht
        // (State Changes, Memory Bandwidth und Pipeline Flushes).
        //
        // Außerdem steigt der Speicherverbrauch stark an,
        // da jedes RenderTarget (z.B. 1920x1080 RGBA) mehrere MB VRAM benötigt.
        //
        // Statt viele separate RenderTargets für einzelne Effekte zu verwenden,
        // wird die Szene in EIN einheitliches RenderTarget gerendert.
        //
        // Mehrere visuelle Effekte (z.B. Fog, Underwater, HeatHaze, ColorGrading)
        // sollten anschließend in einem oder wenigen Fullscreen-Shader-Pässen
        // kombiniert werden, da Shader auf der GPU parallel pro Pixel arbeiten
        // und dadurch deutlich effizienter sind als mehrere Render-Pässe.
        //
        // Ziel: minimale RenderTarget-Anzahl, maximale Nutzung von Shadern.

        public AtmosphereComponent() { }

        protected override AtmosphereDefinition CreateDefinition()
        {
            return new AtmosphereDefinition();
        }

        public void Update(RuntimeContext context)
        {

        }

        public void Draw(RuntimeViewportContext context)
        {
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

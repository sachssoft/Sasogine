using Sachssoft.Sasogine.Presentation;

namespace Sachssoft.Sasogine.Components
{
    public interface IDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(RuntimeViewportContext context);
    }
}

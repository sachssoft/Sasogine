using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public interface IDrawableRuntimeComponent : IRuntimeComponent
    {
        void Draw(RuntimeViewportContext context);
    }
}

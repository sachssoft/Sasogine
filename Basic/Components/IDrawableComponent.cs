using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SceneDrawContext context);
    }
}

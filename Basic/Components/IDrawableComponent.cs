using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public interface IDrawableComponent : IComponent
    {
        bool IsVisible { get; set; }

        void Draw(SceneDrawContext context);
    }
}

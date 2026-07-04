using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public interface IUpdatableComponent
    {
        bool IsEnabled { get; set; }

        void Update(SceneUpdateContext context);
    }
}

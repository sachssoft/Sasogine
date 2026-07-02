using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.World
{
    public interface IUpdatableEntity
    {
        void Update(SceneUpdateContext context);
    }
}

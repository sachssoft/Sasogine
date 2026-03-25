using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Components
{
    public interface IRuntimeComponent : IComponent
    {
        void Update(RuntimeContext context);

    }
}

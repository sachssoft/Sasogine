using Sachssoft.Sasogine.Presentation;

namespace Sachssoft.Sasogine.Components
{
    public interface IRuntimeComponent : IComponent
    {
        void Update(RuntimeContext context);

    }
}

using Sachssoft.Sasogine.Components;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    public interface IAnimationDefinition : IComponentDefinition
    {

        float Speed { get; set; }

        int Duration { get; set; }

        bool Infinite { get; set; }

        int Delay { get; set; }

    }
}

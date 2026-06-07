using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    public interface IPositionAnimationDefinition : IAnimationDefinition
    {

        Vector2 Distance { get; set; }

    }
}

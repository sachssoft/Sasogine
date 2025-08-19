using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Elements;

namespace Sachssoft.Sasogine.Animation;

public interface IAnimatable
{
    GameObjectCollection<AnimationBase> Animations { get; }

    Vector2 StartPosition { get; }

    Vector2 Position { get; }

    float StartRotation { get; }

    float Rotation { get; }

    void OnAnimated(Vector2 start_position, Vector2 position, float start_rotation, float rotation);
}
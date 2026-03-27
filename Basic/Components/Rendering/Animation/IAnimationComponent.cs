using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Animation
{
    public interface IAnimationComponent
    {
        Vector2 AddPosition(float elapsedTime);

        float AddRotation(float elapsedTime);

        void Start(Vector2 position, float rotation);

        void Pause();

        void Reset();
    }
}

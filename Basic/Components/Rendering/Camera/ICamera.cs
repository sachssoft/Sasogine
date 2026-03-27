using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Components.Rendering.Camera
{
    public interface ICamera : ICameraTransform
    {
        GraphicsDevice GraphicsDevice { get; }

        Vector2 ToWorld(Vector2 screenPosition);

        Vector2 ToScreen(Vector2 worldPosition);

        void Update(GameContext context);
    }
}

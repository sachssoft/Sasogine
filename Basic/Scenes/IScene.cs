using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Scenes
{
    public interface IScene
    {
        bool IsPersistent { get; } // Keep Alive
        bool IsLoaded { get; }
        int ViewCount {  get; }

        void Load();
        void Unload();

        void Enter(SceneEnterEventArgs eventArgs);   // Scene wird aktiv
        void Exit();    // Scene wird verlassen

        void Update(SceneUpdateContext context);
        void Draw(SceneDrawContext context);

        ICamera CreateCamera(GraphicsDevice graphicsDevice, int index);
        IEffectAdapter CreateEffectAdapter(GraphicsDevice graphicsDevice);
    }
}

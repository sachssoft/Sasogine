using Microsoft.Xna.Framework;
using Sachssoft.Engine.Scenes;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Displays a simple clear color. Add gameplay components here.</summary>
public sealed class MainScene : BasicSceneBase
{
    /// <inheritdoc />
    public override void Draw(SceneDrawContext context)
    {
        context.GraphicsDevice.Clear(Color.CornflowerBlue);
    }
}

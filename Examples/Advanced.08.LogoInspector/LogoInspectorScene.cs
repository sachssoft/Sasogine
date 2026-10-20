using Microsoft.Xna.Framework;
using Sachssoft.Engine.Components.Rendering;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>A normal Sasogine scene; the controls are rendered after the scene batch has finished.</summary>
internal sealed class LogoInspectorScene : BasicSceneBase
{
    internal LogoInspectorScene() => BackgroundColor = new Color(17, 24, 35);
    /// <inheritdoc/>
    public override ICamera CreateCamera(GraphicsDevice graphicsDevice, int index) => new Camera2();
}

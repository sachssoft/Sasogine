using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Describes a direct scene's physical viewport, inherited clip and composed opacity.
/// </summary>
/// <remarks>The callback owns scene rendering; it must honor Clip when selecting custom rasterizer states.
/// Do not clear the entire color target. Bindings changed inside nested passes must be restored.</remarks>
public readonly record struct SceneViewportDrawContext(GraphicsDevice GraphicsDevice, Rectangle Bounds,
    Rectangle Clip, float Opacity, GameTime GameTime);

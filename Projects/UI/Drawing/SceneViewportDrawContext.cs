using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Describes the physical viewport and state of a scene-rendering pass.</summary>
/// <remarks>Untransformed passes use the UI target's coordinates, inherited clip and composed opacity.
/// Affine passes render into a scene-local target at opacity one; UI clipping and opacity are applied
/// during composition. The callback must honor Clip when selecting custom rasterizer states and must
/// not clear the existing color target. Bindings changed inside nested passes must be restored.</remarks>
/// <param name="GraphicsDevice">The device used by this pass.</param>
/// <param name="Bounds">The physical pixel viewport of this pass.</param>
/// <param name="Clip">The physical pixel clip within the pass target.</param>
/// <param name="Opacity">The opacity to apply while drawing; one when affine composition applies it later.</param>
/// <param name="GameTime">The current frame's timing.</param>
public readonly record struct SceneViewportDrawContext(GraphicsDevice GraphicsDevice, Rectangle Bounds,
    Rectangle Clip, float Opacity, GameTime GameTime);

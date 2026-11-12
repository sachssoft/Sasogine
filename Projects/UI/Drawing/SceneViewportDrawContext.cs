using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Scenes;
using System;
namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Describes the physical viewport and state of a scene-rendering pass.</summary>
/// <remarks>Direct passes use UI target coordinates and must preserve pixels outside the clip.
/// Offscreen passes use local target coordinates and opacity one. UI clipping and opacity apply during composition.
/// Restore target bindings after any nested render pass.</remarks>
/// <param name="GraphicsDevice">The device used by this pass.</param>
/// <param name="Bounds">The physical pixel viewport of this pass.</param>
/// <param name="Clip">The physical pixel clip within the pass target.</param>
/// <param name="Opacity">The opacity to apply while drawing; one when affine composition applies it later.</param>
/// <param name="GameTime">The current frame's timing.</param>
public readonly record struct SceneViewportDrawContext(GraphicsDevice GraphicsDevice, Rectangle Bounds,
    Rectangle Clip, float Opacity, GameTime GameTime)
{
    internal SceneDrawContext? SceneContext { get; init; }

    /// <summary>Gets the scene context supplied by the outer workspace draw call.</summary>
    /// <returns>The original scene context, including its camera and material.</returns>
    /// <exception cref="InvalidOperationException">The workspace was drawn without a scene context.</exception>
    public SceneDrawContext GetSceneContext() => SceneContext ??
        throw new InvalidOperationException("Draw the workspace with a SceneDrawContext to access the scene from this viewport.");
}

using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Input;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Provides navigation controls for a 2D camera.
/// </summary>
/// <remarks>
/// Supports camera movement and zooming around the current cursor position.
/// The camera position is adjusted during zooming so that the world position
/// beneath the cursor remains fixed.
/// </remarks>
public sealed class Camera2DNavigationTool : ToolBase
{
    private Camera2DNavigationToolInteractions? _interactions;
    private ICamera2D? _camera;
    private Vector2 _cursorPosition;
    private Point _delta;
    private bool _isInViewport;

    /// <summary>
    /// Updates the camera navigation.
    /// </summary>
    /// <param name="context">The current scene update context.</param>
    public override void Update(SceneUpdateContext context)
    {
        if (_camera == null || _interactions == null || !_isInViewport)
            return;

        if (_interactions.Move.HasFlag(InteractionFlags.IsPressed))
        {
            Vector2 delta = _delta.ToVector2();
            _camera.Position -= delta / _camera.Zoom;
        }

        if (_interactions.ZoomIn.HasFlag(InteractionFlags.IsPressed))
        {
            ZoomAtCursor(_camera, 1.1f);
        }
        else if (_interactions.ZoomOut.HasFlag(InteractionFlags.IsPressed))
        {
            ZoomAtCursor(_camera, 1f / 1.1f);
        }
    }

    /// <summary>
    /// Sets the camera controlled by this navigation tool.
    /// </summary>
    /// <param name="camera">The 2D camera to control.</param>
    public void SetCamera(ICamera2D? camera)
    {
        _camera = camera;
    }

    /// <summary>
    /// Sets the interactions used to control the camera.
    /// </summary>
    /// <param name="interactions">The camera navigation interactions.</param>
    public void SetInteractions(Camera2DNavigationToolInteractions interactions)
    {
        _interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
    }

    /// <summary>
    /// Updates the current cursor state.
    /// </summary>
    /// <param name="screenPosition">The current cursor position in screen coordinates.</param>
    /// <param name="delta">The cursor movement since the previous update.</param>
    /// <param name="isInViewport">Indicates whether the cursor is inside the viewport.</param>
    public void SetCursor(Vector2 screenPosition, Point delta, bool isInViewport = true)
    {
        _cursorPosition = screenPosition;
        _delta = delta;
        _isInViewport = isInViewport;
    }

    private void ZoomAtCursor(ICamera2D camera, float factor)
    {
        Vector2 before = GetWorldPosition(camera);
        camera.Zoom *= factor;
        Vector2 after = GetWorldPosition(camera);
        camera.Position += before - after;
    }

    private Vector2 GetWorldPosition(ICamera2D camera)
    {
        Matrix inverseView = Matrix.Invert(camera.View);
        return Vector2.Transform(_cursorPosition, inverseView);
    }
}
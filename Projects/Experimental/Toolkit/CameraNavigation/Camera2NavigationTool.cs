using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Experimental.Input;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools;

/// <summary>
/// Provides navigation controls for a 2D camera.
/// </summary>
/// <remarks>
/// Supports camera movement and zooming around the current cursor position.
/// The camera position is adjusted during zooming so that the world position
/// beneath the cursor remains fixed.
/// </remarks>
public sealed class Camera2NavigationTool : ToolBase
{
    private Camera2NavigationToolInteractions? _interactions;
    private ICursorState? _cursorState;
    private ICamera2D? _camera;
    private bool _isInViewport; 
    
    private Vector2 _previousScreenPosition;
    private bool _hasPreviousScreenPosition;

    /// <summary>
    /// Updates the camera navigation.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public override void Update(SceneUpdateContext context)
    {
        if (_camera == null ||
            _cursorState == null ||
            _interactions == null ||
            !_isInViewport)
        {
            _hasPreviousScreenPosition = false;
            return;
        }

        Vector2 currentScreenPosition = _cursorState.ScreenPosition;

        if (_interactions.Action.HasFlag(InteractionFlags.IsPressed))
        {
            if (_hasPreviousScreenPosition)
            {
                Vector2 movement =
                    currentScreenPosition - _previousScreenPosition;

                _camera.Position -= movement / _camera.Zoom;
            }

            _previousScreenPosition = currentScreenPosition;
            _hasPreviousScreenPosition = true;
        }
        else
        {
            _hasPreviousScreenPosition = false;
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

    /// <inheritdoc/>
    protected override ToolInteractions CreateInteractions()
    {
        return new Camera2NavigationToolInteractions();
    }

    /// <inheritdoc/>
    protected internal override void ApplyContext(
        ToolContext context)
    {
        _camera = context.Camera as ICamera2D;
        _cursorState = context.CursorState;
        _interactions = context.Interactions as Camera2NavigationToolInteractions;
        _isInViewport = context.CursorState.IsInViewport;
    }

    private void ZoomAtCursor(
        ICamera2D camera,
        float factor)
    {
        Vector2 before = _cursorState!.GetWorldPosition(camera);

        camera.Zoom *= factor;

        Vector2 after = _cursorState.GetWorldPosition(camera);

        camera.Position += before - after;
    }
}
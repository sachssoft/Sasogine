using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a parallax layer that follows the movement of a two-dimensional camera.
/// </summary>
public class FollowParallaxLayer : ParallaxLayerBase
{
    private Vector2 _parallaxOffset;
    private Point2 _originCameraPosition;
    private bool _hasOrigin;

    /// <summary>
    /// Gets or sets an additional multiplier applied to camera movement.
    /// </summary>
    public Vector2 Factor { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets the spacing used by repeating layer content.
    /// </summary>
    public Vector2 Spacing { get; set; }

    /// <inheritdoc/>
    public override Vector2 ParallaxOffset => _parallaxOffset;

    /// <inheritdoc/>
    public override void Update(SceneUpdateContext context)
    {
        if (!IsEnabled || context.Cameras.Length == 0 || context.Cameras[0] is not ICamera2 camera)
            return;

        Point2 cameraPosition = camera.Position;

        if (!_hasOrigin)
        {
            _originCameraPosition = cameraPosition;
            _hasOrigin = true;
            return;
        }

        Vector2 movement = cameraPosition.ToVector2() - _originCameraPosition.ToVector2();
        _parallaxOffset = movement * Factor * GetDepthFactor();
    }

    /// <inheritdoc/>
    public override void Draw(SceneDrawContext context)
    {
        if (!IsVisible)
            return;
    }

    /// <inheritdoc/>
    public override void Reset()
    {
        _parallaxOffset = Vector2.Zero;
        _originCameraPosition = default;
        _hasOrigin = false;
    }
}

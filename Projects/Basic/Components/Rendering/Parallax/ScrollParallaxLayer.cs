using Microsoft.Xna.Framework;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides a parallax layer that continuously scrolls over time.
/// </summary>
public class ScrollParallaxLayer : ParallaxLayerBase
{
    private Vector2 _scrollOffset;

    /// <summary>
    /// Gets or sets the scrolling speed in units per second.
    /// </summary>
    public Vector2 ScrollSpeed { get; set; }

    /// <summary>
    /// Gets or sets an additional multiplier applied to scrolling movement.
    /// </summary>
    public Vector2 Factor { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets the spacing used by repeating layer content.
    /// </summary>
    public Vector2 Spacing { get; set; }

    /// <summary>
    /// Gets the accumulated scrolling displacement.
    /// </summary>
    public Vector2 ScrollOffset => _scrollOffset;

    /// <inheritdoc/>
    public override Vector2 ParallaxOffset => _scrollOffset;

    /// <inheritdoc/>
    public override void Update(SceneUpdateContext context)
    {
        if (!IsEnabled)
            return;

        float deltaTime = (float)context.GameTime.ElapsedGameTime.TotalSeconds;
        _scrollOffset += ScrollSpeed * Factor * deltaTime;
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
        _scrollOffset = Vector2.Zero;
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Provides the base implementation for a runtime parallax layer component.
/// </summary>
public abstract class ParallaxLayerBase : ResourceComponentBase, IParallaxLayerComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether the layer is updated.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the layer is rendered.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the layer name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the draw order of the layer.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the parallax depth of the layer.
    /// </summary>
    /// <remarks>
    /// A value of <c>1</c> produces the default movement ratio. Greater values
    /// reduce relative movement while values between <c>0</c> and <c>1</c>
    /// increase it.
    /// </remarks>
    public float Depth { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the absolute position of the layer.
    /// </summary>
    public Point2 Position { get; set; }

    /// <summary>
    /// Gets or sets an additional positional offset applied to the layer.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets or sets the scale of the layer.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets the texture rendered by the layer.
    /// </summary>
    public Texture2D? Texture { get; set; }

    /// <summary>
    /// Gets or sets the stretch mode used when rendering the layer.
    /// </summary>
    public StretchMode StretchMode { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of the layer.
    /// </summary>
    public Alignment VerticalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of the layer.
    /// </summary>
    public Alignment HorizontalAlignment { get; set; }

    /// <summary>
    /// Gets the current runtime displacement produced by the parallax behavior.
    /// </summary>
    public abstract Vector2 ParallaxOffset { get; }

    /// <summary>
    /// Sets the draw order of the layer.
    /// </summary>
    /// <param name="index">The new draw order.</param>
    public void SetDrawOrder(int index) => Index = index;

    /// <summary>
    /// Resets the runtime state of the parallax layer.
    /// </summary>
    public virtual void Reset() { }

    /// <summary>
    /// Calculates the movement factor for the configured parallax depth.
    /// </summary>
    /// <returns>The movement factor for the current layer depth.</returns>
    protected float GetDepthFactor()
    {
        return Depth > 0f ? 1f / Depth : 1f;
    }
}

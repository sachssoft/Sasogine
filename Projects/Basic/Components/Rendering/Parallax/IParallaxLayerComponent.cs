using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Defines a component that represents an individual parallax layer.
/// </summary>
public interface IParallaxLayerComponent : IUpdatableComponent, IDrawableComponent
{
    /// <summary>
    /// Gets or sets a value indicating whether the layer is updated.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the layer is rendered.
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the layer name.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Gets or sets the draw order of the layer.
    /// </summary>
    int Index { get; set; }

    /// <summary>
    /// Gets or sets the parallax depth of the layer.
    /// </summary>
    float Depth { get; set; }

    /// <summary>
    /// Gets or sets the absolute position of the layer.
    /// </summary>
    Point2 Position { get; set; }

    /// <summary>
    /// Gets or sets an additional positional offset applied to the layer.
    /// </summary>
    Vector2 Offset { get; set; }

    /// <summary>
    /// Gets or sets the scale of the layer.
    /// </summary>
    Vector2 Scale { get; set; }

    /// <summary>
    /// Gets or sets the texture rendered by the layer.
    /// </summary>
    Texture2D? Texture { get; set; }

    /// <summary>
    /// Gets or sets the stretch mode used when rendering the layer.
    /// </summary>
    StretchMode StretchMode { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of the layer.
    /// </summary>
    Alignment VerticalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of the layer.
    /// </summary>
    Alignment HorizontalAlignment { get; set; }

    /// <summary>
    /// Gets the current runtime displacement produced by the parallax behavior.
    /// </summary>
    Vector2 ParallaxOffset { get; }

    /// <summary>
    /// Sets the draw order of the layer.
    /// </summary>
    /// <param name="index">The new draw order.</param>
    void SetDrawOrder(int index);

    /// <summary>
    /// Resets the runtime parallax state.
    /// </summary>
    void Reset();
}

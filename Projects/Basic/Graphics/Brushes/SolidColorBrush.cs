using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Engine.Graphics.Brushes;

/// <summary>
/// Defines a brush that renders shape geometry using a solid vertex color.
/// </summary>
public sealed class SolidColorBrush : Brush
{
    /// <summary>
    /// Initializes a new solid color brush.
    /// </summary>
    /// <param name="color">The color used by the brush.</param>
    public SolidColorBrush(Color color)
    {
        Color = color;
    }

    /// <summary>
    /// Gets or sets the color used by the brush.
    /// </summary>
    public Color Color { get; set; }

    /// <inheritdoc/>
    protected internal override void Apply(
        ref ShapeVertex vertex,
        in BrushVertexContext context)
    {
        vertex.Color = Color;
        vertex.TextureCoordinate = context.TextureCoordinate;
    }

    /// <inheritdoc/>
    protected internal override bool CanBatchWith(Brush other) =>
        other is SolidColorBrush;
}

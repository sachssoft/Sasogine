using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Engine.Graphics.Brushes;

/// <summary>
/// Defines a linear gradient brush whose colors are evaluated per vertex.
/// </summary>
public sealed class LinearGradientBrush : Brush
{
    /// <summary>
    /// Initializes a new normalized horizontal linear gradient brush.
    /// </summary>
    /// <param name="startColor">The color at the gradient start.</param>
    /// <param name="endColor">The color at the gradient end.</param>
    public LinearGradientBrush(
        Color startColor,
        Color endColor)
        : this(
            Vector2.Zero,
            Vector2.UnitX,
            startColor,
            endColor)
    {
    }

    /// <summary>
    /// Initializes a new linear gradient brush.
    /// </summary>
    /// <param name="start">The gradient start coordinate.</param>
    /// <param name="end">The gradient end coordinate.</param>
    /// <param name="startColor">The color at the gradient start.</param>
    /// <param name="endColor">The color at the gradient end.</param>
    public LinearGradientBrush(
        Vector2 start,
        Vector2 end,
        Color startColor,
        Color endColor)
    {
        Start = start;
        End = end;
        StartColor = startColor;
        EndColor = endColor;
    }

    /// <summary>
    /// Gets or sets the coordinate space in which <see cref="Start"/> and
    /// <see cref="End"/> are evaluated.
    /// </summary>
    public BrushCoordinateSpace CoordinateSpace { get; set; } =
        BrushCoordinateSpace.Normalized;

    /// <summary>
    /// Gets or sets the gradient start coordinate.
    /// </summary>
    public Vector2 Start { get; set; }

    /// <summary>
    /// Gets or sets the gradient end coordinate.
    /// </summary>
    public Vector2 End { get; set; }

    /// <summary>
    /// Gets or sets the color at the gradient start.
    /// </summary>
    public Color StartColor { get; set; }

    /// <summary>
    /// Gets or sets the color at the gradient end.
    /// </summary>
    public Color EndColor { get; set; }

    /// <inheritdoc/>
    protected internal override void Apply(
        ref ShapeVertex vertex,
        in BrushVertexContext context)
    {
        Vector2 position = context.GetPosition(CoordinateSpace);
        Vector2 direction = End - Start;
        float lengthSquared = direction.LengthSquared();

        float amount = lengthSquared <= float.Epsilon
            ? 0f
            : float.Clamp(
                Vector2.Dot(position - Start, direction) / lengthSquared,
                0f,
                1f);

        vertex.Color = Color.Lerp(StartColor, EndColor, amount);
        vertex.TextureCoordinate = context.TextureCoordinate;
    }

    /// <inheritdoc/>
    protected internal override bool CanBatchWith(Brush other) =>
        other is LinearGradientBrush;
}

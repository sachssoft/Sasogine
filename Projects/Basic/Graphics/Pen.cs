using System;

namespace Sachssoft.Engine.Graphics.Rendering.Batches;

/// <summary>
/// Defines the brush and geometry settings used to render a shape stroke.
/// </summary>
public sealed class Pen
{
    /// <summary>
    /// Initializes a new pen.
    /// </summary>
    /// <param name="brush">The brush used to render the stroke.</param>
    /// <param name="thickness">The stroke thickness.</param>
    public Pen(Brush brush, float thickness = 1f)
    {
        Brush = brush ?? throw new ArgumentNullException(nameof(brush));
        Thickness = thickness;
    }

    /// <summary>
    /// Gets or sets the brush used to render the stroke.
    /// </summary>
    public Brush Brush { get; set; }

    /// <summary>
    /// Gets or sets the stroke thickness.
    /// </summary>
    public float Thickness { get; set; }

    /// <summary>
    /// Gets or sets the join style used between connected segments.
    /// </summary>
    public LineJoin Join { get; set; } = LineJoin.Miter;

    /// <summary>
    /// Gets or sets the cap style used at open stroke endpoints.
    /// </summary>
    public LineCap Cap { get; set; } = LineCap.Butt;
}

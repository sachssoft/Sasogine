using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Shapes;

/// <summary>
/// Represents an elliptical shape path defined within normalized
/// coordinates and approximated using configurable path segments.
/// </summary>
public class EllipsePath : ShapePathBase
{
    /// <summary>
    /// Gets the number of segments used to construct the full ellipse.
    /// Must be at least three; higher values produce a more detailed approximation.
    /// </summary>
    public int Segments
    {
        get;
        init;
    } = 64;

    /// <summary>
    /// Gets the rounding method used to construct the individual
    /// segments of the ellipse.
    /// </summary>
    /// <remarks>
    /// The contour samples only segment endpoints, so all supported rounding
    /// methods produce the same polyline approximation.
    /// </remarks>
    public RoundingType RoundingType
    {
        get;
        init;
    } = RoundingType.Linear;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// The segment count or interpolation type is invalid.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        ShapePathGeometry.ValidateSegments(Segments, 3);
        ShapePathGeometry.ValidateRoundingType(RoundingType, nameof(RoundingType));

        var points = new List<Vector2>();

        for (int i = 0; i < Segments; i++)
        {
            float angle = i / (float)Segments * MathHelper.TwoPi;

            points.Add(new Vector2(
                0.5f + 0.5f * MathF.Cos(angle),
                0.5f + 0.5f * MathF.Sin(angle)));
        }

        return ShapePathGeometry.CreatePath(points);
    }
}

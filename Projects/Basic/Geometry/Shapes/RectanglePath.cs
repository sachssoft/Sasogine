using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Shapes;

/// <summary>
/// Represents a normalized rectangular path with independently configurable
/// corner shapes and corner sizes.
/// </summary>
/// <remarks>
/// The rectangle is generated in normalized coordinates ranging from
/// <c>0</c> to <c>1</c>. Corner sizes are proportionally reduced during
/// generation when adjacent corners would overlap along an edge.
/// </remarks>
public class RectanglePath : ShapePathBase
{
    /// <summary>
    /// Stores the normalized bottom-left corner size.
    /// </summary>
    private Size2 _bottomLeftEdgeSize = Size2.Zero;

    /// <summary>
    /// Stores the normalized bottom-right corner size.
    /// </summary>
    private Size2 _bottomRightEdgeSize = Size2.Zero;

    /// <summary>
    /// Stores the normalized top-left corner size.
    /// </summary>
    private Size2 _topLeftEdgeSize = Size2.Zero;

    /// <summary>
    /// Stores the normalized top-right corner size.
    /// </summary>
    private Size2 _topRightEdgeSize = Size2.Zero;

    /// <summary>
    /// Gets the number of segments used to sample rounded corners; must be at least one.
    /// </summary>
    public int Segments
    {
        get;
        init;
    } = 8;

    /// <summary>
    /// Gets the rounding type used for the top-left corner.
    /// </summary>
    public RoundingType TopLeftEdgeType
    {
        get;
        init;
    } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the top-right corner.
    /// </summary>
    public RoundingType TopRightEdgeType
    {
        get;
        init;
    } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the bottom-left corner.
    /// </summary>
    public RoundingType BottomLeftEdgeType
    {
        get;
        init;
    } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the bottom-right corner.
    /// </summary>
    public RoundingType BottomRightEdgeType
    {
        get;
        init;
    } = RoundingType.Linear;

    /// <summary>
    /// Gets the normalized size of the top-left corner.
    /// </summary>
    public Size2 TopLeftEdgeSize
    {
        get => _topLeftEdgeSize;
        init => _topLeftEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the top-right corner.
    /// </summary>
    public Size2 TopRightEdgeSize
    {
        get => _topRightEdgeSize;
        init => _topRightEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the bottom-left corner.
    /// </summary>
    public Size2 BottomLeftEdgeSize
    {
        get => _bottomLeftEdgeSize;
        init => _bottomLeftEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the bottom-right corner.
    /// </summary>
    public Size2 BottomRightEdgeSize
    {
        get => _bottomRightEdgeSize;
        init => _bottomRightEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Builds the path representing the rectangle.
    /// </summary>
    /// <returns>
    /// A <see cref="Path"/> containing the generated rectangle geometry.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The segment count or a corner interpolation type is invalid.
    /// </exception>
    protected override Path BuildDefinedPath()
    {
        ShapePathGeometry.ValidateSegments(Segments);
        ShapePathGeometry.ValidateRoundingType(TopLeftEdgeType, nameof(TopLeftEdgeType));
        ShapePathGeometry.ValidateRoundingType(TopRightEdgeType, nameof(TopRightEdgeType));
        ShapePathGeometry.ValidateRoundingType(BottomLeftEdgeType, nameof(BottomLeftEdgeType));
        ShapePathGeometry.ValidateRoundingType(BottomRightEdgeType, nameof(BottomRightEdgeType));

        float largestSum = float.Max(
            float.Max(
                TopLeftEdgeSize.Width + TopRightEdgeSize.Width,
                BottomLeftEdgeSize.Width + BottomRightEdgeSize.Width),
            float.Max(
                TopLeftEdgeSize.Height + BottomLeftEdgeSize.Height,
                TopRightEdgeSize.Height + BottomRightEdgeSize.Height));

        float scale = largestSum > 1f ? 1f / largestSum : 1f;
        var polygon = new List<Vector2>();

        Vector2 topLeft = new(0f, 0f);
        Vector2 topRight = new(1f, 0f);
        Vector2 bottomRight = new(1f, 1f);
        Vector2 bottomLeft = new(0f, 1f);

        polygon.AddRange(
            RoundCorner(
                topLeft,
                topRight,
                bottomLeft,
                new Size2(TopLeftEdgeSize.Width * scale, TopLeftEdgeSize.Height * scale),
                TopLeftEdgeType,
                Segments));

        polygon.AddRange(
            RoundCorner(
                topRight,
                bottomRight,
                topLeft,
                new Size2(TopRightEdgeSize.Width * scale, TopRightEdgeSize.Height * scale),
                TopRightEdgeType,
                Segments));

        polygon.AddRange(
            RoundCorner(
                bottomRight,
                bottomLeft,
                topRight,
                new Size2(BottomRightEdgeSize.Width * scale, BottomRightEdgeSize.Height * scale),
                BottomRightEdgeType,
                Segments));

        polygon.AddRange(
            RoundCorner(
                bottomLeft,
                topLeft,
                bottomRight,
                new Size2(BottomLeftEdgeSize.Width * scale, BottomLeftEdgeSize.Height * scale),
                BottomLeftEdgeType,
                Segments));

        return ShapePathGeometry.CreatePath(polygon);
    }

    /// <summary>
    /// Samples one rectangle corner without crossing either adjacent edge.
    /// </summary>
    /// <param name="corner">The original rectangle vertex.</param>
    /// <param name="next">The next vertex in contour order.</param>
    /// <param name="previous">The previous vertex in contour order.</param>
    /// <param name="size">The scaled horizontal and vertical corner cuts.</param>
    /// <param name="type">The corner interpolation type.</param>
    /// <param name="segments">The curve sampling count.</param>
    /// <returns>The corner's contour points.</returns>
    private static Vector2[] RoundCorner(
        Vector2 corner,
        Vector2 next,
        Vector2 previous,
        Size2 size,
        RoundingType type,
        int segments)
    {
        if (size == Size2.Zero)
            return new[] { corner };

        Vector2 previousDirection =
            VectorMath.SafeNormalize(corner - previous);

        Vector2 nextDirection =
            VectorMath.SafeNormalize(next - corner);

        float previousRadius =
            previousDirection.X != 0f
                ? size.Width
                : size.Height;

        float nextRadius =
            nextDirection.X != 0f
                ? size.Width
                : size.Height;

        Vector2 start =
            corner - previousDirection * previousRadius;

        Vector2 end =
            corner + nextDirection * nextRadius;

        return type switch
        {
            RoundingType.Linear =>
                new[]
                {
                    start,
                    end
                },

            RoundingType.Quadratic =>
                GeometrySampler.SampleQuadraticBezier(
                    start,
                    corner,
                    end,
                    segments),

            RoundingType.Cubic =>
                GeometrySampler.SampleCubicBezier(
                    start,
                    start + previousDirection * previousRadius * 0.5f,
                    end - nextDirection * nextRadius * 0.5f,
                    end,
                    segments),

            _ => new[] { corner }
        };
    }

    /// <summary>
    /// Validates and clamps a normalized corner size.
    /// </summary>
    /// <param name="size">The requested corner size.</param>
    /// <returns>The corner size clamped to the unit rectangle.</returns>
    /// <exception cref="InvalidOperationException">A size component is not finite.</exception>
    private static Size2 CoerceSize(Size2 size)
    {
        ShapePathGeometry.ValidateFinite(size.Width, nameof(size));
        ShapePathGeometry.ValidateFinite(size.Height, nameof(size));

        return new Size2(
            MathHelper.Clamp(size.Width, 0f, 1f),
            MathHelper.Clamp(size.Height, 0f, 1f));
    }
}
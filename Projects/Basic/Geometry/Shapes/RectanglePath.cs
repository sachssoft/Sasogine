using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes;

/// <summary>
/// Represents a normalized rectangular path with independently configurable
/// corner shapes and corner sizes.
/// </summary>
/// <remarks>
/// The rectangle is generated in normalized coordinates ranging from
/// <c>0</c> to <c>1</c>.
/// </remarks>
public class RectanglePath : ShapePathBase
{
    private Size2 _topLeftEdgeSize = Size2.Zero;
    private Size2 _topRightEdgeSize = Size2.Zero;
    private Size2 _bottomLeftEdgeSize = Size2.Zero;
    private Size2 _bottomRightEdgeSize = Size2.Zero;

    /// <summary>
    /// Gets the number of segments used to sample rounded corners.
    /// </summary>
    public int Segments { get; init; } = 8;

    /// <summary>
    /// Gets the rounding type used for the top-left corner.
    /// </summary>
    public RoundingType TopLeftEdgeType { get; init; } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the top-right corner.
    /// </summary>
    public RoundingType TopRightEdgeType { get; init; } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the bottom-left corner.
    /// </summary>
    public RoundingType BottomLeftEdgeType { get; init; } = RoundingType.Linear;

    /// <summary>
    /// Gets the rounding type used for the bottom-right corner.
    /// </summary>
    public RoundingType BottomRightEdgeType { get; init; } = RoundingType.Linear;

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
    protected override Path BuildDefinedPath()
    {
        var polygon = new List<Point2>();

        Point2 topLeft = new(0f, 0f);
        Point2 topRight = new(1f, 0f);
        Point2 bottomRight = new(1f, 1f);
        Point2 bottomLeft = new(0f, 1f);

        Point2[] RoundCorner(
            Point2 corner,
            Point2 next,
            Point2 previous,
            Size2 size,
            RoundingType type)
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

            Point2 start =
                corner - previousDirection * previousRadius;

            Point2 end =
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
                        Segments),

                RoundingType.Cubic =>
                    GeometrySampler.SampleCubicBezier(
                        start,
                        start + previousDirection * previousRadius * 0.5f,
                        end - nextDirection * nextRadius * 0.5f,
                        end,
                        Segments),

                _ => new[] { corner }
            };
        }

        polygon.AddRange(
            RoundCorner(
                topLeft,
                topRight,
                bottomLeft,
                TopLeftEdgeSize,
                TopLeftEdgeType));

        polygon.AddRange(
            RoundCorner(
                topRight,
                bottomRight,
                topLeft,
                TopRightEdgeSize,
                TopRightEdgeType));

        polygon.AddRange(
            RoundCorner(
                bottomRight,
                bottomLeft,
                topRight,
                BottomRightEdgeSize,
                BottomRightEdgeType));

        polygon.AddRange(
            RoundCorner(
                bottomLeft,
                topLeft,
                bottomRight,
                BottomLeftEdgeSize,
                BottomLeftEdgeType));

        if (polygon.Count > 0 &&
            polygon[0] != polygon[^1])
        {
            polygon.Add(polygon[0]);
        }

        return new Path(
            new[]
            {
                polygon.ToArray()
            });
    }

    private static Size2 CoerceSize(Size2 size)
    {
        return new Size2(
            MathHelper.Clamp(size.Width, 0f, 1f),
            MathHelper.Clamp(size.Height, 0f, 1f));
    }
}
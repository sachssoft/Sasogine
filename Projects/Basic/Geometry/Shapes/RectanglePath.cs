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
    private Vector2 _topLeftEdgeSize = Vector2.Zero;
    private Vector2 _topRightEdgeSize = Vector2.Zero;
    private Vector2 _bottomLeftEdgeSize = Vector2.Zero;
    private Vector2 _bottomRightEdgeSize = Vector2.Zero;

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
    public Vector2 TopLeftEdgeSize
    {
        get => _topLeftEdgeSize;
        init => _topLeftEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the top-right corner.
    /// </summary>
    public Vector2 TopRightEdgeSize
    {
        get => _topRightEdgeSize;
        init => _topRightEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the bottom-left corner.
    /// </summary>
    public Vector2 BottomLeftEdgeSize
    {
        get => _bottomLeftEdgeSize;
        init => _bottomLeftEdgeSize = CoerceSize(value);
    }

    /// <summary>
    /// Gets the normalized size of the bottom-right corner.
    /// </summary>
    public Vector2 BottomRightEdgeSize
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
        var polygon = new List<Vector2>();

        Vector2 topLeft = new(0f, 0f);
        Vector2 topRight = new(1f, 0f);
        Vector2 bottomRight = new(1f, 1f);
        Vector2 bottomLeft = new(0f, 1f);

        Vector2[] RoundCorner(
            Vector2 corner,
            Vector2 next,
            Vector2 previous,
            Vector2 size,
            RoundingType type)
        {
            if (size == Vector2.Zero)
                return new[] { corner };

            Vector2 previousDirection =
                VectorMath.SafeNormalize(corner - previous);

            Vector2 nextDirection =
                VectorMath.SafeNormalize(next - corner);

            float previousRadius =
                previousDirection.X != 0f
                    ? size.X
                    : size.Y;

            float nextRadius =
                nextDirection.X != 0f
                    ? size.X
                    : size.Y;

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

    private static Vector2 CoerceSize(Vector2 size)
    {
        return new Vector2(
            MathHelper.Clamp(size.X, 0f, 1f),
            MathHelper.Clamp(size.Y, 0f, 1f));
    }
}
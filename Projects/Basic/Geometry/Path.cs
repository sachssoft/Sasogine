using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Represents an immutable collection of polygon contours.
/// </summary>
/// <remarks>
/// A <see cref="Path"/> cannot be modified after construction.
/// Operations that change the geometry return a new <see cref="Path"/> instance.
/// This makes the type suitable for geometry caching and dictionary keys.
/// </remarks>
public sealed class Path :
    IEnumerable<IReadOnlyList<Point2>>,
    ICloneable,
    IEquatable<Path>
{
    private readonly Vector2[][] _polygons;
    private readonly PolygonDirection[] _directions;
    private readonly Box2[] _polygonBounds;
    private readonly Box2 _bounds;
    private readonly int _hashCode;

    /// <summary>
    /// Gets an empty path.
    /// </summary>
    public static Path Empty { get; } =
        new Path(Array.Empty<Vector2[]>(), true);

    /// <summary>
    /// Initializes an empty path.
    /// </summary>
    public Path()
        : this(Array.Empty<Vector2[]>(), true)
    {
    }

    /// <summary>
    /// Initializes a path containing a single polygon from the specified points.
    /// </summary>
    /// <param name="points">The polygon points.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the polygon contains fewer than three points or contains non-finite coordinates.
    /// </exception>
    public Path(Point2[] points)
        : this(new[] { points })
    {
    }

    /// <summary>
    /// Initializes a path containing a single polygon from the specified vectors.
    /// </summary>
    /// <param name="points">The polygon points represented as vectors.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the polygon contains fewer than three points or contains non-finite coordinates.
    /// </exception>
    public Path(Vector2[] points)
        : this(new[] { points })
    {
    }

    /// <summary>
    /// Initializes a path containing a single polygon from the specified pixel points.
    /// </summary>
    /// <param name="points">The polygon points represented as pixel coordinates.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the polygon contains fewer than three points.
    /// </exception>
    public Path(PixelPoint2[] points)
        : this(new[] { points })
    {
    }

    /// <summary>
    /// Initializes a path from a collection of polygon contours represented by points.
    /// </summary>
    /// <param name="polygons">The polygon contours.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="polygons"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when a polygon is <see langword="null"/>, contains fewer than three points,
    /// or contains non-finite coordinates.
    /// </exception>
    public Path(IEnumerable<Point2[]> polygons)
    {
        ArgumentNullException.ThrowIfNull(polygons);

        var list = new List<Vector2[]>();

        foreach (var polygon in polygons)
        {
            if (polygon == null)
            {
                throw new ArgumentException(
                    "Polygon cannot be null.",
                    nameof(polygons));
            }

            if (polygon.Length < 3)
            {
                throw new ArgumentException(
                    "Polygon must have at least 3 points.",
                    nameof(polygons));
            }

            var vectors = new Vector2[polygon.Length];

            for (int i = 0; i < polygon.Length; i++)
            {
                Point2 point = polygon[i];

                if (!IsFinite(point))
                {
                    throw new ArgumentException(
                        "Polygon coordinates must be finite.",
                        nameof(polygons));
                }

                vectors[i] = point;
            }

            list.Add(vectors);
        }

        _polygons = list.ToArray();

        _directions = new PolygonDirection[_polygons.Length];
        _polygonBounds = new Box2[_polygons.Length];

        InitializeGeometry();

        _bounds = ComputeBounds(_polygonBounds);
        ValidateBounds(_bounds);

        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Initializes a path from a collection of polygon contours represented by vectors.
    /// </summary>
    /// <param name="polygons">The polygon contours.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="polygons"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when a polygon is <see langword="null"/>, contains fewer than three points,
    /// or contains non-finite coordinates.
    /// </exception>
    public Path(IEnumerable<Vector2[]> polygons)
    {
        ArgumentNullException.ThrowIfNull(polygons);

        var list = new List<Vector2[]>();

        foreach (var polygon in polygons)
        {
            if (polygon == null)
            {
                throw new ArgumentException(
                    "Polygon cannot be null.",
                    nameof(polygons));
            }

            if (polygon.Length < 3)
            {
                throw new ArgumentException(
                    "Polygon must have at least 3 points.",
                    nameof(polygons));
            }

            for (int i = 0; i < polygon.Length; i++)
            {
                if (!IsFinite(polygon[i]))
                {
                    throw new ArgumentException(
                        "Polygon coordinates must be finite.",
                        nameof(polygons));
                }
            }

            list.Add((Vector2[])polygon.Clone());
        }

        _polygons = list.ToArray();

        _directions = new PolygonDirection[_polygons.Length];
        _polygonBounds = new Box2[_polygons.Length];

        InitializeGeometry();

        _bounds = ComputeBounds(_polygonBounds);
        ValidateBounds(_bounds);

        _hashCode = ComputeHashCode();
    }



    /// <summary>
    /// Initializes a path from a collection of polygon contours represented
    /// by pixel points.
    /// </summary>
    /// <param name="polygons">The polygon contours.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="polygons"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when a polygon is <see langword="null"/> or contains
    /// fewer than three points.
    /// </exception>
    public Path(IEnumerable<PixelPoint2[]> polygons)
    {
        ArgumentNullException.ThrowIfNull(polygons);

        var list = new List<Vector2[]>();

        foreach (var polygon in polygons)
        {
            if (polygon == null)
            {
                throw new ArgumentException(
                    "Polygon cannot be null.",
                    nameof(polygons));
            }

            if (polygon.Length < 3)
            {
                throw new ArgumentException(
                    "Polygon must have at least 3 points.",
                    nameof(polygons));
            }

            var vectors = new Vector2[polygon.Length];

            for (int i = 0; i < polygon.Length; i++)
            {
                vectors[i] = new Vector2(
                    polygon[i].X,
                    polygon[i].Y);
            }

            list.Add(vectors);
        }

        _polygons = list.ToArray();

        _directions = new PolygonDirection[_polygons.Length];
        _polygonBounds = new Box2[_polygons.Length];

        InitializeGeometry();

        _bounds = ComputeBounds(_polygonBounds);
        ValidateBounds(_bounds);

        _hashCode = ComputeHashCode();
    }

    private Path(
        Vector2[][] polygons,
        bool takeOwnership)
    {
        ArgumentNullException.ThrowIfNull(polygons);

        _polygons = takeOwnership
            ? polygons
            : ClonePolygons(polygons);

        _directions = new PolygonDirection[_polygons.Length];
        _polygonBounds = new Box2[_polygons.Length];

        InitializeGeometry();

        _bounds = ComputeBounds(_polygonBounds);
        ValidateBounds(_bounds);

        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Gets the number of polygons contained in the path.
    /// </summary>
    public int PolygonCount => _polygons.Length;

    /// <summary>
    /// Gets a value indicating whether the path contains no polygons.
    /// </summary>
    public bool IsEmpty => _polygons.Length == 0;

    /// <summary>
    /// Gets the axis-aligned bounds of the complete path.
    /// </summary>
    public Box2 Bounds => _bounds;

    /// <summary>
    /// Gets the total width of the path bounds.
    /// </summary>
    public float Width => _bounds.Width;

    /// <summary>
    /// Gets the total height of the path bounds.
    /// </summary>
    public float Height => _bounds.Height;

    /// <summary>
    /// Gets the left coordinate of the path bounds.
    /// </summary>
    public float Left => _bounds.MinX;

    /// <summary>
    /// Gets the top coordinate of the path bounds.
    /// </summary>
    public float Top => _bounds.MinY;

    /// <summary>
    /// Gets the right coordinate of the path bounds.
    /// </summary>
    public float Right => _bounds.MaxX;

    /// <summary>
    /// Gets the bottom coordinate of the path bounds.
    /// </summary>
    public float Bottom => _bounds.MaxY;

    /// <summary>
    /// Gets the minimum coordinates of the path bounds.
    /// </summary>
    public Point2 LowerBound => _bounds.Min;

    /// <summary>
    /// Gets the maximum coordinates of the path bounds.
    /// </summary>
    public Point2 UpperBound => _bounds.Max;

    /// <summary>
    /// Gets the center point of the path bounds.
    /// </summary>
    public Point2 Origin =>
        new Point2(
            (Left + Right) * 0.5f,
            (Top + Bottom) * 0.5f);

    /// <summary>
    /// Creates a rectangular path.
    /// </summary>
    /// <param name="x">The left coordinate.</param>
    /// <param name="y">The top coordinate.</param>
    /// <param name="width">The rectangle width.</param>
    /// <param name="height">The rectangle height.</param>
    /// <returns>A new rectangular path.</returns>
    public static Path CreateRectangle(
        float x,
        float y,
        float width,
        float height)
    {
        return new Path(
            new[]
            {
                new Point2(x, y),
                new Point2(x + width, y),
                new Point2(x + width, y + height),
                new Point2(x, y + height)
            });
    }

    /// <summary>
    /// Gets the number of polygons contained in the path.
    /// </summary>
    /// <returns>The polygon count.</returns>
    public int GetPolygonCount()
    {
        return _polygons.Length;
    }

    /// <summary>
    /// Gets a copy of the points of the specified polygon.
    /// </summary>
    /// <param name="index">The polygon index.</param>
    /// <returns>The polygon points.</returns>
    public Point2[] GetPolygonPoints(int index)
    {
        ValidateIndex(index, _polygons.Length, nameof(index));
        return ConvertToPoints(_polygons[index]);
    }

    /// <summary>
    /// Gets the winding direction of the specified polygon.
    /// </summary>
    /// <param name="index">The polygon index.</param>
    /// <returns>The polygon direction.</returns>
    public PolygonDirection GetPolygonDirection(int index)
    {
        ValidateIndex(index, _polygons.Length, nameof(index));
        return _directions[index];
    }

    /// <summary>
    /// Gets the cached axis-aligned bounds of the specified polygon.
    /// </summary>
    /// <param name="index">The polygon index.</param>
    /// <returns>The polygon bounds.</returns>
    public Box2 GetPolygonBounds(int index)
    {
        ValidateIndex(index, _polygons.Length, nameof(index));
        return _polygonBounds[index];
    }

    /// <summary>
    /// Gets a point from the specified polygon.
    /// </summary>
    /// <param name="polygonIndex">The polygon index.</param>
    /// <param name="pointIndex">The point index.</param>
    /// <returns>The requested point.</returns>
    public Point2 GetPoint(
        int polygonIndex,
        int pointIndex)
    {
        ValidateIndex(polygonIndex, _polygons.Length, nameof(polygonIndex));

        Vector2[] polygon = _polygons[polygonIndex];
        ValidateIndex(pointIndex, polygon.Length, nameof(pointIndex));

        Vector2 point = polygon[pointIndex];

        return new Point2(
            point.X,
            point.Y);
    }

    /// <summary>
    /// Gets the number of points contained in the specified polygon.
    /// </summary>
    /// <param name="polygonIndex">The polygon index.</param>
    /// <returns>The number of points.</returns>
    public int GetPointCount(int polygonIndex)
    {
        ValidateIndex(polygonIndex, _polygons.Length, nameof(polygonIndex));
        return _polygons[polygonIndex].Length;
    }

    /// <summary>
    /// Creates a new path containing a single polygon from this path.
    /// </summary>
    /// <param name="index">The polygon index.</param>
    /// <returns>A new path containing the selected polygon.</returns>
    public Path PolygonToPath(int index)
    {
        ValidateIndex(index, _polygons.Length, nameof(index));

        return new Path(
            new[]
            {
                (Vector2[])_polygons[index].Clone()
            },
            true);
    }

    /// <summary>
    /// Creates a transformed copy of the path.
    /// </summary>
    /// <param name="transform">The transformation matrix to apply.</param>
    /// <returns>A new transformed path.</returns>
    /// <exception cref="InvalidOperationException">
    /// The transformation produces non-finite coordinates.
    /// </exception>
    public Path Transform(Matrix transform)
    {
        var polygons = new Vector2[_polygons.Length][];

        for (int i = 0; i < _polygons.Length; i++)
        {
            Vector2[] source = _polygons[i];
            var transformed = new Vector2[source.Length];

            for (int j = 0; j < source.Length; j++)
            {
                Vector2 point = Vector2.Transform(source[j], transform);

                if (!IsFinite(point))
                {
                    throw new InvalidOperationException(
                        "The transformation produced non-finite coordinates.");
                }

                transformed[j] = point;
            }

            polygons[i] = transformed;
        }

        return new Path(
            polygons,
            true);
    }

    /// <summary>
    /// Creates a transformed copy of the path using the specified
    /// point transformation function.
    /// </summary>
    /// <param name="transform">The point transformation function.</param>
    /// <returns>A new transformed path.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="transform"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The transformation produces non-finite coordinates.
    /// </exception>
    public Path Transform(
        Func<Point2, Point2> transform)
    {
        ArgumentNullException.ThrowIfNull(transform);

        var polygons = new Vector2[_polygons.Length][];

        for (int i = 0; i < _polygons.Length; i++)
        {
            Vector2[] source = _polygons[i];
            var transformed = new Vector2[source.Length];

            for (int j = 0; j < source.Length; j++)
            {
                Point2 sourcePoint =
                    new Point2(
                        source[j].X,
                        source[j].Y);

                Point2 result = transform(sourcePoint);

                if (!IsFinite(result))
                {
                    throw new ArgumentException(
                        "The transformation must produce finite coordinates.",
                        nameof(transform));
                }

                transformed[j] =
                    new Vector2(
                        result.X,
                        result.Y);
            }

            polygons[i] = transformed;
        }

        return new Path(
            polygons,
            true);
    }

    /// <summary>
    /// Determines whether the specified point lies inside a polygon.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="polygonIndex">The polygon index.</param>
    /// <param name="transform">
    /// The transformation applied to the polygon before testing.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the point lies inside the polygon;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsPointInPolygon(
        Point2 point,
        int polygonIndex,
        Matrix transform)
    {
        ValidateIndex(polygonIndex, _polygons.Length, nameof(polygonIndex));

        if (!IsFinite(point))
            return false;

        Vector2[] polygon = _polygons[polygonIndex];

        Box2 bounds =
            TransformBounds(
                _polygonBounds[polygonIndex],
                transform);

        if (!IsFinite(bounds))
            return false;

        if (point.X < bounds.MinX ||
            point.X > bounds.MaxX ||
            point.Y < bounds.MinY ||
            point.Y > bounds.MaxY)
        {
            return false;
        }

        Vector2 position =
            new Vector2(
                point.X,
                point.Y);

        float angleSum = 0f;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a =
                Vector2.Transform(
                    polygon[i],
                    transform);

            Vector2 b =
                Vector2.Transform(
                    polygon[(i + 1) % polygon.Length],
                    transform);

            if (!IsFinite(a) || !IsFinite(b))
                return false;

            Vector2 pa = a - position;
            Vector2 pb = b - position;

            angleSum += MathF.Atan2(
                pa.X * pb.Y - pa.Y * pb.X,
                Vector2.Dot(pa, pb));
        }

        return MathF.Abs(angleSum) > 0.0001f;
    }

    /// <summary>
    /// Creates a copy of the path translated so that its left and top bounds are zero.
    /// </summary>
    /// <returns>
    /// A new path whose left and top bounds are positioned at zero.
    /// </returns>
    public Path Trim()
    {
        if (IsEmpty || (Left == 0f && Top == 0f))
            return this;

        return TransformCoordinates(
            1d,
            1d,
            -(double)Left,
            -(double)Top);
    }

    /// <summary>
    /// Creates a trimmed and normalized copy of the path.
    /// </summary>
    /// <remarks>
    /// Non-degenerate dimensions are mapped to the range 0 to 1.
    /// Degenerate dimensions are collapsed to zero.
    /// </remarks>
    /// <returns>
    /// A trimmed and normalized copy of the path.
    /// </returns>
    public Path TrimNormalized()
    {
        if (IsEmpty)
            return this;

        float width = Width;
        float height = Height;

        if (Left == 0f &&
            Top == 0f &&
            (width == 0f || width == 1f) &&
            (height == 0f || height == 1f))
        {
            return this;
        }

        double scaleX = width == 0f ? 0d : 1d / width;
        double scaleY = height == 0f ? 0d : 1d / height;
        double offsetX = width == 0f ? 0d : -(double)Left * scaleX;
        double offsetY = height == 0f ? 0d : -(double)Top * scaleY;

        return TransformCoordinates(
            scaleX,
            scaleY,
            offsetX,
            offsetY);
    }

    /// <summary>
    /// Creates a normalized copy of the path without trimming or translating it.
    /// </summary>
    /// <remarks>
    /// Each non-degenerate dimension is scaled to a length of one.
    /// Degenerate dimensions remain unchanged.
    /// </remarks>
    /// <returns>
    /// A normalized copy of the path.
    /// </returns>
    public Path Normalize()
    {
        if (IsEmpty)
            return this;

        float width = Width;
        float height = Height;

        bool normalizeX = width != 0f && width != 1f;
        bool normalizeY = height != 0f && height != 1f;

        if (!normalizeX && !normalizeY)
            return this;

        return TransformCoordinates(
            normalizeX ? 1d / width : 1d,
            normalizeY ? 1d / height : 1d,
            0d,
            0d);
    }

    /// <summary>
    /// Creates a normalized copy of the path whose bounds range from minus one to one.
    /// </summary>
    /// <remarks>
    /// Non-degenerate dimensions are mapped to the range -1 to 1.
    /// Degenerate dimensions are centered at zero.
    /// </remarks>
    /// <returns>
    /// A signed normalized copy of the path.
    /// </returns>
    public Path NormalizeSigned()
    {
        if (IsEmpty)
            return this;

        float width = Width;
        float height = Height;

        if (Left == -1f &&
            Top == -1f &&
            width == 2f &&
            height == 2f)
        {
            return this;
        }

        double scaleX = width == 0f ? 0d : 2d / width;
        double scaleY = height == 0f ? 0d : 2d / height;
        double offsetX = width == 0f ? 0d : -1d - (double)Left * scaleX;
        double offsetY = height == 0f ? 0d : -1d - (double)Top * scaleY;

        return TransformCoordinates(
            scaleX,
            scaleY,
            offsetX,
            offsetY);
    }

    /// <summary>
    /// Creates a copy of the path centered around the coordinate origin.
    /// </summary>
    /// <returns>
    /// A new path whose bounds are centered around zero.
    /// </returns>
    public Path Center()
    {
        if (IsEmpty)
            return this;

        Point2 origin = Origin;

        if (origin.X == 0f && origin.Y == 0f)
            return this;

        return TransformCoordinates(
            1d,
            1d,
            -(double)origin.X,
            -(double)origin.Y);
    }

    /// <summary>
    /// Creates a copy of the path with the winding direction of all polygons reversed.
    /// </summary>
    /// <returns>
    /// A new path containing the polygons in reversed point order.
    /// </returns>
    public Path Reverse()
    {
        if (IsEmpty)
            return this;

        Vector2[][] polygons = ClonePolygons(_polygons);

        for (int i = 0; i < polygons.Length; i++)
            Array.Reverse(polygons[i]);

        return new Path(polygons, true);
    }

    /// <summary>
    /// Creates a copy of the path whose polygons use clockwise winding.
    /// </summary>
    /// <returns>
    /// A path whose polygon contours use clockwise winding.
    /// </returns>
    public Path EnsureClockwise()
    {
        if (IsEmpty)
            return this;

        Vector2[][]? polygons = null;

        for (int i = 0; i < _polygons.Length; i++)
        {
            if (_directions[i] != PolygonDirection.Anticlockwise)
                continue;

            polygons ??= ClonePolygons(_polygons);
            Array.Reverse(polygons[i]);
        }

        return polygons == null
            ? this
            : new Path(polygons, true);
    }

    /// <summary>
    /// Creates a copy of the path whose polygons use anticlockwise winding.
    /// </summary>
    /// <returns>
    /// A path whose polygon contours use anticlockwise winding.
    /// </returns>
    public Path EnsureAnticlockwise()
    {
        if (IsEmpty)
            return this;

        Vector2[][]? polygons = null;

        for (int i = 0; i < _polygons.Length; i++)
        {
            if (_directions[i] != PolygonDirection.Clockwise)
                continue;

            polygons ??= ClonePolygons(_polygons);
            Array.Reverse(polygons[i]);
        }

        return polygons == null
            ? this
            : new Path(polygons, true);
    }

    /// <summary>
    /// Calculates the total area of all polygons contained in the path.
    /// </summary>
    /// <returns>
    /// The total area of all polygon contours.
    /// </returns>
    public float GetArea()
    {
        double area = 0d;

        for (int i = 0; i < _polygons.Length; i++)
            area += Math.Abs(ComputeSignedPolygonArea(_polygons[i]));

        return ToFiniteSingle(area, "The total path area exceeds the supported range.");
    }

    /// <summary>
    /// Calculates the area of the specified polygon.
    /// </summary>
    /// <param name="index">The polygon index.</param>
    /// <returns>
    /// The area of the specified polygon.
    /// </returns>
    public float GetPolygonArea(int index)
    {
        ValidateIndex(index, _polygons.Length, nameof(index));

        double area = Math.Abs(ComputeSignedPolygonArea(_polygons[index]));
        return ToFiniteSingle(area, "The polygon area exceeds the supported range.");
    }

    /// <summary>
    /// Enumerates copies of the polygon point arrays contained in this path.
    /// </summary>
    /// <returns>The polygon point arrays.</returns>
    public IEnumerable<Point2[]> ToPoints()
    {
        for (int i = 0; i < _polygons.Length; i++)
            yield return ConvertToPoints(_polygons[i]);
    }

    /// <summary>
    /// Creates a read-only list of polygon contours represented by points.
    /// </summary>
    /// <returns>A read-only list containing copies of all polygon contours.</returns>
    public IReadOnlyList<IReadOnlyList<Point2>> ToPointLists()
    {
        var polygons = new IReadOnlyList<Point2>[_polygons.Length];

        for (int i = 0; i < _polygons.Length; i++)
            polygons[i] = ConvertToPoints(_polygons[i]);

        return polygons;
    }

    /// <summary>
    /// Creates a read-only list of polygon contours represented by vectors.
    /// </summary>
    /// <returns>A read-only list containing copies of all polygon contours.</returns>
    public IReadOnlyList<IReadOnlyList<Vector2>> ToVectorLists()
    {
        var polygons = new IReadOnlyList<Vector2>[_polygons.Length];

        for (int i = 0; i < _polygons.Length; i++)
            polygons[i] = (Vector2[])_polygons[i].Clone();

        return polygons;
    }

    /// <summary>
    /// Creates a read-only list of polygon contours represented by pixel points.
    /// </summary>
    /// <returns>A read-only list containing copies of all polygon contours.</returns>
    public IReadOnlyList<IReadOnlyList<PixelPoint2>> ToPixelPointLists()
    {
        var polygons = new IReadOnlyList<PixelPoint2>[_polygons.Length];

        for (int i = 0; i < _polygons.Length; i++)
            polygons[i] = ConvertToPixelPoints(_polygons[i]);

        return polygons;
    }

    /// <summary>
    /// Creates an independent clone of this path.
    /// </summary>
    /// <returns>A new path containing the same geometry.</returns>
    public Path Clone()
    {
        return new Path(
            ClonePolygons(_polygons),
            true);
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    /// <summary>
    /// Determines whether this path contains the same geometry
    /// as the specified path.
    /// </summary>
    /// <param name="other">The path to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if both paths contain identical geometry;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Path? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other == null ||
            _polygons.Length != other._polygons.Length)
        {
            return false;
        }

        for (int i = 0; i < _polygons.Length; i++)
        {
            Vector2[] a = _polygons[i];
            Vector2[] b = other._polygons[i];

            if (a.Length != b.Length)
                return false;

            for (int j = 0; j < a.Length; j++)
            {
                if (a[j] != b[j])
                    return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Path other &&
               Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Path ({_polygons.Length} polygon(s))";
    }

    /// <summary>
    /// Returns an enumerator over copies of the polygons contained in the path.
    /// </summary>
    /// <returns>An enumerator over the polygon point collections.</returns>
    public IEnumerator<IReadOnlyList<Point2>> GetEnumerator()
    {
        for (int i = 0; i < _polygons.Length; i++)
            yield return ConvertToPoints(_polygons[i]);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void InitializeGeometry()
    {
        for (int i = 0; i < _polygons.Length; i++)
        {
            _directions[i] =
                ComputePolygonDirection(_polygons[i]);

            _polygonBounds[i] =
                ComputePolygonBounds(_polygons[i]);
        }
    }

    private static Point2[] ConvertToPoints(
        Vector2[] polygon)
    {
        var points = new Point2[polygon.Length];

        for (int i = 0; i < polygon.Length; i++)
        {
            points[i] =
                new Point2(
                    polygon[i].X,
                    polygon[i].Y);
        }

        return points;
    }

    private static Vector2[][] ClonePolygons(
        Vector2[][] polygons)
    {
        var result = new Vector2[polygons.Length][];

        for (int i = 0; i < polygons.Length; i++)
        {
            result[i] =
                (Vector2[])polygons[i].Clone();
        }

        return result;
    }

    private int ComputeHashCode()
    {
        unchecked
        {
            int hash = 17;

            foreach (Vector2[] polygon in _polygons)
            {
                hash = hash * 31 + polygon.Length;

                foreach (Vector2 point in polygon)
                {
                    hash = hash * 31 + point.X.GetHashCode();
                    hash = hash * 31 + point.Y.GetHashCode();
                }
            }

            return hash;
        }
    }

    private Path TransformCoordinates(
        double scaleX,
        double scaleY,
        double offsetX,
        double offsetY)
    {
        var polygons = new Vector2[_polygons.Length][];

        for (int i = 0; i < _polygons.Length; i++)
        {
            Vector2[] source = _polygons[i];
            var transformed = new Vector2[source.Length];

            for (int j = 0; j < source.Length; j++)
            {
                double x = source[j].X * scaleX + offsetX;
                double y = source[j].Y * scaleY + offsetY;

                transformed[j] =
                    new Vector2(
                        ToFiniteSingle(
                            x,
                            "The geometry operation produced an invalid X coordinate."),
                        ToFiniteSingle(
                            y,
                            "The geometry operation produced an invalid Y coordinate."));
            }

            polygons[i] = transformed;
        }

        return new Path(polygons, true);
    }

    private static PolygonDirection ComputePolygonDirection(Vector2[] points)
    {
        double area = ComputeSignedPolygonArea(points);

        if (area < 0d)
            return PolygonDirection.Clockwise;

        if (area > 0d)
            return PolygonDirection.Anticlockwise;

        return PolygonDirection.Unknown;
    }

    private static double ComputeSignedPolygonArea(Vector2[] polygon)
    {
        double area = 0d;

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[(i + 1) % polygon.Length];

            area +=
                (double)a.X * b.Y -
                (double)b.X * a.Y;
        }

        return area * 0.5d;
    }

    private static bool IsFinite(Point2 point)
    {
        return float.IsFinite(point.X) &&
               float.IsFinite(point.Y);
    }

    private static bool IsFinite(Vector2 point)
    {
        return float.IsFinite(point.X) &&
               float.IsFinite(point.Y);
    }

    private static bool IsFinite(Box2 bounds)
    {
        return float.IsFinite(bounds.MinX) &&
               float.IsFinite(bounds.MinY) &&
               float.IsFinite(bounds.MaxX) &&
               float.IsFinite(bounds.MaxY) &&
               float.IsFinite(bounds.Width) &&
               float.IsFinite(bounds.Height);
    }

    private static void ValidateBounds(Box2 bounds)
    {
        if (!IsFinite(bounds))
        {
            throw new OverflowException(
                "Path bounds exceed the supported finite coordinate range.");
        }
    }

    private static void ValidateIndex(int index, int count, string paramName)
    {
        if ((uint)index >= (uint)count)
            throw new ArgumentOutOfRangeException(paramName);
    }

    private static float ToFiniteSingle(double value, string message)
    {
        if (!double.IsFinite(value) ||
            value < -float.MaxValue ||
            value > float.MaxValue)
        {
            throw new OverflowException(message);
        }

        return (float)value;
    }

    private static Box2 ComputePolygonBounds(
        Vector2[] polygon)
    {
        float minX = polygon[0].X;
        float minY = polygon[0].Y;
        float maxX = minX;
        float maxY = minY;

        for (int i = 1; i < polygon.Length; i++)
        {
            Vector2 point = polygon[i];

            if (point.X < minX)
                minX = point.X;

            if (point.Y < minY)
                minY = point.Y;

            if (point.X > maxX)
                maxX = point.X;

            if (point.Y > maxY)
                maxY = point.Y;
        }

        return new Box2(
            minX,
            minY,
            maxX,
            maxY);
    }

    private static Box2 ComputeBounds(
        Box2[] bounds)
    {
        if (bounds.Length == 0)
            return Box2.Zero;

        float minX = bounds[0].MinX;
        float minY = bounds[0].MinY;
        float maxX = bounds[0].MaxX;
        float maxY = bounds[0].MaxY;

        for (int i = 1; i < bounds.Length; i++)
        {
            Box2 current = bounds[i];

            if (current.MinX < minX)
                minX = current.MinX;

            if (current.MinY < minY)
                minY = current.MinY;

            if (current.MaxX > maxX)
                maxX = current.MaxX;

            if (current.MaxY > maxY)
                maxY = current.MaxY;
        }

        return new Box2(
            minX,
            minY,
            maxX,
            maxY);
    }

    private static Box2 TransformBounds(
        Box2 bounds,
        Matrix transform)
    {
        var min = bounds.Min;
        var max = bounds.Max;

        Vector2 p0 =
            Vector2.Transform(
                new Vector2(min.X, min.Y),
                transform);

        Vector2 p1 =
            Vector2.Transform(
                new Vector2(max.X, min.Y),
                transform);

        Vector2 p2 =
            Vector2.Transform(
                new Vector2(max.X, max.Y),
                transform);

        Vector2 p3 =
            Vector2.Transform(
                new Vector2(min.X, max.Y),
                transform);

        float minX =
            MathF.Min(
                MathF.Min(p0.X, p1.X),
                MathF.Min(p2.X, p3.X));

        float minY =
            MathF.Min(
                MathF.Min(p0.Y, p1.Y),
                MathF.Min(p2.Y, p3.Y));

        float maxX =
            MathF.Max(
                MathF.Max(p0.X, p1.X),
                MathF.Max(p2.X, p3.X));

        float maxY =
            MathF.Max(
                MathF.Max(p0.Y, p1.Y),
                MathF.Max(p2.Y, p3.Y));

        return new Box2(
            minX,
            minY,
            maxX,
            maxY);
    }

    private static PixelPoint2[] ConvertToPixelPoints(Vector2[] polygon)
    {
        var points = new PixelPoint2[polygon.Length];

        for (int i = 0; i < polygon.Length; i++)
        {
            points[i] =
                new PixelPoint2(
                    checked((int)polygon[i].X),
                    checked((int)polygon[i].Y));
        }

        return points;
    }
}
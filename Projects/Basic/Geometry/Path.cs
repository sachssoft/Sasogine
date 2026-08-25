using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Represents an immutable collection of polygon contours.
    /// </summary>
    /// <remarks>
    /// A <see cref="Path"/> cannot be modified after construction.
    /// Operations that change the geometry return a new <see cref="Path"/> instance.
    /// This makes the type suitable for geometry caching and dictionary keys.
    /// </remarks>
    public sealed class Path : IEnumerable<IReadOnlyList<Vector2>>, ICloneable, IEquatable<Path>
    {
        private readonly Vector2[][] _polygons;
        private readonly PolygonDirection[] _directions;
        private readonly BoundingBox2D[] _polygonBounds;
        private readonly BoundingBox2D _bounds;
        private readonly int _hashCode;

        /// <summary>
        /// Gets an empty path.
        /// </summary>
        public static Path Empty { get; } = new Path(Array.Empty<Vector2[]>());

        /// <summary>
        /// Initializes an empty path.
        /// </summary>
        public Path() : this(Array.Empty<Vector2[]>())
        {
        }

        /// <summary>
        /// Initializes a path containing one polygon.
        /// </summary>
        /// <param name="points">The polygon points.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the polygon contains fewer than three points.</exception>
        public Path(Vector2[] points) : this(new[] { points })
        {
        }

        /// <summary>
        /// Initializes a path from a collection of polygons.
        /// </summary>
        /// <param name="polygons">The polygon contours.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="polygons"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when a polygon is <see langword="null"/> or contains fewer than three points.</exception>
        public Path(IEnumerable<Vector2[]> polygons)
        {
            if (polygons is null)
                throw new ArgumentNullException(nameof(polygons));

            var list = new List<Vector2[]>();

            foreach (var polygon in polygons)
            {
                if (polygon is null)
                    throw new ArgumentException("Polygon cannot be null.", nameof(polygons));

                if (polygon.Length < 3)
                    throw new ArgumentException("Polygon must have at least 3 points.", nameof(polygons));

                list.Add((Vector2[])polygon.Clone());
            }

            _polygons = list.ToArray();
            _directions = new PolygonDirection[_polygons.Length];
            _polygonBounds = new BoundingBox2D[_polygons.Length];

            for (int i = 0; i < _polygons.Length; i++)
            {
                _directions[i] = ComputePolygonDirection(_polygons[i]);
                _polygonBounds[i] = ComputePolygonBounds(_polygons[i]);
            }

            _bounds = ComputeBounds(_polygonBounds);
            _hashCode = ComputeHashCode();
        }

        private Path(Vector2[][] polygons, bool takeOwnership)
        {
            _polygons = polygons;
            _directions = new PolygonDirection[polygons.Length];
            _polygonBounds = new BoundingBox2D[polygons.Length];

            for (int i = 0; i < polygons.Length; i++)
            {
                _directions[i] = ComputePolygonDirection(polygons[i]);
                _polygonBounds[i] = ComputePolygonBounds(polygons[i]);
            }

            _bounds = ComputeBounds(_polygonBounds);
            _hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Creates a rectangular path.
        /// </summary>
        /// <param name="x">The left coordinate.</param>
        /// <param name="y">The top coordinate.</param>
        /// <param name="width">The rectangle width.</param>
        /// <param name="height">The rectangle height.</param>
        /// <returns>A new rectangular path.</returns>
        public static Path CreateRectangle(float x, float y, float width, float height)
        {
            return new Path(new[]
            {
                new Vector2(x, y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y + height)
            });
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
        /// Gets the total width of the path bounds.
        /// </summary>
        public float Width => _bounds.Max.X - _bounds.Min.X;

        /// <summary>
        /// Gets the total height of the path bounds.
        /// </summary>
        public float Height => _bounds.Max.Y - _bounds.Min.Y;

        /// <summary>
        /// Gets the left coordinate of the path bounds.
        /// </summary>
        public float Left => _bounds.Min.X;

        /// <summary>
        /// Gets the top coordinate of the path bounds.
        /// </summary>
        public float Top => _bounds.Min.Y;

        /// <summary>
        /// Gets the right coordinate of the path bounds.
        /// </summary>
        public float Right => _bounds.Max.X;

        /// <summary>
        /// Gets the bottom coordinate of the path bounds.
        /// </summary>
        public float Bottom => _bounds.Max.Y;

        /// <summary>
        /// Gets the lower bound of the path.
        /// </summary>
        public Vector2 LowerBound => _bounds.Min;

        /// <summary>
        /// Gets the upper bound of the path.
        /// </summary>
        public Vector2 UpperBound => _bounds.Max;

        /// <summary>
        /// Gets the center point of the path bounds.
        /// </summary>
        public Vector2 Origin => new Vector2((Left + Right) * 0.5f, (Top + Bottom) * 0.5f);

        /// <summary>
        /// Gets the number of polygons contained in the path.
        /// </summary>
        /// <returns>The polygon count.</returns>
        public int GetPolygonCount() => _polygons.Length;

        /// <summary>
        /// Gets the points of a polygon.
        /// </summary>
        /// <param name="index">The polygon index.</param>
        /// <returns>The polygon points as a read-only collection.</returns>
        public IReadOnlyList<Vector2> GetPolygonPoints(int index) => _polygons[index];

        /// <summary>
        /// Gets the direction of a polygon.
        /// </summary>
        /// <param name="index">The polygon index.</param>
        /// <returns>The polygon direction.</returns>
        public PolygonDirection GetPolygonDirection(int index) => _directions[index];

        /// <summary>
        /// Gets the cached bounds of a polygon.
        /// </summary>
        /// <param name="index">The polygon index.</param>
        /// <returns>The polygon bounds.</returns>
        public BoundingBox2D GetPolygonBounds(int index) => _polygonBounds[index];

        /// <summary>
        /// Gets a point from a polygon.
        /// </summary>
        /// <param name="polygonIndex">The polygon index.</param>
        /// <param name="pointIndex">The point index.</param>
        /// <returns>The requested point.</returns>
        public Vector2 GetPoint(int polygonIndex, int pointIndex) => _polygons[polygonIndex][pointIndex];

        /// <summary>
        /// Gets the number of points in a polygon.
        /// </summary>
        /// <param name="polygonIndex">The polygon index.</param>
        /// <returns>The point count.</returns>
        public int GetPointCount(int polygonIndex) => _polygons[polygonIndex].Length;

        /// <summary>
        /// Creates a new path containing a single polygon from this path.
        /// </summary>
        /// <param name="index">The polygon index.</param>
        /// <returns>A new path containing the selected polygon.</returns>
        public Path PolygonToPath(int index)
        {
            return new Path(new[] { _polygons[index] });
        }

        /// <summary>
        /// Creates a transformed copy of the path.
        /// </summary>
        /// <param name="transform">The transformation matrix.</param>
        /// <returns>A new transformed path.</returns>
        public Path Transform(Matrix transform)
        {
            var polygons = new Vector2[_polygons.Length][];

            for (int i = 0; i < _polygons.Length; i++)
            {
                var source = _polygons[i];
                var transformed = new Vector2[source.Length];

                for (int j = 0; j < source.Length; j++)
                    transformed[j] = Vector2.Transform(source[j], transform);

                polygons[i] = transformed;
            }

            return new Path(polygons, true);
        }

        /// <summary>
        /// Creates a transformed copy of the path.
        /// </summary>
        /// <param name="transform">The point transformation function.</param>
        /// <returns>A new transformed path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transform"/> is <see langword="null"/>.</exception>
        public Path Transform(Func<Vector2, Vector2> transform)
        {
            if (transform is null)
                throw new ArgumentNullException(nameof(transform));

            var polygons = new Vector2[_polygons.Length][];

            for (int i = 0; i < _polygons.Length; i++)
            {
                var source = _polygons[i];
                var transformed = new Vector2[source.Length];

                for (int j = 0; j < source.Length; j++)
                    transformed[j] = transform(source[j]);

                polygons[i] = transformed;
            }

            return new Path(polygons, true);
        }

        /// <summary>
        /// Determines whether a point is inside a polygon.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="polygonIndex">The polygon index.</param>
        /// <param name="transform">The transformation applied to the polygon.</param>
        /// <returns><see langword="true"/> when the point is inside the polygon; otherwise, <see langword="false"/>.</returns>
        public bool IsPointInPolygon(Vector2 point, int polygonIndex, Matrix transform)
        {
            var polygon = _polygons[polygonIndex];
            var bounds = TransformBounds(_polygonBounds[polygonIndex], transform);

            if (point.X < bounds.Min.X || point.X > bounds.Max.X ||
                point.Y < bounds.Min.Y || point.Y > bounds.Max.Y)
                return false;

            float angleSum = 0f;

            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 a = Vector2.Transform(polygon[i], transform);
                Vector2 b = Vector2.Transform(polygon[(i + 1) % polygon.Length], transform);
                Vector2 pa = a - point;
                Vector2 pb = b - point;

                angleSum += MathF.Atan2(pa.X * pb.Y - pa.Y * pb.X, Vector2.Dot(pa, pb));
            }

            return MathF.Abs(angleSum) > 0.0001f;
        }

        /// <summary>
        /// Gets all polygons contained in this path.
        /// </summary>
        /// <returns>The polygon point arrays.</returns>
        public IEnumerable<Vector2[]> ToPoints()
        {
            for (int i = 0; i < _polygons.Length; i++)
                yield return _polygons[i];
        }

        /// <summary>
        /// Creates an independent clone of this path.
        /// </summary>
        /// <returns>A new path containing the same geometry.</returns>
        public Path Clone()
        {
            var polygons = new Vector2[_polygons.Length][];

            for (int i = 0; i < _polygons.Length; i++)
                polygons[i] = (Vector2[])_polygons[i].Clone();

            return new Path(polygons, true);
        }

        /// <summary>
        /// Creates an independent clone of this path.
        /// </summary>
        /// <returns>A cloned <see cref="Path"/>.</returns>
        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Determines whether this path is equal to another path.
        /// </summary>
        /// <param name="other">The path to compare.</param>
        /// <returns><see langword="true"/> when both paths contain identical geometry; otherwise, <see langword="false"/>.</returns>
        public bool Equals(Path? other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null || _polygons.Length != other._polygons.Length)
                return false;

            for (int i = 0; i < _polygons.Length; i++)
            {
                var a = _polygons[i];
                var b = other._polygons[i];

                if (a.Length != b.Length)
                    return false;

                for (int j = 0; j < a.Length; j++)
                    if (a[j] != b[j])
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this path is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><see langword="true"/> when the object represents identical path geometry; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is Path other && Equals(other);

        /// <summary>
        /// Returns the hash code of the path geometry.
        /// </summary>
        /// <returns>The geometry hash code.</returns>
        public override int GetHashCode() => _hashCode;

        /// <summary>
        /// Returns a string representation of the path.
        /// </summary>
        /// <returns>A string describing the path.</returns>
        public override string ToString() => $"Path ({_polygons.Length} polygon(s))";

        /// <summary>
        /// Returns an enumerator over the polygons.
        /// </summary>
        /// <returns>An enumerator containing the polygon point collections.</returns>
        public IEnumerator<IReadOnlyList<Vector2>> GetEnumerator()
        {
            for (int i = 0; i < _polygons.Length; i++)
                yield return _polygons[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int ComputeHashCode()
        {
            unchecked
            {
                int hash = 17;

                foreach (var polygon in _polygons)
                {
                    hash = hash * 31 + polygon.Length;

                    foreach (var point in polygon)
                    {
                        hash = hash * 31 + point.X.GetHashCode();
                        hash = hash * 31 + point.Y.GetHashCode();
                    }
                }

                return hash;
            }
        }

        private static PolygonDirection ComputePolygonDirection(Vector2[] points)
        {
            float area = 0f;

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % points.Length];
                area += a.X * b.Y - b.X * a.Y;
            }

            if (area < 0f)
                return PolygonDirection.Clockwise;

            if (area > 0f)
                return PolygonDirection.Anticlockwise;

            return PolygonDirection.Unknown;
        }

        private static BoundingBox2D ComputePolygonBounds(Vector2[] polygon)
        {
            float left = polygon[0].X, top = polygon[0].Y;
            float right = left, bottom = top;

            for (int i = 1; i < polygon.Length; i++)
            {
                Vector2 p = polygon[i];

                if (p.X < left) left = p.X;
                if (p.Y < top) top = p.Y;
                if (p.X > right) right = p.X;
                if (p.Y > bottom) bottom = p.Y;
            }

            return new BoundingBox2D(new Vector2(left, top), new Vector2(right, bottom));
        }

        private static BoundingBox2D ComputeBounds(BoundingBox2D[] bounds)
        {
            if (bounds.Length == 0)
                return BoundingBox2D.Empty;

            float left = bounds[0].Min.X, top = bounds[0].Min.Y;
            float right = bounds[0].Max.X, bottom = bounds[0].Max.Y;

            for (int i = 1; i < bounds.Length; i++)
            {
                var bound = bounds[i];

                if (bound.Min.X < left) left = bound.Min.X;
                if (bound.Min.Y < top) top = bound.Min.Y;
                if (bound.Max.X > right) right = bound.Max.X;
                if (bound.Max.Y > bottom) bottom = bound.Max.Y;
            }

            return new BoundingBox2D(new Vector2(left, top), new Vector2(right, bottom));
        }

        private static BoundingBox2D TransformBounds(BoundingBox2D bounds, Matrix transform)
        {
            Vector2 min = bounds.Min;
            Vector2 max = bounds.Max;

            Vector2 p0 = Vector2.Transform(new Vector2(min.X, min.Y), transform);
            Vector2 p1 = Vector2.Transform(new Vector2(max.X, min.Y), transform);
            Vector2 p2 = Vector2.Transform(new Vector2(max.X, max.Y), transform);
            Vector2 p3 = Vector2.Transform(new Vector2(min.X, max.Y), transform);

            float left = MathF.Min(MathF.Min(p0.X, p1.X), MathF.Min(p2.X, p3.X));
            float top = MathF.Min(MathF.Min(p0.Y, p1.Y), MathF.Min(p2.Y, p3.Y));
            float right = MathF.Max(MathF.Max(p0.X, p1.X), MathF.Max(p2.X, p3.X));
            float bottom = MathF.Max(MathF.Max(p0.Y, p1.Y), MathF.Max(p2.Y, p3.Y));

            return new BoundingBox2D(new Vector2(left, top), new Vector2(right, bottom));
        }
    }
}
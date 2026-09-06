using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Represents an immutable collection of <see cref="Path"/> instances.
    /// </summary>
    /// <remarks>
    /// <see cref="PathCollection"/> precomputes its bounds and centroid during construction,
    /// making it suitable for repeated geometry queries and caching.
    /// </remarks>
    public sealed class PathCollection : IReadOnlyList<Path>, ICloneable
    {
        private readonly Path[] _paths;
        private readonly int _hashCode;

        /// <summary>
        /// Initializes an empty <see cref="PathCollection"/>.
        /// </summary>
        public PathCollection()
        {
            _paths = Array.Empty<Path>();
            LowerBound = Point2.Zero;
            UpperBound = Point2.Zero;
            Centroid = Point2.Zero;
            _hashCode = ComputeHashCode(_paths);
        }

        /// <summary>
        /// Initializes a <see cref="PathCollection"/> from the specified paths.
        /// </summary>
        /// <param name="paths">The paths to include in the collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="paths"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the collection contains a <see langword="null"/> path.
        /// </exception>
        public PathCollection(IEnumerable<Path> paths)
        {
            if (paths is null)
                throw new ArgumentNullException(nameof(paths));

            _paths = paths is Path[] array
                ? (Path[])array.Clone()
                : new List<Path>(paths).ToArray();

            for (int i = 0; i < _paths.Length; i++)
            {
                if (_paths[i] is null)
                {
                    throw new ArgumentException(
                        "PathCollection cannot contain null paths.",
                        nameof(paths));
                }
            }

            (LowerBound, UpperBound) = ComputeBounds(_paths);
            Centroid = ComputeCentroid(_paths);
            _hashCode = ComputeHashCode(_paths);
        }

        /// <summary>
        /// Initializes a <see cref="PathCollection"/> from the specified paths.
        /// </summary>
        /// <param name="pathArray">The paths to include in the collection.</param>
        public PathCollection(params Path[] pathArray)
            : this((IEnumerable<Path>)(
                pathArray ??
                throw new ArgumentNullException(nameof(pathArray))))
        {
        }

        /// <summary>
        /// Gets the geometric centroid of the collection.
        /// </summary>
        public Point2 Centroid { get; }

        /// <summary>
        /// Gets the minimum bounds of the collection.
        /// </summary>
        public Point2 LowerBound { get; }

        /// <summary>
        /// Gets the maximum bounds of the collection.
        /// </summary>
        public Point2 UpperBound { get; }

        /// <summary>
        /// Gets the width of the collection bounds.
        /// </summary>
        public float Width => UpperBound.X - LowerBound.X;

        /// <summary>
        /// Gets the height of the collection bounds.
        /// </summary>
        public float Height => UpperBound.Y - LowerBound.Y;

        /// <summary>
        /// Gets the path at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the path.</param>
        /// <returns>The path at the specified index.</returns>
        public Path this[int index] => _paths[index];

        /// <summary>
        /// Gets the number of paths in the collection.
        /// </summary>
        public int Count => _paths.Length;

        /// <summary>
        /// Returns an enumerator that iterates through the paths.
        /// </summary>
        /// <returns>An enumerator for the paths.</returns>
        public IEnumerator<Path> GetEnumerator()
        {
            return ((IEnumerable<Path>)_paths).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        /// <summary>
        /// Returns all polygon point arrays contained in all paths.
        /// </summary>
        /// <returns>An enumerable collection of polygon point arrays.</returns>
        public IEnumerable<Point2[]> ToPoints()
        {
            for (int i = 0; i < _paths.Length; i++)
            {
                foreach (var points in _paths[i].ToPoints())
                    yield return points;
            }
        }

        /// <summary>
        /// Creates a transformed copy of this collection.
        /// </summary>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>A new transformed path collection.</returns>
        public PathCollection Transform(Matrix matrix)
        {
            var paths = new Path[_paths.Length];

            for (int i = 0; i < _paths.Length; i++)
                paths[i] = _paths[i].Transform(matrix);

            return new PathCollection(paths);
        }

        /// <summary>
        /// Creates a transformed copy of this collection using the specified point transformation.
        /// </summary>
        /// <param name="transform">The transformation function.</param>
        /// <returns>A new transformed path collection.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transform"/> is <see langword="null"/>.
        /// </exception>
        public PathCollection Transform(
            Func<Point2, Point2> transform)
        {
            if (transform is null)
                throw new ArgumentNullException(nameof(transform));

            var paths = new Path[_paths.Length];

            for (int i = 0; i < _paths.Length; i++)
                paths[i] = _paths[i].Transform(transform);

            return new PathCollection(paths);
        }

        /// <summary>
        /// Creates an independent clone of this collection.
        /// </summary>
        /// <returns>A new path collection containing cloned paths.</returns>
        public PathCollection Clone()
        {
            var paths = new Path[_paths.Length];

            for (int i = 0; i < _paths.Length; i++)
                paths[i] = _paths[i].Clone();

            return new PathCollection(paths);
        }

        /// <summary>
        /// Creates an independent clone of this collection.
        /// </summary>
        /// <returns>An independent clone of this collection.</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Determines whether this collection is equal to another object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// <see langword="true"/> when the collections contain equal paths in the same order;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not PathCollection other ||
                _paths.Length != other._paths.Length)
            {
                return false;
            }

            for (int i = 0; i < _paths.Length; i++)
            {
                if (!_paths[i].Equals(other._paths[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code for this collection.
        /// </summary>
        /// <returns>The hash code of the collection.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static (Point2 LowerBound, Point2 UpperBound) ComputeBounds(
            IReadOnlyList<Path> paths)
        {
            if (paths.Count == 0)
                return (Point2.Zero, Point2.Zero);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool hasBounds = false;

            for (int i = 0; i < paths.Count; i++)
            {
                Path path = paths[i];

                if (path.IsEmpty)
                    continue;

                minX = MathF.Min(minX, path.Left);
                minY = MathF.Min(minY, path.Top);
                maxX = MathF.Max(maxX, path.Right);
                maxY = MathF.Max(maxY, path.Bottom);

                hasBounds = true;
            }

            if (!hasBounds)
                return (Point2.Zero, Point2.Zero);

            return (
                new Point2(minX, minY),
                new Point2(maxX, maxY));
        }

        private static Point2 ComputeCentroid(
            IReadOnlyList<Path> paths)
        {
            if (paths.Count == 0)
                return Point2.Zero;

            Vector2 accumulatedCentroid = Vector2.Zero;
            float totalArea = 0f;

            for (int pathIndex = 0;
                 pathIndex < paths.Count;
                 pathIndex++)
            {
                Path path = paths[pathIndex];

                for (int polygonIndex = 0;
                     polygonIndex < path.GetPolygonCount();
                     polygonIndex++)
                {
                    IReadOnlyList<Point2> polygon =
                        path.GetPolygonPoints(polygonIndex);

                    if (polygon.Count < 3)
                        continue;

                    float area = 0f;
                    Vector2 centroid = Vector2.Zero;

                    for (int i = 0; i < polygon.Count; i++)
                    {
                        Point2 current = polygon[i];
                        Point2 next =
                            polygon[(i + 1) % polygon.Count];

                        float cross =
                            current.X * next.Y -
                            next.X * current.Y;

                        area += cross;

                        centroid.X +=
                            (current.X + next.X) * cross;

                        centroid.Y +=
                            (current.Y + next.Y) * cross;
                    }

                    area *= 0.5f;

                    if (MathF.Abs(area) <= 0.000001f)
                        continue;

                    centroid /= 6f * area;

                    accumulatedCentroid +=
                        centroid * area;

                    totalArea += area;
                }
            }

            if (totalArea == 0f)
                return Point2.Zero;

            return accumulatedCentroid / totalArea;
        }

        private static int ComputeHashCode(
            IReadOnlyList<Path> paths)
        {
            var hash = new HashCode();

            hash.Add(paths.Count);

            for (int i = 0; i < paths.Count; i++)
                hash.Add(paths[i]);

            return hash.ToHashCode();
        }
    }
}
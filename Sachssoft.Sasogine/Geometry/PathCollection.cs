using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Geometry;

public sealed class PathCollection : IReadOnlyList<Path>
{
    private readonly List<Path> _paths;
    private Vector2 _centroid;
    private bool _centroidComputed;

    public PathCollection()
    {
        _paths = new List<Path>();
        LowerBound = Vector2.Zero;
        UpperBound = Vector2.Zero;
        _centroid = Vector2.Zero;
        _centroidComputed = false;
    }

    public PathCollection(IEnumerable<Path> paths)
    {
        _paths = new List<Path>(paths ?? Enumerable.Empty<Path>());
        ComputeBounds();
        _centroidComputed = false;
    }

    private void ComputeBounds()
    {
        if (_paths.Count == 0)
        {
            LowerBound = Vector2.Zero;
            UpperBound = Vector2.Zero;
            return;
        }

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var path in _paths)
        {
            if (path.IsEmpty())
                continue;

            minX = MathF.Min(minX, path.Left);
            minY = MathF.Min(minY, path.Top);
            maxX = MathF.Max(maxX, path.Right);
            maxY = MathF.Max(maxY, path.Bottom);
        }

        LowerBound = new Vector2(minX, minY);
        UpperBound = new Vector2(maxX, maxY);
    }

    private void ComputeCentroid()
    {
        if (_paths.Count == 0)
        {
            _centroid = Vector2.Zero;
            _centroidComputed = true;
            return;
        }

        Vector2 accumulatedCentroid = Vector2.Zero;
        float totalArea = 0f;

        foreach (var path in _paths)
        {
            int polygonCount = path.GetPolygonCount();
            for (int i = 0; i < polygonCount; i++)
            {
                var polygon = path.GetPolygon(i).ToArray();
                if (polygon.Length < 3)
                    continue;

                float area = 0f;
                Vector2 centroid = Vector2.Zero;

                for (int j = 0; j < polygon.Length; j++)
                {
                    Vector2 current = polygon[j];
                    Vector2 next = polygon[(j + 1) % polygon.Length];

                    float cross = current.X * next.Y - next.X * current.Y;
                    area += cross;
                    centroid += (current + next) * cross;
                }

                area *= 0.5f;
                if (MathF.Abs(area) < 1e-6f)
                    continue;

                centroid /= (6f * area);

                accumulatedCentroid += centroid * area;
                totalArea += area;
            }
        }

        _centroid = totalArea != 0f ? accumulatedCentroid / totalArea : Vector2.Zero;
        _centroidComputed = true;
    }

    public Vector2 Centroid
    {
        get
        {
            if (!_centroidComputed)
                ComputeCentroid();
            return _centroid;
        }
    }

    public Vector2 LowerBound { get; private set; }
    public Vector2 UpperBound { get; private set; }

    public float Width => UpperBound.X - LowerBound.X;
    public float Height => UpperBound.Y - LowerBound.Y;

    public Path this[int index] => _paths[index];
    public int Count => _paths.Count;

    public IEnumerator<Path> GetEnumerator() => _paths.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public PathCollection Transform(Matrix matrix)
    {
        var transformedPaths = _paths.Select(p => p.Transform(matrix));
        return new PathCollection(transformedPaths);
    }
}

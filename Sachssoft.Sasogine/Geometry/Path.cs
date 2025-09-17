using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Geometry
{
    public class Path
    {
        private readonly List<List<Vector2>> _points;
        private readonly List<PolygonDirection> _directions;

        private float? _width, _height, _left, _top, _right, _bottom;

        public Path()
        {
            _points = new List<List<Vector2>>();
            _directions = new List<PolygonDirection>();
        }

        public Path(IEnumerable<Vector2[]> polygons) : this()
        {
            foreach (var poly in polygons)
                AddPolygon(poly);
        }

        public Path(Vector2[] points) : this()
        {
            AddPolygon(points);
        }

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

        public static Path Empty => new Path();

        public float Width => _width ?? ComputeSize().Width;
        public float Height => _height ?? ComputeSize().Height;
        public float Left => _left ?? ComputeSize().Left;
        public float Top => _top ?? ComputeSize().Top;
        public float Right => _right ?? ComputeSize().Right;
        public float Bottom => _bottom ?? ComputeSize().Bottom;

        public Vector2 LowerBound => new Vector2(Left, Top);
        public Vector2 UpperBound => new Vector2(Right, Bottom);
        public Vector2 Origin => new Vector2(Left + Width / 2f, Top + Height / 2f);

        public bool IsEmpty() => _points.Count == 0;

        public Path Clone()
        {
            var clone = new Path();
            foreach (var poly in _points)
                clone.AddPolygon(poly.ToArray());
            return clone;
        }

        public Path Transform(Matrix transform)
        {
            var transformed = new Path();
            foreach (var poly in _points)
            {
                var transformedPoly = new List<Vector2>(poly.Count);
                for (int i = 0; i < poly.Count; i++)
                    transformedPoly.Add(Vector2.Transform(poly[i], transform));

                transformed._points.Add(transformedPoly);
                transformed._directions.Add(ComputePolygonDirection(transformedPoly.ToArray()));
            }
            return transformed;
        }

        public void AddPolygon(Vector2[] points)
        {
            if (points == null || points.Length < 3)
                throw new ArgumentException("Polygon must have at least 3 points.", nameof(points));

            _points.Add(new List<Vector2>(points));
            _directions.Add(ComputePolygonDirection(points));
            ResetCache();
        }

        public void RemovePolygon(int index)
        {
            _points.RemoveAt(index);
            _directions.RemoveAt(index);
            ResetCache();
        }

        public int GetPolygonCount() => _points.Count;
        public IReadOnlyList<Vector2> GetPolygon(int index) => _points[index];
        public Path PolygonToPath(int index) => new Path(_points[index].ToArray());
        public PolygonDirection GetPolygonDirection(int index) => _directions[index];

        private PolygonDirection ComputePolygonDirection(Vector2[] points)
        {
            float area = 0f;
            int n = points.Length;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % n];
                area += (b.X - a.X) * (b.Y + a.Y) / 2f;
            }

            if (area < 0) return PolygonDirection.Clockwise;
            if (area > 0) return PolygonDirection.Anticlockwise;
            return PolygonDirection.Unknown;
        }

        public bool IsPointInPolygon(Vector2 point, int polygonIndex, Matrix transform)
        {
            var polygon = _points[polygonIndex];

            // BoundingBox-Vorfilter
            var bounds = GetPolygonBounds(polygonIndex);
            if (point.X < bounds[0].X || point.X > bounds[1].X ||
                point.Y < bounds[0].Y || point.Y > bounds[1].Y)
                return false;

            int n = polygon.Count;
            float angleSum = 0f;

            Vector2 TransformPoint(Vector2 v) => Vector2.Transform(v, transform);

            for (int i = 0; i < n; i++)
            {
                Vector2 a = TransformPoint(polygon[i]);
                Vector2 b = TransformPoint(polygon[(i + 1) % n]);
                Vector2 pa = a - point;
                Vector2 pb = b - point;

                float cross = pa.X * pb.Y - pa.Y * pb.X;
                float dot = Vector2.Dot(pa, pb);
                angleSum += MathF.Atan2(cross, dot);
            }

            return MathF.Abs(angleSum) > 0.0001f;
        }

        public Vector2 GetPoint(int polygonIndex, int pointIndex) => _points[polygonIndex][pointIndex];
        public int GetPointCount(int polygonIndex) => _points[polygonIndex].Count;

        private void ResetCache()
        {
            _width = _height = _left = _top = _right = _bottom = null;
        }

        public Vector2[] GetPolygonBounds(int index)
        {
            float left = float.MaxValue, top = float.MaxValue;
            float right = float.MinValue, bottom = float.MinValue;

            foreach (var p in _points[index])
            {
                left = Math.Min(left, p.X);
                top = Math.Min(top, p.Y);
                right = Math.Max(right, p.X);
                bottom = Math.Max(bottom, p.Y);
            }

            return new[] { new Vector2(left, top), new Vector2(right, bottom) };
        }

        private (float Left, float Top, float Right, float Bottom, float Width, float Height) ComputeSize()
        {
            float left = float.MaxValue, top = float.MaxValue;
            float right = float.MinValue, bottom = float.MinValue;

            foreach (var poly in _points)
            {
                foreach (var p in poly)
                {
                    left = Math.Min(left, p.X);
                    top = Math.Min(top, p.Y);
                    right = Math.Max(right, p.X);
                    bottom = Math.Max(bottom, p.Y);
                }
            }

            if (_points.Count == 0)
                left = top = right = bottom = 0f;

            _left = left;
            _top = top;
            _right = right;
            _bottom = bottom;
            _width = right - left;
            _height = bottom - top;

            return (_left.Value, _top.Value, _right.Value, _bottom.Value, _width.Value, _height.Value);
        }
    }
}

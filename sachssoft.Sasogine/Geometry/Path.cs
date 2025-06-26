/*
 * GolfVector
 * © 2022 Tobias Sachs | All rights reserved.
 * Effective 2022-11-04
 * -----------------------------------------------------------
 * Own shape path for golf
 * -----------------------------------------------------------
 * Moved To Sasogine
 * Updated 2025-05-24
 */

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sachssoft.Sasogine.Geometry;

public struct Path
{
    private List<List<Vector2>> _points;
    private List<PolygonDirection> _directions;
    private float? _width, _height, _left, _top, _right, _bottom;

    public Path()
    {
        _points = new List<List<Vector2>>();
        _directions = new List<PolygonDirection>();
    }

    public Path(Vector2[][] polygons) : this()
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

    public bool IsEmpty() => _points.Count == 0;

    public Vector2 LowerBound => new Vector2(Left, Top);
    public Vector2 UpperBound => new Vector2(Right, Bottom);
    public Vector2 Origin => new Vector2(Left + Width / 2f, Top + Height / 2f);

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
            var transformedPoly = new List<Vector2>();
            foreach (var point in poly)
                transformedPoly.Add(Vector2.Transform(point, transform));

            transformed._points.Add(transformedPoly);
            transformed._directions.Add(ComputePolygonDirection(transformedPoly.ToArray()));
        }
        return transformed;
    }

    public void AddPolygon(Vector2[] points)
    {
        _points ??= new List<List<Vector2>>();
        _points.Add(new List<Vector2>(points));

        _directions ??= new List<PolygonDirection>();
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
    public IEnumerable<Vector2> GetPolygon(int index) => _points[index];
    public Path PolygonToPath(int index) => new Path(_points[index].ToArray());
    public PolygonDirection GetPolygonDirection(int index) => _directions[index];

    private PolygonDirection ComputePolygonDirection(Vector2[] points)
    {
        int num = points.Length;
        float area = 0f;

        for (int i = 0; i < num; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % num];
            area += (b.X - a.X) * (b.Y + a.Y) / 2f;
        }

        if (area < 0) return PolygonDirection.Clockwise;
        if (area > 0) return PolygonDirection.Anticlockwise;
        return PolygonDirection.Unknown;
    }

    public bool IsPointInPolygon(Vector2 point, int index, Matrix transform)
    {
        var points = _points[index].Select(p => Vector2.Transform(p, transform)).ToList();
        int n = points.Count;

        float get_angle(Vector2 a, Vector2 b, Vector2 c)
        {
            var abx = a.X - b.X;
            var aby = a.Y - b.Y;
            var cbx = c.X - b.X;
            var cby = c.Y - b.Y;

            var dot = abx * cbx + aby * cby;
            var cross = abx * cby - aby * cbx;

            return MathF.Atan2(cross, dot);
        }

        float angle = get_angle(points[n - 1], point, points[0]);
        for (int i = 0; i < n - 1; i++)
            angle += get_angle(points[i], point, points[i + 1]);

        return MathF.Abs(angle) > 0.0001f;
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

        foreach (var point in _points[index])
        {
            left = Math.Min(left, point.X);
            top = Math.Min(top, point.Y);
            right = Math.Max(right, point.X);
            bottom = Math.Max(bottom, point.Y);
        }

        return new[]
        {
            new Vector2(left, top),
            new Vector2(right, bottom)
        };
    }

    private (float Left, float Top, float Right, float Bottom, float Width, float Height) ComputeSize()
    {
        float left = float.MaxValue, top = float.MaxValue;
        float right = float.MinValue, bottom = float.MinValue;

        foreach (var poly in _points)
        {
            foreach (var point in poly)
            {
                left = Math.Min(left, point.X);
                top = Math.Min(top, point.Y);
                right = Math.Max(right, point.X);
                bottom = Math.Max(bottom, point.Y);
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

        return (left, top, right, bottom, _width.Value, _height.Value);
    }
}
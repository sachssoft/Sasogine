using global::sachssoft.Sasogine.Tiling;
using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Editor.Tiling.Tools;
using sachssoft.Sasogine.Geometry;
using System;

namespace sachssoft.Sasogine.Editor.Tiling.Tools;

public sealed class EditorToolTriangleRegion : IEditorToolShapeRegion, IEditorToolShapeWithTransform
{
    private readonly Scope _region;

    private Vector2[] _originalPoints;
    private Vector2[] _transformedPoints;

    private RotationFacing _rotation = RotationFacing.Up;
    private FlipMode _flip = FlipMode.None;

    public RotationFacing Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            UpdateTransformedPoints();
        }
    }

    public FlipMode Flip
    {
        get => _flip;
        set
        {
            _flip = value;
            UpdateTransformedPoints();
        }
    }

    public EditorToolTriangleRegion(Scope region)
    {
        _region = region;

        // Ursprüngliche Eckpunkte des Dreiecks im Scope (unten links, unten rechts, oben links)
        _originalPoints = new Vector2[]
        {
            new Vector2(region.Lower.X, region.Lower.Y), // p1
            new Vector2(region.Upper.X, region.Lower.Y), // p2
            new Vector2(region.Lower.X, region.Upper.Y)  // p3
        };

        _transformedPoints = new Vector2[3];
        UpdateTransformedPoints();
    }

    private void UpdateTransformedPoints()
    {
        // Mitte des Scopes als Drehpunkt
        Vector2 center = new Vector2(
            (_region.Lower.X + _region.Upper.X) / 2f,
            (_region.Lower.Y + _region.Upper.Y) / 2f);

        for (int i = 0; i < 3; i++)
        {
            // Relativ zum Zentrum
            Vector2 p = _originalPoints[i] - center;

            // Flip anwenden
            p = ApplyFlip(p, _flip);

            // Rotation anwenden (90° Schritte)
            p = ApplyRotation(p, _rotation);

            // Zurück zur Weltposition
            _transformedPoints[i] = p + center;
        }
    }

    private static Vector2 ApplyFlip(Vector2 p, FlipMode flip)
    {
        switch (flip)
        {
            case FlipMode.Horizontal:
                return new Vector2(-p.X, p.Y);
            case FlipMode.Vertical:
                return new Vector2(p.X, -p.Y);
            case FlipMode.BothAxes:
                return new Vector2(-p.X, -p.Y);
            default:
                return p;
        }
    }

    private static Vector2 ApplyRotation(Vector2 p, RotationFacing rotation)
    {
        // Rotation um das Zentrum im 90°-Raster
        switch (rotation)
        {
            case RotationFacing.Up:
                return p;
            case RotationFacing.Left:
                return new Vector2(-p.Y, p.X);
            case RotationFacing.Down:
                return new Vector2(-p.X, -p.Y);
            case RotationFacing.Right:
                return new Vector2(p.Y, -p.X);
            default:
                return p;
        }
    }

    public bool ContainsFill(Coordinate coordinate, Scope region)
    {
        Vector2 p = new Vector2(coordinate.X, coordinate.Y);
        return PointInTriangle(p, _transformedPoints[0], _transformedPoints[1], _transformedPoints[2]);
    }

    public bool ContainsOutline(Coordinate coordinate, Scope region, int thickness)
    {
        Vector2 p = new Vector2(coordinate.X, coordinate.Y);
        float halfThickness = thickness * 0.5f;

        return
            DistanceToSegment(p, _transformedPoints[0], _transformedPoints[1]) <= halfThickness ||
            DistanceToSegment(p, _transformedPoints[1], _transformedPoints[2]) <= halfThickness ||
            DistanceToSegment(p, _transformedPoints[2], _transformedPoints[0]) <= halfThickness;
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = TriangleArea(a, b, c);
        float area1 = TriangleArea(p, b, c);
        float area2 = TriangleArea(a, p, c);
        float area3 = TriangleArea(a, b, p);

        return Math.Abs(area - (area1 + area2 + area3)) < 0.01f;
    }

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return MathF.Abs(
            (a.X * (b.Y - c.Y) +
             b.X * (c.Y - a.Y) +
             c.X * (a.Y - b.Y)) * 0.5f);
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;

        float abLenSquared = ab.LengthSquared();
        if (abLenSquared == 0f)
            return Vector2.Distance(p, a);

        float t = Vector2.Dot(ap, ab) / abLenSquared;
        t = Math.Clamp(t, 0f, 1f);

        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
}

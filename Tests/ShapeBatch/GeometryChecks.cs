using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Graphics.Rendering;
using Sachssoft.Engine.Graphics.Rendering.Batches;

namespace Sachssoft.Sasogine.ShapeBatchTests;

/// <summary>Checks geometric area and single coverage independently of graphics resources.</summary>
internal static class GeometryChecks
{
    private static int _checks;

    /// <summary>Runs deterministic regression and randomized geometry checks.</summary>
    internal static void Run()
    {
        RectangleChecks();
        StrokeChecks();
        InvalidChecks();
        Console.WriteLine($"CPU PASS: {_checks} checks.");
    }

    private static void RectangleChecks()
    {
        Span<Vector2> points = stackalloc Vector2[ShapeStrokeGeometry.RectangleCapacity];
        var bounds = new Bounds2(0, 0, 100, 50);
        foreach (CornerType corner in Enum.GetValues<CornerType>())
        {
            int count = ShapeStrokeGeometry.RectangleContour(bounds, corner, 10, points);
            double area = Area(points[..count]);
            double expected = corner switch
            {
                CornerType.None => 5000,
                CornerType.Bevel => 4800,
                _ => 5000 - (4 - Math.PI) * 100
            };
            Check(Math.Abs(area - expected) < 3, $"{corner} area {area} vs {expected}");
            Check(Convex(points[..count]), $"{corner} convex perimeter");
            count = ShapeStrokeGeometry.RectangleContour(bounds, corner, 10000, points);
            Check(Convex(points[..count]) && Area(points[..count]) > 0, $"{corner} clamps oversized corners");
            count = ShapeStrokeGeometry.RectangleContour(new Bounds2(0, 0, 50, 50), corner, 25, points);
            Check(Convex(points[..count]) && Area(points[..count]) > 0, $"{corner} square/circle maximum radius");
        }
        Check(ShapeStrokeGeometry.RectangleContour(new Bounds2(0, 0, 0, 50), CornerType.Rounded, 5, points) == 0, "zero width no-op");
        Span<Vector2> vertices = stackalloc Vector2[8];
        Span<int> indices = stackalloc int[24];
        foreach (float thickness in new[] { 0f, 1f, 4f, 49f, 50f, 100f })
        {
            int n = ShapeStrokeGeometry.RectangleRing(bounds, thickness, vertices, indices, out int count);
            var mesh = new PolygonTriangulationResult(vertices[..count].ToArray(), indices[..n].ToArray());
            double expected = thickness == 0 ? 0 : (100 + thickness) * (50 + thickness) -
                Math.Max(0, 100 - thickness) * Math.Max(0, 50 - thickness);
            Check(Math.Abs(MeshArea(mesh) - expected) < 0.01, $"ring thickness {thickness} area");
            SingleCoverage(mesh, 500, 400 + (int)thickness);
            if (thickness < 50 && thickness > 0) Check(Coverage(mesh, new Vector2(50.123f, 25.234f)) == 0, "ring center remains empty");
        }
        for (int i = 0; i < 20; i++) ShapeStrokeGeometry.RectangleRing(bounds, 4, vertices, indices, out _);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) ShapeStrokeGeometry.RectangleRing(bounds, 4, vertices, indices, out _);
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "sharp rectangle ring has no steady-state allocations");
    }

    private static void StrokeChecks()
    {
        Vector2[] square = [new(0, 0), new(100, 0), new(100, 50), new(0, 50)];
        foreach (LineJoin join in Enum.GetValues<LineJoin>())
        {
            var mesh = Stroke([square], 4, join, LineCap.Butt, true);
            Check(mesh.Indices.Count > 0, $"{join} rectangle produces triangles");
            Check(Coverage(mesh, new(50.123f, 25.234f)) == 0, $"{join} keeps rectangle interior empty");
            SingleCoverage(mesh, 1500, 42);
            if (join == LineJoin.Miter) Check(Math.Abs(MeshArea(mesh) - 1200) < .01, "generic miter agrees with fast rectangle area");
            var reverse = square.Reverse().ToArray();
            Check(Math.Abs(MeshArea(mesh) - MeshArea(Stroke([reverse], 4, join, LineCap.Butt, true))) < .01, "winding independent stroke");
            foreach (LineCap cap in Enum.GetValues<LineCap>())
            {
                var line = Stroke([new Vector2[] { new(0, 0), new(20, 0), new(20, 20) }], 4, join, cap, false);
                SingleCoverage(line, 1000, 77);
                if (join == LineJoin.Miter && cap == LineCap.Butt) Check(Math.Abs(MeshArea(line) - 160) < .01, "L join area without overdraw");
            }
        }
        Vector2[] duplicates = [new(0, 0), new(0, 0), new(100, 0), new(100, 50), new(0, 50), new(0, 0)];
        Check(Math.Abs(MeshArea(Stroke([duplicates], 4, LineJoin.Miter, LineCap.Butt, true)) - 1200) < .01, "duplicate vertices and explicit closure");
        var crossing = Stroke([new Vector2[] { new(0, 0), new(30, 30), new(0, 30), new(30, 0) }], 6, LineJoin.Round, LineCap.Round, false);
        SingleCoverage(crossing, 3000, 101);
        Check(Coverage(crossing, new(15.123f, 15.234f)) == 1, "self-intersection is covered once");
        var overlapping = Stroke([square, square.Reverse().ToArray()], 4, LineJoin.Miter, LineCap.Butt, true);
        Check(Math.Abs(MeshArea(overlapping) - 1200) < .01, "duplicate contours union rather than doubling alpha");
        SingleCoverage(overlapping, 1000, 99);
        var hairpin = Stroke([new Vector2[] { new(0, 0), new(30, 0), new(0, 0) }], 5, LineJoin.Round, LineCap.Round, false);
        SingleCoverage(hairpin, 1000, 123);
        var rng = new Random(20261103);
        for (int test = 0; test < 30; test++)
        {
            var points = new Vector2[12];
            for (int i = 0; i < points.Length; i++) points[i] = new((float)rng.NextDouble() * 100, (float)rng.NextDouble() * 100);
            var mesh = Stroke([points], 1 + test % 10, (LineJoin)(test % 3), (LineCap)(test % 3), test % 2 == 0);
            SingleCoverage(mesh, 500, test);
        }
    }

    private static void InvalidChecks()
    {
        Throws(() => ShapeStrokeGeometry.ValidateThickness(float.NaN));
        Throws(() => ShapeStrokeGeometry.ValidateThickness(float.PositiveInfinity));
        Throws(() => ShapeStrokeGeometry.ValidatePoint(new(float.NaN, 0)));
        Throws(() => ShapeStrokeGeometry.ValidateBounds(new Bounds2(0, 0, float.PositiveInfinity, 20)));
        Throws(() => ShapeStrokeGeometry.RectangleContour(new(0, 0, 20, 20), (CornerType)99, 2, new Vector2[36]));
        Throws(() => ShapeStrokeGeometry.RectangleContour(new(0, 0, 20, 20), CornerType.Rounded, -1, new Vector2[36]));
        Throws(() => Stroke([new Vector2[] { new(0, 0), new(1, 1) }], 1, (LineJoin)99, LineCap.Butt, false));
        Throws(() => Stroke([new Vector2[] { new(0, 0), new(float.NaN, 1) }], 1, LineJoin.Miter, LineCap.Butt, false));
    }

    private static PolygonTriangulationResult Stroke(IReadOnlyList<IReadOnlyList<Vector2>> points, float width, LineJoin join, LineCap cap, bool closed) =>
        ShapeStrokeGeometry.Stroke(points, width, join, cap, closed);

    private static double Area(ReadOnlySpan<Vector2> points)
    {
        double sum = 0;
        for (int i = 0; i < points.Length; i++) sum += Cross(points[i], points[(i + 1) % points.Length]);
        return Math.Abs(sum) * .5;
    }

    private static bool Convex(ReadOnlySpan<Vector2> points)
    {
        for (int i = 0; i < points.Length; i++)
            if (Cross(points[(i + 1) % points.Length] - points[i], points[(i + 2) % points.Length] - points[(i + 1) % points.Length]) < -.0001) return false;
        return true;
    }

    private static double MeshArea(PolygonTriangulationResult mesh)
    {
        double sum = 0;
        for (int i = 0; i < mesh.Indices.Count; i += 3)
        {
            Vector2 a = mesh.Vertices[mesh.Indices[i]], b = mesh.Vertices[mesh.Indices[i + 1]], c = mesh.Vertices[mesh.Indices[i + 2]];
            sum += Math.Abs(Cross(b - a, c - a)) * .5;
        }
        return sum;
    }

    private static int Coverage(PolygonTriangulationResult mesh, Vector2 point)
    {
        int count = 0;
        for (int i = 0; i < mesh.Indices.Count; i += 3)
        {
            Vector2 a = mesh.Vertices[mesh.Indices[i]], b = mesh.Vertices[mesh.Indices[i + 1]], c = mesh.Vertices[mesh.Indices[i + 2]];
            double ab = Cross(b - a, point - a), bc = Cross(c - b, point - b), ca = Cross(a - c, point - c);
            if (ab > 1e-5 && bc > 1e-5 && ca > 1e-5 || ab < -1e-5 && bc < -1e-5 && ca < -1e-5) count++;
        }
        return count;
    }

    private static void SingleCoverage(PolygonTriangulationResult mesh, int samples, int seed)
    {
        if (mesh.Vertices.Count == 0) return;
        Vector2 min = new(float.MaxValue), max = new(float.MinValue);
        foreach (var point in mesh.Vertices)
        {
            Check(float.IsFinite(point.X) && float.IsFinite(point.Y), "finite mesh vertex");
            min = Vector2.Min(min, point); max = Vector2.Max(max, point);
        }
        var rng = new Random(seed);
        for (int i = 0; i < samples; i++)
        {
            var point = min + new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()) * (max - min);
            Check(Coverage(mesh, point) <= 1, "triangle coverage must not overlap at " + point);
        }
    }

    private static double Cross(Vector2 a, Vector2 b) => (double)a.X * b.Y - (double)a.Y * b.X;

    private static void Throws(Action action)
    {
        try { action(); } catch (ArgumentException) { _checks++; return; }
        throw new InvalidOperationException("Expected argument validation.");
    }

    internal static void Check(bool value, string message)
    {
        _checks++;
        if (!value) throw new InvalidOperationException(message);
    }
}

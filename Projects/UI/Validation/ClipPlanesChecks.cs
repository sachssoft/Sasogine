using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls.Drawing;

/// <summary>Checks exact convex clipping without a graphics device.</summary>
internal static class ClipPlanesChecks
{
    /// <summary>Checks nested clip equations, geometry interpolation and steady-state allocations.</summary>
    /// <param name="check">The assertion callback owned by the validation runner.</param>
    internal static void Run(Action<bool, string> check)
    {
        Vector3[] planes = { new(1, 1, -2), new(-1, -1, 6), new(-1, 1, 2), new(1, -1, 2) };
        VertexPositionColorTexture Vertex(float x, float y) => new(new Vector3(x, y, 0), Color.White, new Vector2((x + 1) / 9, (y + 1) / 9));
        var a = Vertex(-1, -1); var b = Vertex(8, -1); var c = Vertex(-1, 8);
        Span<VertexPositionColorTexture> output = stackalloc VertexPositionColorTexture[20];
        var bounds = new Bounds2(-10, -10, 30, 30);
        int count = ControlBatchGeometry.ClipTriangle(a, b, c, bounds, planes, output);
        check(count >= 3, "The rotated clip produces a polygon.");
        double area = 0;
        for (int index = 0; index < count; index++)
        {
            var current = output[index]; var next = output[(index + 1) % count];
            area += current.Position.X * next.Position.Y - next.Position.X * current.Position.Y;
            check(Math.Abs(current.TextureCoordinate.X - (current.Position.X + 1) / 9) < .0001f
                && Math.Abs(current.TextureCoordinate.Y - (current.Position.Y + 1) / 9) < .0001f, "Clipping preserves interpolated texture coordinates.");
        }
        check(Math.Abs(Math.Abs(area) / 2 - 8) < .001, "The diamond clip has the exact expected area.");
        check(ControlBatchGeometry.Contains(new Vector3(2, 2, 0), planes)
            && !ControlBatchGeometry.Contains(new Vector3(5, 2, 0), planes), "Clip containment rejects the axis-aligned envelope's corners.");
        Vector3[] nested = { planes[0], planes[1], planes[2], planes[3], new(1, 0, -2) };
        count = ControlBatchGeometry.ClipTriangle(a, b, c, bounds, nested, output);
        area = 0;
        for (int index = 0; index < count; index++)
        {
            var current = output[index].Position; var next = output[(index + 1) % count].Position;
            area += current.X * next.Y - next.X * current.Y;
        }
        check(Math.Abs(Math.Abs(area) / 2 - 4) < .001, "A nested clip intersects the rotated ancestor exactly.");
        for (int index = 0; index < 100; index++) ControlBatchGeometry.ClipTriangle(a, b, c, bounds, planes, output);
        long start = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 1000; index++) ControlBatchGeometry.ClipTriangle(a, b, c, bounds, planes, output);
        check(GC.GetAllocatedBytesForCurrentThread() == start, "Steady-state convex clipping allocates no managed memory.");
    }
}

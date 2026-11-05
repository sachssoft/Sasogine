using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Clips triangles in pixel coordinates while preserving interpolated texture coordinates.
/// </summary>
internal static class ControlBatchGeometry
{
    internal static int ClipTriangle(VertexPositionColorTexture a, VertexPositionColorTexture b, VertexPositionColorTexture c, Bounds2 clip, Span<VertexPositionColorTexture> output)
        => ClipTriangle(a, b, c, clip, ReadOnlySpan<Vector3>.Empty, output);

    /// <summary>Clips a triangle against rectangular bounds and arbitrary inward halfplanes.</summary>
    internal static int ClipTriangle(VertexPositionColorTexture a, VertexPositionColorTexture b, VertexPositionColorTexture c, Bounds2 clip, ReadOnlySpan<Vector3> planes, Span<VertexPositionColorTexture> output)
    {
        Span<VertexPositionColorTexture> first = stackalloc VertexPositionColorTexture[planes.Length + 8];
        Span<VertexPositionColorTexture> second = stackalloc VertexPositionColorTexture[planes.Length + 8];
        first[0] = a;
        first[1] = b;
        first[2] = c;
        int count = 3;
        for (int side = 0; side < 4 + planes.Length && count > 0; side++)
        {
            int next = 0;
            VertexPositionColorTexture previous = first[count - 1];
            float previousDistance = ClipDistance(previous.Position, clip, planes, side);
            for (int i = 0; i < count; i++)
            {
                var current = first[i];
                float distance = ClipDistance(current.Position, clip, planes, side);
                bool inside = distance >= 0, wasInside = previousDistance >= 0;
                if (inside != wasInside)
                {
                    float t = previousDistance / (previousDistance - distance);
                    second[next++] = new VertexPositionColorTexture(Vector3.Lerp(previous.Position, current.Position, t), previous.Color == current.Color ? previous.Color : Color.Lerp(previous.Color, current.Color, t), Vector2.Lerp(previous.TextureCoordinate, current.TextureCoordinate, t));
                }

                if (inside)
                    second[next++] = current;
                previous = current;
                previousDistance = distance;
            }

            count = next;
            second[..count].CopyTo(first);
        }

        first[..count].CopyTo(output);
        return count;
    }

    /// <summary>Tests whether a point belongs to every active inward halfplane.</summary>
    internal static bool Contains(Vector3 point, ReadOnlySpan<Vector3> planes)
    {
        foreach (Vector3 plane in planes) if (point.X * plane.X + point.Y * plane.Y + plane.Z < 0) return false;
        return true;
    }

    private static float ClipDistance(Vector3 point, Bounds2 clip, ReadOnlySpan<Vector3> planes, int side)
        => side < 4 ? Distance(point, clip, side) : point.X * planes[side - 4].X + point.Y * planes[side - 4].Y + planes[side - 4].Z;

    private static float Distance(Vector3 p, Bounds2 clip, int side) => side switch
    {
        0 => p.X - clip.Left,
        1 => clip.Right - p.X,
        2 => p.Y - clip.Top,
        _ => clip.Bottom - p.Y
    };
    internal static bool Overlaps(Bounds2 a, Bounds2 b) => a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;
    internal static Bounds2 Union(Bounds2 a, Bounds2 b)
    {
        float x = MathF.Min(a.X, b.X), y = MathF.Min(a.Y, b.Y);
        return new Bounds2(x, y, MathF.Max(a.Right, b.Right) - x, MathF.Max(a.Bottom, b.Bottom) - y);
    }
}

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
    {
        Span<VertexPositionColorTexture> first = stackalloc VertexPositionColorTexture[12];
        Span<VertexPositionColorTexture> second = stackalloc VertexPositionColorTexture[12];
        first[0] = a;
        first[1] = b;
        first[2] = c;
        int count = 3;
        for (int side = 0; side < 4 && count > 0; side++)
        {
            int next = 0;
            VertexPositionColorTexture previous = first[count - 1];
            float previousDistance = Distance(previous.Position, clip, side);
            for (int i = 0; i < count; i++)
            {
                var current = first[i];
                float distance = Distance(current.Position, clip, side);
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

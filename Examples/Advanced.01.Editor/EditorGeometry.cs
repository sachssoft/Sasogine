using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Shared center rotation for rendering and inverse-transformed hit testing.</summary>
internal static class EditorGeometry
{
    internal static Vector2 Center(EditorObject item) => new(item.X + item.Width / 2, item.Y + item.Height / 2);
    internal static Vector2 Rotate(Vector2 point, Vector2 center, float degrees)
    {
        float a = degrees * MathF.PI / 180, c = MathF.Cos(a), s = MathF.Sin(a);
        Vector2 p = point - center;
        return center + new Vector2(p.X * c - p.Y * s, p.X * s + p.Y * c);
    }

    internal static bool Contains(EditorObject item, Vector2 point)
    {
        Vector2 p = Rotate(point, Center(item), -item.RotationDegrees);
        return p.X >= item.X && p.Y >= item.Y && p.X < item.X + item.Width && p.Y < item.Y + item.Height;
    }

    internal static Vector2[] Corners(EditorObject item)
    {
        Vector2 center = Center(item);
        return[Rotate(new(item.X, item.Y), center, item.RotationDegrees), Rotate(new(item.X + item.Width, item.Y), center, item.RotationDegrees), Rotate(new(item.X + item.Width, item.Y + item.Height), center, item.RotationDegrees), Rotate(new(item.X, item.Y + item.Height), center, item.RotationDegrees)];
    }
}

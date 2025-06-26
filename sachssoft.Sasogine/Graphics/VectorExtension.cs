using Microsoft.Xna.Framework;

namespace sachssoft.Sasogine.Graphics;

public static class VectorExtension
{
    public static Vector2 Projection(this Vector2 vector, Vector2 normal)
    {
        return normal - ((Vector2.Dot(vector, normal) / Vector2.Dot(vector, vector)) * vector);
    }

    public static Vector2 Rejection(this Vector2 vector, Vector2 normal)
    {
        return vector - Projection(vector, normal);
    }

    public static Vector2 UnitCircle(this Vector2 position, float rotation, float distance)
    {
        return Vector2.Add(new Vector2(float.Cos(rotation) * distance, float.Sin(rotation) * distance), position);
    }

    public static Vector2 Abs(this Vector2 vector)
    {
        return new Vector2(float.Abs(vector.X), float.Abs(vector.Y));
    }
}

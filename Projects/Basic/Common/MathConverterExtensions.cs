using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

public static class MathConverterExtensions
{
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.X, vector.Y);
    }

    public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
    {
        return new Vector3(vector.X, vector.Y, z);
    }

    public static Matrix ToTranslationMatrix(this Vector2 vector)
    {
        return ToTranslationMatrix(vector.ToVector3());
    }

    public static Matrix ToTranslationMatrix(this Vector3 vector)
    {
        return Matrix.CreateTranslation(vector);
    }

    public static Matrix ToScaleMatrix(this Vector2 vector)
    {
        return ToScaleMatrix(vector.ToVector3());
    }

    public static Matrix ToScaleMatrix(this Vector3 vector)
    {
        return Matrix.CreateScale(vector);
    }

    public static Matrix ToRotationXMatrix(this float value)
    {
        return Matrix.CreateRotationX(value);
    }

    public static Matrix ToRotationYMatrix(this float value)
    {
        return Matrix.CreateRotationY(value);
    }

    public static Matrix ToRotationZMatrix(this float value)
    {
        return Matrix.CreateRotationZ(value);
    }

    public static Matrix ToRotationMatrix(this Vector2 vector)
    {
        return ToRotationMatrix(vector.ToVector3());
    }

    public static Matrix ToRotationMatrix(this Vector3 vector)
    {
        return Matrix.CreateRotationX(vector.X) * Matrix.CreateRotationX(vector.Y) * Matrix.CreateRotationX(vector.Z);
    }
}

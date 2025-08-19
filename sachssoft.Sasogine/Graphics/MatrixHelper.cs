using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics;

public static class MatrixHelper
{
    public static Matrix Create(Vector2 position, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
    {
        // Origin 0-1f
        // https://stackoverflow.com/questions/14962285/how-rotate-a-3d-cube-at-its-center-xna

        return Matrix.CreateTranslation(-origin.X, -origin.Y, 0f) *
               Matrix.CreateScale(scale.X, scale.Y, 1f) *
               Matrix.CreateRotationZ(rotation) *
               Matrix.CreateTranslation(position.X, position.Y, depth);
    }

    public static Matrix CreateCenter(Vector2 position, Vector2 scale, float rotation, float depth = 0f)
    {
        return Create(position, scale, rotation, new Vector2(0.5f), depth);
    }

    public static Matrix CreateCenter(Vector2 position, Vector2 scale, float rotation)
    {
        return Create(position, scale, rotation, new Vector2(0.5f));
    }

    public static Matrix CreateCenter(Vector2 position, Vector2 scale)
    {
        return Create(position, scale, 0f, new Vector2(0.5f));
    }

    public static Matrix CreateCenter(Vector2 position, float rotation)
    {
        return Create(position, Vector2.One, rotation, new Vector2(0.5f));
    }

    public static Matrix CreateCenter(Vector2 position)
    {
        return Create(position, Vector2.One, 0f, new Vector2(0.5f));
    }

    public static Matrix CreateSize(Vector2 position, Point size, float rotation, Vector2 origin)
    {
        return Create(position, size.ToVector2(), rotation, origin);
    }

    public static Matrix CreateSizeCenter(Vector2 position, Point size, float rotation)
    {
        return CreateCenter(position, size.ToVector2(), rotation);
    }

    public static Matrix CreateDepth(float z)
    {
        return Matrix.CreateTranslation(0f, 0f, z);
    }
}

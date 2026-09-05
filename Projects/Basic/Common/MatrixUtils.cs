using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides helper methods for creating common 2D transformation matrices.
/// </summary>
public static class MatrixUtils
{
    /// <summary>
    /// Creates a transformation matrix using position, scale, rotation, and depth.
    /// </summary>
    public static Matrix Create(
        Point2 position,
        Vector2 scale,
        float rotation,
        float depth = 0f)
    {
        return
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(position.X, position.Y, depth);
    }

    /// <summary>
    /// Creates a transformation matrix around an absolute pivot point.
    /// </summary>
    public static Matrix Create(
        Point2 position,
        Vector2 scale,
        float rotation,
        Point2 pivot,
        float depth = 0f)
    {
        return
            Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f) *
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(
                position.X + pivot.X,
                position.Y + pivot.Y,
                depth);
    }

    /// <summary>
    /// Creates a transformation matrix using a normalized pivot.
    /// </summary>
    public static Matrix Create(
        Point2 position,
        Vector2 scale,
        float rotation,
        Vector2 pivot,
        Vector2 size,
        float depth = 0f)
    {
        var pivotX = pivot.X * size.X;
        var pivotY = pivot.Y * size.Y;

        return
            Matrix.CreateTranslation(-pivotX, -pivotY, 0f) *
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(
                position.X + pivotX,
                position.Y + pivotY,
                depth);
    }

    /// <summary>
    /// Creates a transformation matrix around the center of the specified size.
    /// </summary>
    public static Matrix CreateCenter(
        Point2 position,
        Vector2 scale,
        float rotation,
        Vector2 size,
        float depth = 0f)
    {
        return Create(
            position,
            scale,
            rotation,
            new Vector2(0.5f),
            size,
            depth);
    }

    /// <summary>
    /// Creates a translation matrix.
    /// </summary>
    public static Matrix CreateTranslation(
        Point2 position,
        float depth = 0f)
    {
        return Matrix.CreateTranslation(
            position.X,
            position.Y,
            depth);
    }

    /// <summary>
    /// Creates a rotation matrix around an absolute pivot point.
    /// </summary>
    public static Matrix CreateRotation(
        float rotation,
        Point2 pivot)
    {
        return
            Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateTranslation(pivot.X, pivot.Y, 0f);
    }

    /// <summary>
    /// Creates a scale matrix around an absolute pivot point.
    /// </summary>
    public static Matrix CreateScale(
        Vector2 scale,
        Point2 pivot)
    {
        return
            Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f) *
            Matrix.CreateScale(scale.X, scale.Y, 1f) *
            Matrix.CreateTranslation(pivot.X, pivot.Y, 0f);
    }

    /// <summary>
    /// Creates a resize transformation from the current size to a new size.
    /// </summary>
    public static Matrix CreateResize(
        Vector2 currentSize,
        Vector2 newSize)
    {
        return Matrix.CreateScale(
            GetResizeScale(currentSize.X, newSize.X),
            GetResizeScale(currentSize.Y, newSize.Y),
            1f);
    }

    /// <summary>
    /// Creates a resize transformation around an absolute pivot point.
    /// </summary>
    public static Matrix CreateResize(
        Vector2 currentSize,
        Vector2 newSize,
        Point2 pivot)
    {
        var scale = new Vector2(
            GetResizeScale(currentSize.X, newSize.X),
            GetResizeScale(currentSize.Y, newSize.Y));

        return CreateScale(scale, pivot);
    }

    /// <summary>
    /// Creates a resize transformation around a normalized pivot.
    /// </summary>
    public static Matrix CreateResize(
        Vector2 currentSize,
        Vector2 newSize,
        Vector2 pivot)
    {
        var absolutePivot = new Point2(
            currentSize.X * pivot.X,
            currentSize.Y * pivot.Y);

        return CreateResize(
            currentSize,
            newSize,
            absolutePivot);
    }

    /// <summary>
    /// Creates a resize transformation around the center of the current size.
    /// </summary>
    public static Matrix CreateResizeCenter(
        Vector2 currentSize,
        Vector2 newSize)
    {
        return CreateResize(
            currentSize,
            newSize,
            new Vector2(0.5f));
    }

    /// <summary>
    /// Creates a translation matrix that applies only a depth offset.
    /// </summary>
    public static Matrix CreateDepth(float z)
    {
        return Matrix.CreateTranslation(0f, 0f, z);
    }

    /// <summary>
    /// Creates a two-dimensional skew transformation matrix.
    /// </summary>
    public static Matrix CreateSkew(
        float skewX,
        float skewY)
    {
        return new Matrix(
            1f, skewY, 0f, 0f,
            skewX, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }

    private static float GetResizeScale(
        float current,
        float value)
    {
        if (current == 0f)
            throw new ArgumentOutOfRangeException(
                nameof(current),
                "Current size must not be zero.");

        return value / current;
    }
}
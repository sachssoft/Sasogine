using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Geometry;

public readonly struct BoundingBox2D : IEquatable<BoundingBox2D>
{
    public Vector2 Min { get; }
    public Vector2 Max { get; }

    public float Left => Min.X;
    public float Top => Min.Y;
    public float Right => Max.X;
    public float Bottom => Max.Y;

    public float Width => Max.X - Min.X;
    public float Height => Max.Y - Min.Y;
    public Vector2 Size => new Vector2(Width, Height);

    public Vector2 Center => new Vector2((Min.X + Max.X) * 0.5f, (Min.Y + Max.Y) * 0.5f);

    public static BoundingBox2D Empty => new BoundingBox2D(Vector2.Zero, Vector2.Zero);

    public BoundingBox2D(Vector2 min, Vector2 max)
    {
        Min = new Vector2(MathF.Min(min.X, max.X), MathF.Min(min.Y, max.Y));
        Max = new Vector2(MathF.Max(min.X, max.X), MathF.Max(min.Y, max.Y));
    }

    public bool Contains(Vector2 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y;
    }

    public bool Intersects(BoundingBox2D other)
    {
        return !(other.Max.X < Min.X || other.Min.X > Max.X ||
                 other.Max.Y < Min.Y || other.Min.Y > Max.Y);
    }

    public BoundingBox2D Union(BoundingBox2D other)
    {
        var min = new Vector2(MathF.Min(Min.X, other.Min.X), MathF.Min(Min.Y, other.Min.Y));
        var max = new Vector2(MathF.Max(Max.X, other.Max.X), MathF.Max(Max.Y, other.Max.Y));
        return new BoundingBox2D(min, max);
    }

    public BoundingBox2D Inflate(float amount)
    {
        var min = new Vector2(Min.X - amount, Min.Y - amount);
        var max = new Vector2(Max.X + amount, Max.Y + amount);
        return new BoundingBox2D(min, max);
    }

    public bool Equals(BoundingBox2D other) => Min.Equals(other.Min) && Max.Equals(other.Max);
    public override bool Equals(object? obj) => obj is BoundingBox2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    public static bool operator ==(BoundingBox2D left, BoundingBox2D right) => left.Equals(right);
    public static bool operator !=(BoundingBox2D left, BoundingBox2D right) => !(left == right);

    public override string ToString() => $"BoundingBox2D(Min={Min}, Max={Max})";

}

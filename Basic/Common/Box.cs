using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct Box : IEquatable<Box>
{
    private readonly float _minX;
    private readonly float _minY;
    private readonly float _maxX;
    private readonly float _maxY;

    public static readonly Box Zero = new Box(0f, 0f, 0f, 0f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Box(float minX, float minY, float maxX, float maxY)
    {
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
    }

    public float MinX => _minX;
    public float MinY => _minY;
    public float MaxX => _maxX;
    public float MaxY => _maxY;

    public float Width => _maxX - _minX;
    public float Height => _maxY - _minY;

    public Vector2 Min => new Vector2(_minX, _minY);
    public Vector2 Max => new Vector2(_maxX, _maxY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bounds ToBounds()
        => new Bounds(_minX, _minY, _maxX - _minX, _maxY - _minY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float x, float y)
        => x >= _minX && x < _maxX && y >= _minY && y < _maxY;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Vector2 v)
        => v.X >= _minX && v.X < _maxX && v.Y >= _minY && v.Y < _maxY;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Box other)
        => _minX == other._minX && _minY == other._minY &&
           _maxX == other._maxX && _maxY == other._maxY;

    public override bool Equals(object? obj) => obj is Box b && Equals(b);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _minX.GetHashCode();
            hash = hash * 31 + _minY.GetHashCode();
            hash = hash * 31 + _maxX.GetHashCode();
            hash = hash * 31 + _maxY.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}", _minX, _minY, _maxX, _maxY);
}
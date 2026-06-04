using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Common;

public readonly struct PixelBox : IEquatable<PixelBox>
{
    private readonly int _minX;
    private readonly int _minY;
    private readonly int _maxX;
    private readonly int _maxY;

    public static readonly PixelBox Zero = new PixelBox(0, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PixelBox(int minX, int minY, int maxX, int maxY)
    {
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;
    }

    public int MinX => _minX;
    public int MinY => _minY;
    public int MaxX => _maxX;
    public int MaxY => _maxY;

    public int Width => _maxX - _minX;
    public int Height => _maxY - _minY;

    public Point Min => new Point(_minX, _minY);
    public Point Max => new Point(_maxX, _maxY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(int x, int y)
        => x >= _minX && x < _maxX && y >= _minY && y < _maxY;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in Point p)
        => p.X >= _minX && p.X < _maxX && p.Y >= _minY && p.Y < _maxY;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(PixelBox other)
        => _minX == other._minX && _minY == other._minY &&
           _maxX == other._maxX && _maxY == other._maxY;

    public override bool Equals(object? obj) => obj is PixelBox b && Equals(b);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _minX;
            hash = hash * 31 + _minY;
            hash = hash * 31 + _maxX;
            hash = hash * 31 + _maxY;
            return hash;
        }
    }

    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture,
            "{0}, {1}, {2}, {3}", _minX, _minY, _maxX, _maxY);
}
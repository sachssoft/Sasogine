using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Box2D;

/// <summary>
/// A transform contains translation and rotation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[PublicAPI]
public struct Transform : IEquatable<Transform>
{
    /// <summary>
    /// The translation position in world space. Use <see cref="Vec2.Zero" /> for the origin.
    /// </summary>
    public Vec2 Position;
    
    /// <summary>
    /// The rotation in radians. Use <see cref="Rotation.Identity" /> for no rotation.
    /// </summary>
    public Rotation Rotation;
    
    /// <summary>
    /// The identity transform. This transform has no translation or rotation.
    /// </summary>
    public static readonly Transform Identity = new()
        {
            Position = Vec2.Zero,
            Rotation = Rotation.Identity
        };

    /// <summary>
    /// Construct a transform with a position and rotation.
    /// </summary>
    public Transform(Vec2 position, Rotation rotation)
    {
        Position = position;
        Rotation = rotation;
    }
    
    /// <summary>
    /// Returns a string representation of the transform.
    /// </summary>
    public override string ToString()
    {
        return $"Transform(Position: {Position}, Rotation: {Rotation})";
    }
    
    // Equals:
    /// <summary>
    /// Check if two transforms are equal.
    /// </summary>
    public bool Equals(Transform other) => Position.Equals(other.Position) && Rotation.Equals(other.Rotation);
    
    /// <summary>
    /// Check if an object is equal to this transform.
    /// </summary>
    public override bool Equals(object? obj) => obj is Transform other && Equals(other);
    
    /// <summary>
    /// Returns a hash code for this transform.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Position, Rotation);
    
    /// <summary>
    /// Debugger display for the transform.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string DebuggerDisplay => $"Transform(Position: {Position}, Rotation: {Rotation})";
}
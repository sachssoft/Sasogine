using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Represents the runtime state of a three-dimensional world cursor.
/// </summary>
public readonly struct WorldCursor3State
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the specified position.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    public WorldCursor3State(Point3 position)
        : this(
            position,
            position,
            Quaternion.Identity,
            Vector3.One,
            Vector3.Up,
            Vector3.Forward,
            Vector3.Right,
            Vector3.Up,
            default,
            0f,
            false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the specified position and click position.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="clickPosition">
    /// The position at which the current cursor interaction started.
    /// </param>
    public WorldCursor3State(
        Point3 position,
        Point3 clickPosition)
        : this(
            position,
            clickPosition,
            Quaternion.Identity,
            Vector3.One,
            Vector3.Up,
            Vector3.Forward,
            Vector3.Right,
            Vector3.Up,
            default,
            0f,
            false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the specified position, rotation, and scale.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="rotation">The current cursor rotation.</param>
    /// <param name="scale">The current cursor scale.</param>
    public WorldCursor3State(
        Point3 position,
        Quaternion rotation,
        Vector3 scale)
        : this(
            position,
            position,
            rotation,
            scale,
            Vector3.Up,
            Vector3.Forward,
            Vector3.Right,
            Vector3.Up,
            default,
            0f,
            false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the specified position and surface orientation.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="normal">The surface normal at the cursor position.</param>
    /// <param name="direction">The current cursor direction.</param>
    public WorldCursor3State(
        Point3 position,
        Vector3 normal,
        Vector3 direction)
        : this(
            position,
            position,
            Quaternion.Identity,
            Vector3.One,
            normal,
            direction,
            Vector3.Right,
            Vector3.Up,
            default,
            0f,
            true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the specified picking information.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="normal">The surface normal at the cursor position.</param>
    /// <param name="ray">The picking ray associated with the cursor.</param>
    /// <param name="distance">
    /// The distance from the ray origin to the cursor position.
    /// </param>
    public WorldCursor3State(
        Point3 position,
        Vector3 normal,
        Ray ray,
        float distance)
        : this(
            position,
            position,
            Quaternion.Identity,
            Vector3.One,
            normal,
            ray.Direction,
            Vector3.Right,
            Vector3.Up,
            ray,
            distance,
            true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor3State"/> struct
    /// using the complete cursor state.
    /// </summary>
    /// <param name="position">The current cursor position.</param>
    /// <param name="clickPosition">
    /// The position at which the current cursor interaction started.
    /// </param>
    /// <param name="rotation">The current cursor rotation.</param>
    /// <param name="scale">The current cursor scale.</param>
    /// <param name="normal">The surface normal at the cursor position.</param>
    /// <param name="direction">The current cursor direction.</param>
    /// <param name="right">The right direction of the cursor.</param>
    /// <param name="up">The up direction of the cursor.</param>
    /// <param name="ray">The picking ray associated with the cursor.</param>
    /// <param name="distance">
    /// The distance from the ray origin to the cursor position.
    /// </param>
    /// <param name="hasHit">
    /// Whether the cursor currently has a valid world-space hit.
    /// </param>
    public WorldCursor3State(
        Point3 position,
        Point3 clickPosition,
        Quaternion rotation,
        Vector3 scale,
        Vector3 normal,
        Vector3 direction,
        Vector3 right,
        Vector3 up,
        Ray ray,
        float distance,
        bool hasHit)
    {
        Position = position;
        ClickPosition = clickPosition;
        Rotation = rotation;
        Scale = scale;
        Normal = normal;
        Direction = direction;
        Right = right;
        Up = up;
        Ray = ray;
        Distance = distance;
        HasHit = hasHit;
    }

    /// <summary>
    /// Gets the current cursor position in world coordinates.
    /// </summary>
    public Point3 Position { get; }

    /// <summary>
    /// Gets the world position at which the current cursor interaction started.
    /// </summary>
    public Point3 ClickPosition { get; }

    /// <summary>
    /// Gets the current cursor rotation.
    /// </summary>
    public Quaternion Rotation { get; }

    /// <summary>
    /// Gets the current cursor scale.
    /// </summary>
    public Vector3 Scale { get; }

    /// <summary>
    /// Gets the surface normal at the current cursor position.
    /// </summary>
    public Vector3 Normal { get; }

    /// <summary>
    /// Gets the current cursor direction.
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    /// Gets the right direction of the cursor.
    /// </summary>
    public Vector3 Right { get; }

    /// <summary>
    /// Gets the up direction of the cursor.
    /// </summary>
    public Vector3 Up { get; }

    /// <summary>
    /// Gets the picking ray associated with the current cursor position.
    /// </summary>
    public Ray Ray { get; }

    /// <summary>
    /// Gets the distance from the ray origin to the current cursor position.
    /// </summary>
    public float Distance { get; }

    /// <summary>
    /// Gets whether the cursor currently has a valid world-space hit.
    /// </summary>
    public bool HasHit { get; }

    /// <summary>
    /// Gets the transformation matrix represented by the current cursor state.
    /// </summary>
    /// <returns>The world transformation matrix of the cursor.</returns>
    public Matrix GetTransform()
    {
        return Matrix.CreateScale(Scale) *
               Matrix.CreateFromQuaternion(Rotation) *
               Matrix.CreateTranslation(
                   Position.X,
                   Position.Y,
                   Position.Z);
    }
}
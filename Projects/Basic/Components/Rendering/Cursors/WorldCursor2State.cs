using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Represents the runtime state of a two-dimensional world cursor.
/// </summary>
public readonly struct WorldCursor2State
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCursor2State"/> struct.
    /// </summary>
    /// <param name="position">
    /// The current cursor position in world coordinates.
    /// </param>
    /// <param name="offset">
    /// The offset applied to the cursor representation.
    /// </param>
    public WorldCursor2State(
        Point2 position,
        Vector2 offset)
    {
        Position = position;
        Offset = offset;
    }

    /// <summary>
    /// Gets the current cursor position in world coordinates.
    /// </summary>
    public Point2 Position { get; }

    /// <summary>
    /// Gets the offset applied to the cursor representation.
    /// </summary>
    public Vector2 Offset { get; }
}
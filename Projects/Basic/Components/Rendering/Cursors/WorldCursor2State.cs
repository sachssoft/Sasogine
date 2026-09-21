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
    /// <param name="clickPosition">
    /// The world position at which the current click started.
    /// </param>
    public WorldCursor2State(
        Point2 position,
        Point2 clickPosition)
    {
        Position = position;
        ClickPosition = clickPosition;
    }

    /// <summary>
    /// Gets the current cursor position in world coordinates.
    /// </summary>
    public Point2 Position { get; }

    /// <summary>
    /// Gets the world position at which the current click started.
    /// </summary>
    public Point2 ClickPosition { get; }
}
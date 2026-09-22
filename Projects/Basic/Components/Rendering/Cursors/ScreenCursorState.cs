using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Represents the runtime state of a screen cursor.
/// </summary>
public readonly struct ScreenCursorState
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenCursorState"/> struct.
    /// </summary>
    /// <param name="position">
    /// The current cursor position in screen coordinates.
    /// </param>
    /// <param name="offset">
    /// The offset applied to the cursor representation.
    /// </param>
    public ScreenCursorState(
        PixelPoint2 position,
        Vector2 offset)
    {
        Position = position;
        Offset = offset;
    }

    /// <summary>
    /// Gets the current cursor position in screen coordinates.
    /// </summary>
    public PixelPoint2 Position { get; }

    /// <summary>
    /// Gets the offset applied to the cursor representation.
    /// </summary>
    public Vector2 Offset { get; }
}
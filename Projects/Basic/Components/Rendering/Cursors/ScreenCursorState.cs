using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Rendering;

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
    /// <param name="clickPosition">
    /// The screen position at which the current click started.
    /// </param>
    public ScreenCursorState(
        PixelPoint2 position,
        PixelPoint2 clickPosition)
    {
        Position = position;
        ClickPosition = clickPosition;
    }

    /// <summary>
    /// Gets the current cursor position in screen coordinates.
    /// </summary>
    public PixelPoint2 Position { get; }

    /// <summary>
    /// Gets the screen position at which the current click started.
    /// </summary>
    public PixelPoint2 ClickPosition { get; }
}
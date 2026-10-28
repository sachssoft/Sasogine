namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Defines strongly typed identifiers for standard cursors supported by <see cref="WindowsCursorService"/>.
/// </summary>
/// <remarks>The values follow the platform-independent IPlatformCursorService identifier contract.</remarks>
public enum WindowsCursorIds
{
    /// <summary>Identifies the standard arrow.</summary>
    Arrow = 0,

    /// <summary>Identifies the crosshair.</summary>
    Crosshair = 3,

    /// <summary>Identifies the link hand.</summary>
    Hand = 1,

    /// <summary>Identifies the four-direction movement cursor.</summary>
    Move = 4,

    /// <summary>Identifies the unavailable action cursor.</summary>
    NotAllowed = 9,

    /// <summary>Identifies the northwest/southeast resize cursor.</summary>
    ResizeDiagonalDown = 7,

    /// <summary>Identifies the northeast/southwest resize cursor.</summary>
    ResizeDiagonalUp = 8,

    /// <summary>Identifies the horizontal resize cursor.</summary>
    ResizeHorizontal = 5,

    /// <summary>Identifies the vertical resize cursor.</summary>
    ResizeVertical = 6,

    /// <summary>Identifies the text insertion cursor.</summary>
    Text = 2,

    /// <summary>Identifies the busy cursor.</summary>
    Wait = 10,

    /// <summary>Identifies the arrow with a busy indicator.</summary>
    WaitArrow = 11
}

using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Combines the user input methods available for navigating a Carousel.</summary>
[Flags]
public enum CarouselNavigationMode
{
    /// <summary>Allows only programmatic navigation and optional automatic advancement.</summary>
    None = 0,
    /// <summary>Displays previous and next buttons.</summary>
    Arrows = 1,
    /// <summary>Enables touch swipes and primary-button mouse drags in the page viewport.</summary>
    Swipe = 2,
    /// <summary>Enables axis-specific arrow keys, Home and End through routed keyboard input.</summary>
    Keyboard = 4,
    /// <summary>Enables all built-in navigation input methods.</summary>
    All = Arrows | Swipe | Keyboard
}

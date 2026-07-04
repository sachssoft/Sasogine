using System;

namespace Sachssoft.Sasogine.Presentation.Rendering
{
    [Flags]
    public enum VisualState
    {
        None = 0,       // Standard, nichts passiert
        Hovered = 1,    // Maus/Touch über Control
        Pressed = 2,    // Maus/Tap gedrückt
        Focused = 4,    // Keyboard/Gamepad Fokus
        Disabled = 8,   // Control deaktiviert (optional)
        Selected = 16   // Für Toggle/Checkbox/Radio (optional)
    }
}

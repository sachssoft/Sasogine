using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Input;
/// <summary>Describes one ordered UI input event.</summary>
public enum ControlInputEventType
{
    /// <summary>Moves the pointer.</summary>
    PointerMove,
    /// <summary>Presses a pointer button.</summary>
    PointerDown,
    /// <summary>Releases a pointer button.</summary>
    PointerUp,
    /// <summary>Scrolls the pointer wheel.</summary>
    PointerWheel,
    /// <summary>Presses a key.</summary>
    KeyDown,
    /// <summary>Releases a key.</summary>
    KeyUp,
    /// <summary>Provides a text character.</summary>
    TextInput,
    /// <summary>Cancels interaction when the application loses activation.</summary>
    Deactivate,
    /// <summary>Cancels one pointer without completing its gesture.</summary>
    PointerCancel
}

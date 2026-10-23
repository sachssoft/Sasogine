using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Input;
/// <summary>
/// Immutable event data. Pointer positions use root logical coordinates.
/// </summary>
public readonly record struct ControlInputEvent(ControlInputEventType Type, Vector2 Position = default, MouseButton Button = MouseButton.Left, Keys Key = Keys.None, int WheelDelta = 0, char Character = default, bool Shift = false, bool IsRepeat = false, bool Control = false, bool Alt = false, int PointerId = 0, ControlPointerKind PointerKind = ControlPointerKind.Mouse)
{
    /// <summary>
    /// Gets this event's pointer identity.
    /// </summary>
    public ControlPointer Pointer => new(PointerKind, PointerId);
}

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sachssoft.Engine.Input;

/// <summary>Holds mouse, keyboard, and touch states sampled for one update.</summary>
/// <param name="Mouse">The mouse position, buttons, and wheel state.</param>
/// <param name="Keyboard">The keyboard state.</param>
/// <param name="Touches">The native touch state.</param>
public readonly record struct InputSnapshot(MouseState Mouse, KeyboardState Keyboard, TouchCollection Touches)
{
    /// <summary>Samples each device once on the application thread.</summary>
    /// <returns>The states to share with the consumers of this update.</returns>
    public static InputSnapshot Capture() => new(
        Microsoft.Xna.Framework.Input.Mouse.GetState(),
        Microsoft.Xna.Framework.Input.Keyboard.GetState(),
        TouchPanel.GetState());
}

namespace Sachssoft.Sasogine.Inputs
{
    /// <summary>
    /// Represents the type of input device currently used or connected.
    /// </summary>
    public enum InputType
    {
        Unknown = 0,     // Fallback für unbekannte oder seltene Geräte
        Keyboard = 1,  // Standard PC keyboard
        Mouse = 2,     // Maus / Touchpad
        Touch = 3,     // Touchscreen (mobile / tablet)
        Gamepad = 4,   // Standard Gamepad / Controller
        Joystick = 5,  // Flight stick / arcade stick / optional
    }
}

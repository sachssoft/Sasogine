namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Specifies the input device type used by a shortcut.
    /// </summary>
    public enum ShortcutInputDeviceTypes
    {
        /// <summary>
        /// No input device is specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// The shortcut uses keyboard input.
        /// </summary>
        Keyboard = 1,

        /// <summary>
        /// The shortcut uses gamepad input.
        /// </summary>
        Gamepad = 2
    }
}
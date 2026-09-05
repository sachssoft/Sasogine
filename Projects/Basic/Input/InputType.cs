namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Specifies the type of input device.
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// Represents an unknown or unspecified input device.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents a keyboard.
        /// </summary>
        Keyboard = 1,

        /// <summary>
        /// Represents a mouse or similar pointing device.
        /// </summary>
        Mouse = 2,

        /// <summary>
        /// Represents a touch input device.
        /// </summary>
        Touch = 3,

        /// <summary>
        /// Represents a gamepad or game controller.
        /// </summary>
        Gamepad = 4,

        /// <summary>
        /// Represents a joystick or similar controller.
        /// </summary>
        Joystick = 5
    }
}
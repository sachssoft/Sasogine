using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Input;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Services.Platforms
{
    /// <summary>
    /// Provides Linux-specific modifier key handling and shortcut formatting.
    /// </summary>
    public class LinuxModifierKeyService : IPlatformModifierService
    {
        private readonly string[] modifiers =
        {
            "Ctrl",
            "Alt",
            "Shift"
        };

        /// <summary>
        /// Gets the number of supported modifier keys.
        /// </summary>
        public int ModifierCount => modifiers.Length;

        /// <summary>
        /// Gets the display name of a modifier by its index.
        /// </summary>
        /// <param name="index">
        /// The zero-based modifier index.
        /// </param>
        /// <returns>
        /// The modifier name, or an empty string when the index is invalid.
        /// </returns>
        public string GetModifierString(int index) =>
            index >= 0 && index < modifiers.Length
                ? modifiers[index]
                : string.Empty;

        /// <summary>
        /// Determines whether the specified modifier key is currently pressed.
        /// </summary>
        /// <param name="index">
        /// The zero-based modifier index.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the modifier is pressed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsModifierPressed(int index)
        {
            return index switch
            {
                0 => Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                     Keyboard.GetState().IsKeyDown(Keys.RightControl),

                1 => Keyboard.GetState().IsKeyDown(Keys.LeftAlt) ||
                     Keyboard.GetState().IsKeyDown(Keys.RightAlt),

                2 => Keyboard.GetState().IsKeyDown(Keys.LeftShift) ||
                     Keyboard.GetState().IsKeyDown(Keys.RightShift),

                _ => false
            };
        }

        /// <summary>
        /// Creates a display string for the specified shortcut.
        /// </summary>
        /// <param name="shortcut">
        /// The shortcut to format.
        /// </param>
        /// <returns>
        /// A human-readable representation of the shortcut.
        /// </returns>
        public string ToString(Shortcut shortcut)
        {
            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Keyboard)
            {
                var parts = new List<string>();

                for (int i = 0; i < ModifierCount; i++)
                {
                    if (shortcut.GetModifier(i))
                        parts.Add(GetModifierString(i));
                }

                parts.Add(shortcut.Keys.ToString());

                return string.Join("+", parts);
            }

            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Gamepad)
                return $"Gamepad:{shortcut.GamepadButton}";

            return string.Empty;
        }

        /// <summary>
        /// Creates a shortcut using Ctrl and the specified key.
        /// </summary>
        public static Shortcut Ctrl(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Alt and the specified key.
        /// </summary>
        public static Shortcut Alt(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(1, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Shift and the specified key.
        /// </summary>
        public static Shortcut Shift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(2, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Ctrl+Alt and the specified key.
        /// </summary>
        public static Shortcut CtrlAlt(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true);
            shortcut.SetModifier(1, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Ctrl+Shift and the specified key.
        /// </summary>
        public static Shortcut CtrlShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true);
            shortcut.SetModifier(2, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Alt+Shift and the specified key.
        /// </summary>
        public static Shortcut AltShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(1, true);
            shortcut.SetModifier(2, true);
            return shortcut;
        }

        /// <summary>
        /// Creates a shortcut using Ctrl+Alt+Shift and the specified key.
        /// </summary>
        public static Shortcut CtrlAltShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true);
            shortcut.SetModifier(1, true);
            shortcut.SetModifier(2, true);
            return shortcut;
        }
    }
}
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Services.Platform;
using System;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Represents a keyboard or gamepad shortcut.
    /// </summary>
    public struct Shortcut
    {
        private const int ModifierCount = 3;

        private byte _modifiers;

        /// <summary>
        /// Gets the default shortcut.
        /// </summary>
        public static Shortcut Default => new()
        {
            DeviceType = ShortcutInputDeviceTypes.None
        };

        /// <summary>
        /// Gets or sets the input device type.
        /// </summary>
        public ShortcutInputDeviceTypes DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the keyboard key.
        /// </summary>
        public Keys Keys { get; set; }

        /// <summary>
        /// Gets or sets the gamepad button.
        /// </summary>
        public Buttons GamepadButton { get; set; }

        /// <summary>
        /// Gets the state of the specified modifier.
        /// </summary>
        /// <param name="index">
        /// The modifier index: 0 for Control/Command,
        /// 1 for Alt/Option and 2 for Shift.
        /// </param>
        public bool GetModifier(int index)
        {
            if ((uint)index >= ModifierCount)
                return false;

            return (_modifiers & (1 << index)) != 0;
        }

        /// <summary>
        /// Sets the state of the specified modifier.
        /// </summary>
        /// <param name="index">
        /// The modifier index: 0 for Control/Command,
        /// 1 for Alt/Option and 2 for Shift.
        /// </param>
        /// <param name="value">The modifier state.</param>
        public void SetModifier(int index, bool value)
        {
            if ((uint)index >= ModifierCount)
                return;

            var mask = (byte)(1 << index);

            if (value)
                _modifiers |= mask;
            else
                _modifiers &= (byte)~mask;
        }

        /// <summary>
        /// Returns a platform-specific string representation of the shortcut.
        /// </summary>
        /// <param name="service">
        /// The platform service used to format modifier keys.
        /// </param>
        public string ToString(IPlatformKeyModifiers service)
        {
            ArgumentNullException.ThrowIfNull(service);
            return service.ToString(this);
        }

        /// <summary>
        /// Attempts to parse the specified shortcut.
        /// </summary>
        public static bool TryParse(
            string? str,
            IPlatformKeyModifiers service,
            out Shortcut shortcut)
        {
            ArgumentNullException.ThrowIfNull(service);

            shortcut = default;

            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            if (str.StartsWith("Gamepad:", StringComparison.OrdinalIgnoreCase))
            {
                var buttonText = str[8..].Trim();

                if (!Enum.TryParse<Buttons>(buttonText, true, out var button))
                    return false;

                shortcut = new Shortcut
                {
                    DeviceType = ShortcutInputDeviceTypes.Gamepad,
                    GamepadButton = button
                };

                return true;
            }

            var parts = str.Split(
                '+',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

            var result = new Shortcut
            {
                DeviceType = ShortcutInputDeviceTypes.Keyboard
            };

            var hasKey = false;

            foreach (var part in parts)
            {
                var matchedModifier = false;

                for (int i = 0; i < service.ModifierCount; i++)
                {
                    if (!string.Equals(
                        part,
                        service.GetModifierString(i),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    result.SetModifier(i, true);
                    matchedModifier = true;
                    break;
                }

                if (matchedModifier)
                    continue;

                if (hasKey || !Enum.TryParse<Keys>(part, true, out var key))
                    return false;

                result.Keys = key;
                hasKey = true;
            }

            if (!hasKey || result.Keys == Keys.None)
                return false;

            shortcut = result;
            return true;
        }

        /// <summary>
        /// Parses the specified shortcut.
        /// </summary>
        public static Shortcut Parse(
            string str,
            IPlatformKeyModifiers service)
        {
            if (TryParse(str, service, out var shortcut))
                return shortcut;

            throw new FormatException(
                $"Invalid shortcut string: '{str}'");
        }
    }
}
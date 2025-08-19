using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Sasogine.Interactions;

public struct Shortcut
{
    public ShortcutInputDeviceTypes DeviceType { get; set; }

    // Für Tastatur-Shortcut
    public Keys Keys { get; set; }

    public bool ModifierCtrl { get; set; }

    public bool ModifierAlt { get; set; }

    public bool ModifierShift { get; set; }

    // Für Gamepad-Shortcut (z.B. A, B, X, Y)
    public Buttons GamepadButton { get; set; }

    public override string ToString()
    {
        if (DeviceType == ShortcutInputDeviceTypes.Keyboard)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (ModifierCtrl) parts.Add("ModifierCtrl");
            if (ModifierAlt) parts.Add("ModifierAlt");
            if (ModifierShift) parts.Add("ModifierShift");
            parts.Add(Keys.ToString());
            return string.Join("+", parts);
        }
        else // Gamepad
        {
            return $"Gamepad:{GamepadButton}";
        }
    }

    public static bool TryParse(string input, out Shortcut shortcut)
    {
        shortcut = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        // Check if input is a gamepad shortcut
        if (input.StartsWith("gamepad:", StringComparison.OrdinalIgnoreCase))
        {
            var btnString = input.Substring(8); // nach "gamepad:"
            if (Enum.TryParse<Buttons>(btnString, true, out var btn))
            {
                shortcut = new Shortcut
                {
                    DeviceType = ShortcutInputDeviceTypes.Gamepad,
                    GamepadButton = btn
                };
                return true;
            }
            return false;
        }
        else
        {
            // Parse Keyboard shortcut (wie vorher)
            var parts = input.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            bool ctrl = false, alt = false, shift = false;
            Keys key = Keys.None;

            foreach (var part in parts)
            {
                switch (part.ToLowerInvariant())
                {
                    case "ctrl":
                    case "control":
                        ctrl = true;
                        break;
                    case "alt":
                        alt = true;
                        break;
                    case "shift":
                        shift = true;
                        break;
                    default:
                        if (Enum.TryParse<Keys>(part, true, out var parsedKey))
                        {
                            key = parsedKey;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                }
            }

            if (key == Keys.None)
                return false;

            shortcut = new Shortcut
            {
                DeviceType = ShortcutInputDeviceTypes.Keyboard,
                ModifierCtrl = ctrl,
                ModifierAlt = alt,
                ModifierShift = shift,
                Keys = key
            };
            return true;
        }
    }

    public static Shortcut Parse(string input)
    {
        if (TryParse(input, out var shortcut))
            return shortcut;
        throw new FormatException($"Ungültiger Shortcut-String: '{input}'");
    }
}

using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Interactions;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Services.Platforms
{
    public class MacOSModifierKeyService : IPlatformModifierService
    {
        private readonly string[] modifiers = { "⌘", "⌥", "⇧" }; // Cmd, Option, Shift

        public int ModifierCount => modifiers.Length;

        public string GetModifierString(int index) =>
            index >= 0 && index < modifiers.Length ? modifiers[index] : string.Empty;

        public bool IsModifierPressed(int index)
        {
            // Auf macOS meist nur Cmd, Option, Shift relevant
            return index switch
            {
                0 => Keyboard.GetState().IsKeyDown(Keys.LeftWindows) || Keyboard.GetState().IsKeyDown(Keys.RightWindows),
                1 => Keyboard.GetState().IsKeyDown(Keys.LeftAlt) || Keyboard.GetState().IsKeyDown(Keys.RightAlt),
                2 => Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift),
                _ => false
            };
        }

        public string ToString(Shortcut shortcut)
        {
            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Keyboard)
            {
                var parts = new List<string>();
                for (int i = 0; i < ModifierCount; i++)
                    if (shortcut.GetModifier(i))
                        parts.Add(GetModifierString(i));

                parts.Add(shortcut.Keys.ToString());
                return string.Join("+", parts);
            }

            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Gamepad)
                return $"Gamepad:{shortcut.GamepadButton}";

            return string.Empty;
        }

        // -------------------------
        // Factory-Methoden Shortcut
        // -------------------------
        public static Shortcut Cmd(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true); // Cmd = Index 0
            return shortcut;
        }

        public static Shortcut Option(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(1, true); // Option = Index 1
            return shortcut;
        }

        public static Shortcut Shift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(2, true); // Shift = Index 2
            return shortcut;
        }

        public static Shortcut CmdOption(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true); // Cmd
            shortcut.SetModifier(1, true); // Option
            return shortcut;
        }

        public static Shortcut CmdShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true); // Cmd
            shortcut.SetModifier(2, true); // Shift
            return shortcut;
        }

        public static Shortcut OptionShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(1, true); // Option
            shortcut.SetModifier(2, true); // Shift
            return shortcut;
        }

        public static Shortcut CmdOptionShift(Keys key)
        {
            var shortcut = new Shortcut();
            shortcut.Keys = key;
            shortcut.SetModifier(0, true); // Cmd
            shortcut.SetModifier(1, true); // Option
            shortcut.SetModifier(2, true); // Shift
            return shortcut;
        }
    }
}

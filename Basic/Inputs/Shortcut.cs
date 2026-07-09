using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Platform;
using System;

namespace Sachssoft.Sasogine.Input
{
    public struct Shortcut
    {
        public static Shortcut Default => new Shortcut
        {
            DeviceType = ShortcutInputDeviceTypes.None
        };

        public ShortcutInputDeviceTypes DeviceType { get; set; }

        public Keys Keys { get; set; }

        private bool[] modifiers;

        public bool GetModifier(int index)
        {
            if (modifiers == null) return false;
            if (index < 0 || index >= 3) return false;
            return modifiers[index];
        }

        public void SetModifier(int index, bool value)
        {
            if (modifiers == null)
                modifiers = new bool[3]; // 0=Ctrl/Cmd, 1=Alt/Option, 2=Shift
            if (index < 0 || index >= 3) return;
            modifiers[index] = value;
        }

        public Buttons GamepadButton { get; set; }

        public string ToString(IPlatformModifier? service = null)
        {
            if (service == null)
            {
                //if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                //{
                //    service = new MacOSModifierKeyService();
                //}
                //else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //{
                //    service = new LinuxModifierKeyService();
                //}
                //else
                //{
                //    service = new WindowsModifierKeyService();
                //}
            }

            return service.ToString(this);
        }

        public override string ToString()
        {
            // Ruft die ToString-Version mit dem passenden Plattform-Service auf
            return ToString(null);
        }

        public static bool TryParse(string? str, IPlatformModifier service, out Shortcut shortcut)
        {
            shortcut = default;
            if (string.IsNullOrWhiteSpace(str))
                return false;

            str = str.Trim();

            // Plattform-Service automatisch auswählen, falls null
            if (service == null)
            {
                throw new NullReferenceException("IPlatformModifier must be provided for parsing shortcuts. Automatic platform detection is not implemented in this method.");
            }

            // Gamepad-Format: "Gamepad:A"
            if (str.StartsWith("Gamepad:", StringComparison.OrdinalIgnoreCase))
            {
                var btnStr = str.Substring(8).Trim();
                if (Enum.TryParse<Buttons>(btnStr, true, out var btn))
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

            // Keyboard-Format
            var parts = str.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var tempShortcut = new Shortcut { DeviceType = ShortcutInputDeviceTypes.Keyboard };

            foreach (var partRaw in parts)
            {
                var part = partRaw.Trim();
                bool matchedModifier = false;

                // Prüfe, ob es ein Modifier ist
                for (int i = 0; i < service.ModifierCount; i++)
                {
                    if (string.Equals(part, service.GetModifierString(i), StringComparison.OrdinalIgnoreCase))
                    {
                        tempShortcut.SetModifier(i, true);
                        matchedModifier = true;
                        break;
                    }
                }

                // Wenn kein Modifier, dann Key
                if (!matchedModifier)
                {
                    if (Enum.TryParse<Keys>(part, true, out var key))
                    {
                        tempShortcut.Keys = key;
                    }
                    else
                    {
                        // Ungültiger Teil
                        return false;
                    }
                }
            }

            // Prüfe, ob ein Key gesetzt wurde
            if (tempShortcut.Keys == Keys.None)
                return false;

            shortcut = tempShortcut;
            return true;
        }

        public static bool TryParse(string? str, out Shortcut shortcut)
        {
            return TryParse(str, null, out shortcut);
        }

        public static Shortcut Parse(string str, IPlatformModifier? service = null)
        {
            if (TryParse(str, service, out var shortcut))
                return shortcut;

            throw new FormatException($"Invalid shortcut string: '{str}'");
        }
    }
}

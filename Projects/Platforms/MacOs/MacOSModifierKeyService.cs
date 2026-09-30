using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using Sachssoft.Engine.Services.Platform;
using System.Collections.Generic;

namespace Sachssoft.Engine.Platform.MacOs;

/// <summary>
/// Provides macOS-specific modifier key handling and shortcut formatting.
/// </summary>
public sealed class MacOSModifierKeyService : IPlatformKeyModifiers
{
    private static readonly string[] Modifiers =
    {
        "⌘",
        "⌥",
        "⇧"
    };

    /// <inheritdoc />
    public int ModifierCount => Modifiers.Length;

    /// <inheritdoc />
    public string GetModifierString(int index) =>
        index >= 0 && index < Modifiers.Length
            ? Modifiers[index]
            : string.Empty;

    /// <inheritdoc />
    public bool IsModifierPressed(int index)
    {
        var state = Keyboard.GetState();

        return index switch
        {
            0 => state.IsKeyDown(Keys.LeftWindows) || state.IsKeyDown(Keys.RightWindows),
            1 => state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt),
            2 => state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift),
            _ => false
        };
    }

    /// <inheritdoc />
    public string ToString(Shortcut shortcut)
    {
        if (shortcut.DeviceType == ShortcutInputDeviceTypes.Keyboard)
        {
            var parts = new List<string>();

            for (var i = 0; i < ModifierCount; i++)
            {
                if (shortcut.GetModifier(i))
                    parts.Add(GetModifierString(i));
            }

            if (shortcut.Keys != Keys.None)
                parts.Add(shortcut.Keys.ToString());

            return string.Join("+", parts);
        }

        if (shortcut.DeviceType == ShortcutInputDeviceTypes.Gamepad)
            return $"Gamepad:{shortcut.GamepadButton}";

        return string.Empty;
    }

    public static Shortcut Cmd(Keys key) => CreateKeyboardShortcut(key, 0);

    public static Shortcut Option(Keys key) => CreateKeyboardShortcut(key, 1);

    public static Shortcut Shift(Keys key) => CreateKeyboardShortcut(key, 2);

    public static Shortcut CmdOption(Keys key) => CreateKeyboardShortcut(key, 0, 1);

    public static Shortcut CmdShift(Keys key) => CreateKeyboardShortcut(key, 0, 2);

    public static Shortcut OptionShift(Keys key) => CreateKeyboardShortcut(key, 1, 2);

    public static Shortcut CmdOptionShift(Keys key) => CreateKeyboardShortcut(key, 0, 1, 2);

    private static Shortcut CreateKeyboardShortcut(Keys key, params int[] modifiers)
    {
        var shortcut = new Shortcut
        {
            DeviceType = ShortcutInputDeviceTypes.Keyboard,
            Keys = key
        };

        foreach (var modifier in modifiers)
            shortcut.SetModifier(modifier, true);

        return shortcut;
    }
}

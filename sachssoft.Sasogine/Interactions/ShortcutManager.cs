using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace sachssoft.Sasogine.Interactions;

public class ShortcutManager
{
    private readonly Dictionary<ICommand, List<Shortcut>> _command_shortcuts;
    private readonly Dictionary<(ICommand, Shortcut), bool> _pressed_shortcuts;

    public ShortcutManager()
    {
        _command_shortcuts = new Dictionary<ICommand, List<Shortcut>>();
        _pressed_shortcuts = new Dictionary<(ICommand, Shortcut), bool>();
    }

    public void Register(ICommand command, Shortcut shortcut)
    {
        if (!_command_shortcuts.TryGetValue(command, out var list))
        {
            list = new List<Shortcut>();
            _command_shortcuts[command] = list;
        }

        if (!list.Contains(shortcut))
        {
            list.Add(shortcut);
            _pressed_shortcuts[(command, shortcut)] = false;
        }
    }

    public void Unregister(ICommand command, Shortcut shortcut)
    {
        if (_command_shortcuts.TryGetValue(command, out var list))
        {
            if (list.Remove(shortcut))
            {
                _pressed_shortcuts.Remove((command, shortcut));

                if (list.Count == 0)
                    _command_shortcuts.Remove(command);
            }
        }
    }

    public void Update(KeyboardState keyboardState, GamePadState gamePadState)
    {
        foreach (var kvp in _command_shortcuts)
        {
            var command = kvp.Key;
            var shortcuts = kvp.Value;

            foreach (var shortcut in shortcuts)
            {
                if (!_pressed_shortcuts.ContainsKey((command, shortcut)))
                    _pressed_shortcuts[(command, shortcut)] = false;

                bool isPressedNow = false;

                if (shortcut.DeviceType == ShortcutInputDeviceTypes.Keyboard)
                {
                    bool ctrlPressed = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
                    bool altPressed = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
                    bool shiftPressed = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

                    bool modifiersMatch = shortcut.ModifierCtrl == ctrlPressed &&
                                          shortcut.ModifierAlt == altPressed &&
                                          shortcut.ModifierShift == shiftPressed;

                    if (modifiersMatch && keyboardState.IsKeyDown(shortcut.Keys))
                    {
                        isPressedNow = true;
                    }
                }
                else if (shortcut.DeviceType == ShortcutInputDeviceTypes.Gamepad)
                {
                    if (gamePadState.IsButtonDown(shortcut.GamepadButton))
                    {
                        isPressedNow = true;
                    }
                }

                bool wasPressed = _pressed_shortcuts[(command, shortcut)];

                if (isPressedNow && !wasPressed)
                {
                    if (command.CanExecute(null))
                        command.Execute(null);
                }

                _pressed_shortcuts[(command, shortcut)] = isPressedNow;
            }
        }
    }
}

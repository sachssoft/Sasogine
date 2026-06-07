using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Platform;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Interactions
{
    public class ShortcutManager
    {
        private readonly Dictionary<ICommand, List<Shortcut>> _command_shortcuts;
        private readonly Dictionary<(ICommand, Shortcut), bool> _pressed_shortcuts;
        private readonly IPlatformModifier _modifierService;

        public ShortcutManager(IPlatformModifier modifierService = null)
        {
            _command_shortcuts = new Dictionary<ICommand, List<Shortcut>>();
            _pressed_shortcuts = new Dictionary<(ICommand, Shortcut), bool>();

            _ = modifierService ?? throw new NullReferenceException("IPlatformModifier must be provided. Automatic platform detection is not implemented in this constructor.");
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
                        // Prüfe alle Modifier über Service
                        bool modifiersMatch = true;
                        for (int i = 0; i < _modifierService.ModifierCount; i++)
                        {
                            bool pressed = _modifierService.IsModifierPressed(i);
                            if (shortcut.GetModifier(i) != pressed)
                            {
                                modifiersMatch = false;
                                break;
                            }
                        }

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

        public string ShortcutToString(Shortcut shortcut)
        {
            return _modifierService.ToString(shortcut);
        }
    }
}

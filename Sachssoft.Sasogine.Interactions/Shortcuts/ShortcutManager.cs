using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Services;
using Sachssoft.Sasogine.Services.Platforms;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Interactions
{
    public class ShortcutManager
    {
        private readonly Dictionary<ICommand, List<Shortcut>> _command_shortcuts;
        private readonly Dictionary<(ICommand, Shortcut), bool> _pressed_shortcuts;
        private readonly IPlatformModifierService _modifierService;

        public ShortcutManager(IPlatformModifierService? modifierService = null)
        {
            _command_shortcuts = new Dictionary<ICommand, List<Shortcut>>();
            _pressed_shortcuts = new Dictionary<(ICommand, Shortcut), bool>();

            // Plattform-Service automatisch wählen, falls null
            if (modifierService != null)
            {
                _modifierService = modifierService;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _modifierService = new MacOSModifierKeyService();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _modifierService = new LinuxModifierKeyService();
            }
            else
            {
                _modifierService = new WindowsModifierKeyService();
            }
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

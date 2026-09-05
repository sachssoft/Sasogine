using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Services.Platform;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages command shortcuts for keyboard and gamepad input.
    /// </summary>
    public sealed class ShortcutManager
    {
        private readonly Dictionary<ICommand, List<Shortcut>> _commandShortcuts = new();
        private readonly Dictionary<(ICommand Command, Shortcut Shortcut), bool> _pressedShortcuts = new();
        private readonly IPlatformKeyModifiers _modifierService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutManager"/> class.
        /// </summary>
        /// <param name="modifierService">
        /// The platform-specific modifier key service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modifierService"/> is <see langword="null"/>.
        /// </exception>
        public ShortcutManager(IPlatformKeyModifiers modifierService)
        {
            _modifierService = modifierService ??
                throw new ArgumentNullException(nameof(modifierService));
        }

        /// <summary>
        /// Registers a shortcut for the specified command.
        /// </summary>
        public void Register(ICommand command, Shortcut shortcut)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (!_commandShortcuts.TryGetValue(command, out var shortcuts))
            {
                shortcuts = new List<Shortcut>();
                _commandShortcuts.Add(command, shortcuts);
            }

            if (shortcuts.Contains(shortcut))
                return;

            shortcuts.Add(shortcut);
            _pressedShortcuts.Add((command, shortcut), false);
        }

        /// <summary>
        /// Unregisters a shortcut from the specified command.
        /// </summary>
        public void Unregister(ICommand command, Shortcut shortcut)
        {
            ArgumentNullException.ThrowIfNull(command);

            if (!_commandShortcuts.TryGetValue(command, out var shortcuts))
                return;

            if (!shortcuts.Remove(shortcut))
                return;

            _pressedShortcuts.Remove((command, shortcut));

            if (shortcuts.Count == 0)
                _commandShortcuts.Remove(command);
        }

        /// <summary>
        /// Updates the registered shortcuts using the current input states.
        /// </summary>
        public void Update(
            KeyboardState keyboardState,
            GamePadState gamePadState)
        {
            foreach (var pair in _commandShortcuts)
            {
                var command = pair.Key;
                var shortcuts = pair.Value;

                foreach (var shortcut in shortcuts)
                {
                    var key = (command, shortcut);
                    var isPressed = IsPressed(shortcut, keyboardState, gamePadState);
                    var wasPressed = _pressedShortcuts[key];

                    if (isPressed && !wasPressed && command.CanExecute(null))
                        command.Execute(null);

                    _pressedShortcuts[key] = isPressed;
                }
            }
        }

        /// <summary>
        /// Returns a platform-specific string representation of the shortcut.
        /// </summary>
        public string ShortcutToString(Shortcut shortcut)
        {
            return _modifierService.ToString(shortcut);
        }

        private bool IsPressed(
            Shortcut shortcut,
            KeyboardState keyboardState,
            GamePadState gamePadState)
        {
            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Keyboard)
            {
                for (int i = 0; i < _modifierService.ModifierCount; i++)
                {
                    if (shortcut.GetModifier(i) !=
                        _modifierService.IsModifierPressed(i))
                    {
                        return false;
                    }
                }

                return keyboardState.IsKeyDown(shortcut.Keys);
            }

            if (shortcut.DeviceType == ShortcutInputDeviceTypes.Gamepad)
                return gamePadState.IsButtonDown(shortcut.GamepadButton);

            return false;
        }
    }
}
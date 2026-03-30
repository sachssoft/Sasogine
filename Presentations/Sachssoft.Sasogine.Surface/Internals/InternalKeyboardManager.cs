using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Sasogine.Surface.Internals
{
    internal class InternalKeyboardManager : IDisposable
    {
        private readonly bool[] _downKeys = new bool[256];
        private readonly bool[] _lastDownKeys = new bool[256];
        private DateTime? _lastKeyDown;
        private int _keyDownCount = 0;
        private readonly Game _game;

        public event Action<Keys>? KeyDown;       // einmal beim Drücken
        public event Action<Keys>? KeyUp;         // einmal beim Loslassen
        public event Action<Keys>? KeyPressed;    // wiederholt, solange gedrückt
        public event Action<char>? Char;          // echtes Zeichen

        public InternalKeyboardManager(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            _game.Window.TextInput += OnTextInput;
        }

        private void OnTextInput(object sender, TextInputEventArgs e)
        {
            Char?.Invoke(e.Character);
        }

        public int RepeatKeyDownStartInMs { get; set; } = 500;
        public int RepeatKeyDownInternalInMs { get; set; } = 50;

        public void UpdateKeys(bool[] currentKeys)
        {
            var now = DateTime.Now;
            for (int i = 0; i < currentKeys.Length; i++)
            {
                var key = (Keys)i;
                bool isDown = currentKeys[i];
                bool wasDown = _lastDownKeys[i];

                if (isDown && !wasDown)
                {
                    // einmalig beim Drücken
                    KeyDown?.Invoke(key);
                    _lastKeyDown = now;
                    _keyDownCount = 0;
                }
                else if (!isDown && wasDown)
                {
                    // einmalig beim Loslassen
                    KeyUp?.Invoke(key);
                    _lastKeyDown = null;
                    _keyDownCount = 0;
                }
                else if (isDown && wasDown)
                {
                    // Wiederholung
                    if (_lastKeyDown.HasValue &&
                        ((_keyDownCount == 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownStartInMs) ||
                         (_keyDownCount > 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownInternalInMs)))
                    {
                        KeyPressed?.Invoke(key);
                        _lastKeyDown = now;
                        _keyDownCount++;
                    }
                }
            }

            Array.Copy(currentKeys, _lastDownKeys, currentKeys.Length);
        }

        public bool IsKeyDown(Keys key) => _downKeys[(int)key];

        public bool IsShiftDown => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
        public bool IsControlDown => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);
        public bool IsAltDown => IsKeyDown(Keys.LeftAlt) || IsKeyDown(Keys.RightAlt);
        public bool IsAltGrDown => IsKeyDown(Keys.RightAlt) && IsKeyDown(Keys.RightControl);

        internal void SetKeyState(Keys key, bool down)
        {
            _downKeys[(int)key] = down;
        }

        public void Dispose()
        {
            _game.Window.TextInput -= OnTextInput;
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public sealed class InputManager
    {
        // interne vorherige States
        private MouseState _prevMouse;
        private List<TouchLocation> _prevTouches = new List<TouchLocation>();
        private const int KeyCount = 256; // maximal 256 Tasten
        private readonly KeyStateInfo[] _keys = new KeyStateInfo[KeyCount];
        private KeyboardState _prevKeyboard;
        private const int GamePadButtonCount = 32; // je nach Buttons im Enum
        private readonly GamePadButtonStateInfo[] _buttons = new GamePadButtonStateInfo[GamePadButtonCount];
        private GamePadState _prevGamePad;

        // --- Mouse ---
        public MouseStateInfo Mouse { get; } = new MouseStateInfo();

        // --- Touch ---
        public List<TouchPoint> Touches { get; } = new List<TouchPoint>();

        // --- Keyboard ---
        public Dictionary<Keys, KeyStateInfo> Keys { get; } = new Dictionary<Keys, KeyStateInfo>();

        // --- GamePad (optional, nur PlayerIndex.One hier) ---
        public Dictionary<Buttons, GamePadButtonStateInfo> GamePadButtons { get; } = new Dictionary<Buttons, GamePadButtonStateInfo>();

        public void Update(GameTime gameTime)
        {
            UpdateMouse();
            UpdateTouches();
            UpdateKeyboard();
            UpdateGamePad();
        }

        #region Update Methoden

        private void UpdateMouse()
        {
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Mouse.Position = new Vector2(mouse.X, mouse.Y);

            // Delta vertikal
            if (_prevMouse.Y < mouse.Y)
                Mouse.Delta = MouseDeltaState.Down;
            else if (_prevMouse.Y > mouse.Y)
                Mouse.Delta = MouseDeltaState.Up;
            else
                Mouse.Delta = MouseDeltaState.None;

            // Interaction
            Mouse.Interaction = MouseInteractionState.None;
            bool leftDown = mouse.LeftButton == ButtonState.Pressed;
            bool prevLeftDown = _prevMouse.LeftButton == ButtonState.Pressed;

            if (leftDown)
                Mouse.Interaction |= MouseInteractionState.Pressed;
            if (!leftDown && prevLeftDown)
                Mouse.Interaction |= MouseInteractionState.Released | MouseInteractionState.Clicked;

            _prevMouse = mouse;

            //Debug.WriteLine($"Mouse: Pos={Mouse.Position}, Delta={Mouse.Delta}, Interaction={Mouse.Interaction}");
        }

        private void UpdateTouches()
        {
            TouchCollection touches = TouchPanel.GetState();
            Touches.Clear();

            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];

                // Suche vorherigen Touch
                Vector2 prevPos = t.Position;
                bool prevDown = false;

                for (int j = 0; j < _prevTouches.Count; j++)
                {
                    if (_prevTouches[j].Id == t.Id)
                    {
                        prevPos = _prevTouches[j].Position;
                        prevDown = _prevTouches[j].State == TouchLocationState.Pressed;
                        break;
                    }
                }

                Vector2 delta = t.Position - prevPos;

                TouchInteractionState state = TouchInteractionState.None;
                bool down = t.State == TouchLocationState.Pressed;

                if (down)
                    state |= TouchInteractionState.Pressed;
                if (!down && prevDown)
                    state |= TouchInteractionState.Released | TouchInteractionState.Clicked | TouchInteractionState.Tapped;

                Touches.Add(new TouchPoint
                {
                    Id = t.Id,
                    Position = t.Position,
                    Delta = delta,
                    Interaction = state
                });
            }

            // prevTouches für nächsten Frame speichern
            _prevTouches.Clear();
            for (int i = 0; i < touches.Count; i++)
                _prevTouches.Add(touches[i]);
        }

        private void UpdateKeyboard()
        {
            var keyboard = Keyboard.GetState();

            for (ushort keyCode = 0; keyCode < KeyCount; keyCode++)
            {
                Keys key = (Keys)keyCode; // Mapping von byte → Keys
                bool down = keyboard.IsKeyDown(key);
                bool prevDown = _prevKeyboard.IsKeyDown(key);

                var kinfo = _keys[keyCode];
                if (kinfo == null)
                {
                    kinfo = new KeyStateInfo { Key = key };
                    _keys[keyCode] = kinfo;
                }

                kinfo.Interaction = KeyInteractionState.None;
                if (down) kinfo.Interaction |= KeyInteractionState.Down | KeyInteractionState.Pressed;
                if (!down && prevDown) kinfo.Interaction |= KeyInteractionState.Released;
            }

            _prevKeyboard = keyboard;
        }

        private void UpdateGamePad()
        {
            var gp = GamePad.GetState(PlayerIndex.One);

            for (byte i = 0; i < GamePadButtonCount; i++)
            {
                Buttons button = (Buttons)i; // Mapping von byte → Buttons
                bool down = gp.IsButtonDown(button);
                bool prevDown = _prevGamePad.IsButtonDown(button);

                var binfo = _buttons[i];
                if (binfo == null)
                {
                    binfo = new GamePadButtonStateInfo { Button = button };
                    _buttons[i] = binfo;
                }

                binfo.Interaction = GamePadButtonInteractionState.None;
                if (down) binfo.Interaction |= GamePadButtonInteractionState.Down | GamePadButtonInteractionState.Pressed;
                if (!down && prevDown) binfo.Interaction |= GamePadButtonInteractionState.Released;
            }

            _prevGamePad = gp;
        }

        #endregion
    }
}
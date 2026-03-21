using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        private MouseInfo _lastMouseInfo;
        private DateTime? _lastKeyDown;
        private int _keyDownCount = 0;
        private readonly bool[] _downKeys = new bool[0xff], _lastDownKeys = new bool[0xff];
        private Point _mousePosition;
        private Point? _touchPosition;
        private float _mouseWheelDelta;
        private TextInputState _textInputState;

        #region Mouse

        public Point PreviousMousePosition { get; private set; }

        public Point MousePosition
        {
            get => _mousePosition;
            private set
            {
                if (value == _mousePosition)
                {
                    return;
                }

                _mousePosition = value;
                InputEventsManager.Queue(this, InputEventType.MouseMoved);
            }
        }

        public float MouseWheelDelta
        {
            get => _mouseWheelDelta;

            set
            {
                _mouseWheelDelta = value;

                if (!value.IsZero())
                {
                    InputEventsManager.Queue(this, InputEventType.MouseWheel);
                }
            }
        }

        public void UpdateMouseInput()
        {
            if (UIEnvironment.MouseInfoGetter == null)
            {
                return;
            }

            var mouseInfo = UIEnvironment.MouseInfoGetter();

            // Mouse Position
            MousePosition = mouseInfo.Position;

            // Touch Position
            Point? touchPosition = null;
            if (mouseInfo.IsLeftButtonDown || mouseInfo.IsRightButtonDown || mouseInfo.IsMiddleButtonDown)
            {
                // Touch by mouse
                touchPosition = MousePosition;
            }

            TouchPosition = touchPosition;

#if STRIDE
			var handleWheel = mouseInfo.Wheel != 0;
#else
            var handleWheel = mouseInfo.Wheel != _lastMouseInfo.Wheel;
#endif

            if (handleWheel)
            {
                var delta = mouseInfo.Wheel;
#if !STRIDE
                delta -= _lastMouseInfo.Wheel;
#endif
                MouseWheelDelta = delta;
            }
            else
            {
                MouseWheelDelta = 0;
            }

            _lastMouseInfo = mouseInfo;
        }

        #endregion

        #region Touch

        public Point? PreviousTouchPosition { get; private set; }

        public Point? TouchPosition
        {
            get => _touchPosition;

            private set
            {
                if (value == _touchPosition)
                {
                    return;
                }

                var oldValue = _touchPosition;
                _touchPosition = value;

                if (value != null && oldValue == null)
                {
                    InputEventsManager.Queue(this, InputEventType.TouchDown);
                }
                else if (value == null && oldValue != null)
                {
                    InputEventsManager.Queue(this, InputEventType.TouchUp);
                }
                else if (value != null && oldValue != null &&
                    value.Value != oldValue.Value)
                {
                    InputEventsManager.Queue(this, InputEventType.TouchMoved);
                }
            }
        }

        public bool IsTouchDown => TouchPosition != null;

        public void UpdateTouchInput()
        {
            var touchState = TouchPanel.GetState();

            if (touchState.IsConnected && touchState.Count > 0)
            {
                var pos = touchState[0].Position;
                TouchPosition = new Point((int)pos.X, (int)pos.Y);
            }
            else
            {
                TouchPosition = null;
            }
        }

        #endregion

        #region Keyboard

        public void UpdateKeyboardInput()
        {
            if (UIEnvironment.DownKeysGetter == null)
            {
                return;
            }

            UIEnvironment.DownKeysGetter(_downKeys);

            var now = DateTime.Now;
            for (var i = 0; i < _downKeys.Length; ++i)
            {
                var key = (Keys)i;
                if (_downKeys[i] && !_lastDownKeys[i])
                {
                    if (key == Keys.Tab)
                    {
                        FocusNextWidget();
                    }

                    KeyDownHandler?.Invoke(key);

                    _lastKeyDown = now;
                    _keyDownCount = 0;
                }
                else if (!_downKeys[i] && _lastDownKeys[i])
                {
                    // Key had been released
                    KeyUp?.Invoke(this, new(key));
                    if (_focusedKeyboardWidget != null)
                    {
                        _focusedKeyboardWidget.OnKeyUp(key);
                    }

                    _lastKeyDown = null;
                    _keyDownCount = 0;
                }
                else if (_downKeys[i] && _lastDownKeys[i])
                {
                    if (_lastKeyDown != null &&
                                      ((_keyDownCount == 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownStartInMs) ||
                                      (_keyDownCount > 0 && (now - _lastKeyDown.Value).TotalMilliseconds > RepeatKeyDownInternalInMs)))
                    {
                        KeyDownHandler?.Invoke(key);

                        _lastKeyDown = now;
                        ++_keyDownCount;
                    }
                }
            }

            Array.Copy(_downKeys, _lastDownKeys, _downKeys.Length);
        }

        #endregion

        private void UpdateInput(PresentationContext context)
        {
            UpdateKeyboardInput();

            PreviousMousePosition = MousePosition;
            PreviousTouchPosition = TouchPosition;

            if (!UIEnvironment.IsMobile)
            {
                UpdateMouseInput();
            }
            else
            {
                try
                {
                    UpdateTouchInput();
                }
                catch (Exception)
                {
                }
            }
        }

        void IInputEventsProcessor.ProcessEvent(InputEventType eventType)
        {
            switch (eventType)
            {
                case InputEventType.MouseLeft:
                    break;
                case InputEventType.MouseEntered:
                    break;
                case InputEventType.MouseMoved:
                    MouseMoved?.Invoke(this, EventArgs.Empty);
                    break;
                case InputEventType.MouseWheel:
                    MouseWheelChanged?.Invoke(this, new(MouseWheelDelta));
                    break;
                case InputEventType.TouchLeft:
                    break;
                case InputEventType.TouchEntered:
                    break;
                case InputEventType.TouchMoved:
                    TouchMoved?.Invoke(this, EventArgs.Empty);
                    break;
                case InputEventType.TouchDown:
                    InputOnTouchDown();
                    TouchDown?.Invoke(this, EventArgs.Empty);
                    break;
                case InputEventType.TouchUp:
                    TouchUp?.Invoke(this, EventArgs.Empty);
                    break;
                case InputEventType.TouchDoubleClick:
                    TouchDoubleClick?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

    }
}

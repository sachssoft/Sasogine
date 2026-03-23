using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Surface.Internals
{
    internal class InternalPointerManager
    {
        private Point _mousePosition;
        private Point? _touchPosition;
        private float _mouseWheelDelta;

        private bool _isLeftDown;
        private bool _isRightDown;
        private bool _isMiddleDown;

        public event Action<Point>? MouseMoved;
        public event Action<Point>? MouseLeftDown;
        public event Action<Point>? MouseLeftUp;
        public event Action<Point>? MouseRightDown;
        public event Action<Point>? MouseRightUp;
        public event Action<Point>? MouseMiddleDown;
        public event Action<Point>? MouseMiddleUp;
        public event Action<float>? MouseWheelChanged;

        public event Action<Point>? TouchDown;
        public event Action<Point>? TouchUp;
        public event Action<Point>? TouchMoved;
        public event Action<Point>? TouchEntered;
        public event Action<Point>? TouchLeft;
        public event Action<Point>? TouchDoubleClick;

        public Point MousePosition => _mousePosition;
        public Point? TouchPosition => _touchPosition;
        public float MouseWheelDelta => _mouseWheelDelta;

        public bool IsLeftDown => _isLeftDown;
        public bool IsRightDown => _isRightDown;
        public bool IsMiddleDown => _isMiddleDown;
        public bool IsTouchDown => _touchPosition != null;

        public void UpdateMouse(Point position, bool leftDown, bool rightDown, bool middleDown, float wheel)
        {
            // Position ändern
            if (position != _mousePosition)
            {
                _mousePosition = position;
                MouseMoved?.Invoke(_mousePosition);
            }

            // Linke Maustaste
            if (leftDown != _isLeftDown)
            {
                _isLeftDown = leftDown;
                if (_isLeftDown)
                {
                    MouseLeftDown?.Invoke(_mousePosition);
                }
                else
                {
                    MouseLeftUp?.Invoke(_mousePosition);
                }
            }

            // Rechte Maustaste
            if (rightDown != _isRightDown)
            {
                _isRightDown = rightDown;
                if (_isRightDown)
                {
                    MouseRightDown?.Invoke(_mousePosition);
                }
                else
                {
                    MouseRightUp?.Invoke(_mousePosition);
                }
            }

            // Mittlere Maustaste
            if (middleDown != _isMiddleDown)
            {
                _isMiddleDown = middleDown;
                if (_isMiddleDown)
                {
                    MouseMiddleDown?.Invoke(_mousePosition);
                }
                else
                {
                    MouseMiddleUp?.Invoke(_mousePosition);
                }
            }

            // Mausrad
            if (float.Abs(wheel - _mouseWheelDelta) > float.Epsilon)
            {
                _mouseWheelDelta = wheel;
                MouseWheelChanged?.Invoke(_mouseWheelDelta);
            }
        }

        public void UpdateTouch(Point? position)
        {
            var oldPos = _touchPosition;
            _touchPosition = position;

            if (position.HasValue && !oldPos.HasValue)
            {
                TouchDown?.Invoke(position.Value);
                TouchEntered?.Invoke(position.Value);
            }
            else if (!position.HasValue && oldPos.HasValue)
            {
                TouchUp?.Invoke(oldPos.Value);
                TouchLeft?.Invoke(oldPos.Value);
            }
            else if (position.HasValue && oldPos.HasValue && position.Value != oldPos.Value)
            {
                TouchMoved?.Invoke(position.Value);
            }
        }
    }
}

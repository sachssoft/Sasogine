using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Input
{

    public class MouseStateWrapper : IInputState<MouseButton>
    {
        private readonly MouseState _mouseState;

        public MouseStateWrapper(MouseState mouse_state)
        {
            _mouseState = mouse_state;
        }

        public bool IsButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _mouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _mouseState.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _mouseState.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => _mouseState.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => _mouseState.XButton2 == ButtonState.Pressed,
                _ => false,
            };
        }

        public bool IsButtonUp(MouseButton button)
        {
            return !IsButtonDown(button);
        }
    }
}

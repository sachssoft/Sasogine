using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides a mouse input state backed by a MonoGame
    /// <see cref="MouseState"/>.
    /// </summary>
    public sealed class MouseStateWrapper : IInputState<MouseButton>
    {
        private readonly MouseState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseStateWrapper"/> class.
        /// </summary>
        /// <param name="state">The mouse state to wrap.</param>
        public MouseStateWrapper(MouseState state)
        {
            _state = state;
        }

        /// <inheritdoc />
        public bool IsButtonDown(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => _state.LeftButton == ButtonState.Pressed,
                MouseButton.Right => _state.RightButton == ButtonState.Pressed,
                MouseButton.Middle => _state.MiddleButton == ButtonState.Pressed,
                MouseButton.XButton1 => _state.XButton1 == ButtonState.Pressed,
                MouseButton.XButton2 => _state.XButton2 == ButtonState.Pressed,
                _ => false
            };
        }

        /// <inheritdoc />
        public bool IsButtonUp(MouseButton button)
        {
            return !IsButtonDown(button);
        }
    }
}
using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides a gamepad input state backed by a MonoGame
    /// <see cref="GamePadState"/>.
    /// </summary>
    public sealed class GamepadStateWrapper : IInputState<Buttons>
    {
        private readonly GamePadState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadStateWrapper"/> class.
        /// </summary>
        /// <param name="state">The gamepad state to wrap.</param>
        public GamepadStateWrapper(GamePadState state)
        {
            _state = state;
        }

        /// <inheritdoc />
        public bool IsButtonDown(Buttons button) =>
            _state.IsButtonDown(button);

        /// <inheritdoc />
        public bool IsButtonUp(Buttons button) =>
            _state.IsButtonUp(button);
    }
}
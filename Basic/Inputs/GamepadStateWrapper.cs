using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Input
{
    public class GamepadStateWrapper : IInputState<Buttons>
    {
        private readonly GamePadState _state;

        public GamepadStateWrapper(GamePadState state)
        {
            _state = state;
        }

        public bool IsButtonDown(Buttons button) => _state.IsButtonDown(button);

        public bool IsButtonUp(Buttons button) => _state.IsButtonUp(button);
    }
}
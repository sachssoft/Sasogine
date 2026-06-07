using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Interactions;

public class GamePadStateWrapper : IInputState<Buttons>
{
    private readonly GamePadState _state;

    public GamePadStateWrapper(GamePadState state)
    {
        _state = state;
    }

    public bool IsButtonDown(Buttons button) => _state.IsButtonDown(button);

    public bool IsButtonUp(Buttons button) => _state.IsButtonUp(button);
}
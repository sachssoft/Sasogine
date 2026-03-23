using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Interactions;

public class KeyboardStateWrapper : IInputState<Keys>
{
    private readonly KeyboardState _state;

    public KeyboardStateWrapper() : this(new()) { }

    public KeyboardStateWrapper(KeyboardState state)
    {
        _state = state;
    }

    public bool IsButtonDown(Keys button) => _state.IsKeyDown(button);

    public bool IsButtonUp(Keys button) => _state.IsKeyUp(button);
}

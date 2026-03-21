using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    public readonly struct TextInputState
    {
        public TextInputState() { }

        public char Character { get; init; } = '\0';

        public Keys Key { get; init; } = Keys.None;
    }
}

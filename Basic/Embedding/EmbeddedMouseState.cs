using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Embedding
{
    public readonly struct EmbeddedMouseState
    {
        public Vector2 Position { get; init; }

        public bool IsLeftPressed { get; init; }

        public bool IsRightPressed { get; init; }
    }
}

using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public struct TouchPoint
    {
        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Delta { get; set; }
        public TouchInteractionState Interaction { get; set; }
    }
}

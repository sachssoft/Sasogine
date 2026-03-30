using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public class MouseStateInfo
    {
        public Vector2 Position { get; internal set; }
        public MouseDeltaState Delta { get; internal set; } = MouseDeltaState.None;
        public MouseInteractionState Interaction { get; internal set; } = MouseInteractionState.None;
    }
}

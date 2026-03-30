using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public class GamePadButtonStateInfo
    {
        public Buttons Button { get; internal set; }
        public GamePadButtonInteractionState Interaction { get; internal set; } = GamePadButtonInteractionState.None;
    }
}

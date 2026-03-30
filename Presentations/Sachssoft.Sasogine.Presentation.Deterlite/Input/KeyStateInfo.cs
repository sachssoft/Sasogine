using Microsoft.Xna.Framework.Input;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public class KeyStateInfo
    {
        public Keys Key { get; internal set; }
        public KeyInteractionState Interaction { get; internal set; } = KeyInteractionState.None;
    }
}

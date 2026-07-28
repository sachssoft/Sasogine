namespace Sachssoft.Sasogine.Presentation.Input
{
    public class KeyStateInfo
    {
        public Keys Key { get; internal set; }
        public KeyInteractionState Interaction { get; internal set; } = KeyInteractionState.None;
    }
}

using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    [Flags]
    public enum MouseInteractionState
    {
        None = 0,
        Pressed = 1 << 0,
        Released = 1 << 1,
        Clicked = 1 << 2
    }
}

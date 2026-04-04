using System;

namespace Sachssoft.Sasogine.Presentation.Input
{
    [Flags]
    public enum TouchInteractionState
    {
        None = 0,
        Pressed = 1 << 0,
        Released = 1 << 1,
        Clicked = 1 << 2,
        Tapped = 1 << 3
    }
}

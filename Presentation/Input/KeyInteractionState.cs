using System;

namespace Sachssoft.Sasogine.Presentation.Input
{
    [Flags]
    public enum KeyInteractionState
    {
        None = 0,
        Pressed = 1 << 0,
        Released = 1 << 1,
        Down = 1 << 2
    }
}

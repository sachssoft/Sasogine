using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    [Flags]
    public enum GamePadButtonInteractionState
    {
        None = 0,
        Pressed = 1 << 0,
        Released = 1 << 1,
        Down = 1 << 2
    }
}

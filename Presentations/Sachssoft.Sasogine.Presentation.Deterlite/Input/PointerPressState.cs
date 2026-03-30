using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    [Flags]
    public enum PointerPressState
    {
        None = 0,
        Pressed = 1,
        Released = 2,
        Clicked = 4,
        DoubleClicked = 8
    }
}

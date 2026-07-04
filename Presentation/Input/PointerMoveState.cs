using System;

namespace Sachssoft.Sasogine.Presentation.Input
{
    [Flags]
    public enum PointerMoveState
    {
        None = 0,
        Hovered = 1,
        Enter = 2,
        Leave = 4,
        Moved = 8
    }
}

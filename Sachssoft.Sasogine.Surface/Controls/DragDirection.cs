using System;

namespace Sachssoft.Sasogine.Surface.Controls
{

    [Flags]
    public enum DragDirection
    {
        None = 0,
        Vertical = 1,
        Horizontal = 2,
        Both = Vertical | Horizontal
    }

}

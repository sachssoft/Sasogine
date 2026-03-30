using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Surface.Interactions;

public class InputContext
{
    public bool MouseOrTouchHandled { get; set; }
    public Widget MouseWheelWidget { get; set; }
    public bool ParentContainsMouse { get; set; }
    public bool ParentContainsTouch { get; set; }

    public void Reset()
    {
        MouseOrTouchHandled = false;
        MouseWheelWidget = null;
        ParentContainsMouse = true;
        ParentContainsTouch = true;
    }
}

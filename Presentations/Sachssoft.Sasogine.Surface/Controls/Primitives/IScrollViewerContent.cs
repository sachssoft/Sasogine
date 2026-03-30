
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public interface IScrollViewerContent
    {
        bool IsHorizontalScrollBarVisible { get; set; }

        bool IsVerticalScrollBarVisible { get; set; }

        bool CanScrollHorizontal { get; set; }

        bool CanScrollVertical { get; set; }

        Point ScrollPosition { get; set; }
    }
}

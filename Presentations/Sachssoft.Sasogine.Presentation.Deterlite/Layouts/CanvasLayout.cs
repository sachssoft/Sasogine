using Microsoft.Xna.Framework;
using System.Drawing;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public class CanvasLayout : LayoutBase
    {
        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            availableSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

            foreach (FrameBase child in Children)
            {
                child.Measure(availableSize);
            }

            return new Vector2();
        }
    }
}
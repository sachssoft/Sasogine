using Microsoft.Xna.Framework;
using sachssoft.Sasogine.Surface.Utility;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public class Panel : Container
{
    protected override void InternalArrange()
    {
        foreach (var control in ChildrenCopy)
        {
            if (!control.IsVisible)
            {
                continue;
            }

            LayoutControl(control);
        }
    }

    private void LayoutControl(Widget control)
    {
        control.Arrange(ActualBounds);
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        Point result = Mathematics.PointZero;

        foreach (var control in ChildrenCopy)
        {
            if (!control.IsVisible)
            {
                continue;
            }

            Point measure = control.Measure(availableSize);

            if (measure.X > result.X)
            {
                result.X = measure.X;
            }

            if (measure.Y > result.Y)
            {
                result.Y = measure.Y;
            }
        }

        return result;
    }
}
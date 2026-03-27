using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Controls;

public class VerticalStackPanel : StackPanel
{
    public override Orientation Orientation => Orientation.Vertical;

    protected override ElementBase CreateCloneInstance()
    {
        return new VerticalStackPanel();
    }
}

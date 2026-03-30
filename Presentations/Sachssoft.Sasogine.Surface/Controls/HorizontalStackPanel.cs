using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Controls;

public class HorizontalStackPanel : StackPanel
{
    public override Orientation Orientation => Orientation.Horizontal;

    protected override ElementBase CreateCloneInstance()
    {
        return new HorizontalStackPanel();
    }
}

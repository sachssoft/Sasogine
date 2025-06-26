using sachssoft.Sasogine.Surface.Visuals.Controls;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Surface.Views;

public abstract class SurfaceViewBase : ViewBase
{
    public SurfaceViewBase(ViewSwitchMode view_switch_mode = ViewSwitchMode.Restart)
        : base(view_switch_mode)
    {
    }

    public new Desktop Host => (Desktop)base.Host;

    public new Container Container => (Container)base.Container;

    protected override ISurfaceElement CreateContainer()
    {
        return new Panel();
    }

    public IList<Widget> Widgets => Container.Widgets;
}

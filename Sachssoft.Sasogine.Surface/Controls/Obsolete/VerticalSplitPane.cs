using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

[Obsolete("Should be removed and replaced by GridSplitter")]
public class VerticalSplitPane : SplitPane
{
    public override Orientation Orientation
    {
        get
        {
            return Orientation.Vertical;
        }
    }

    public VerticalSplitPane()
    {
    }

    //protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    //{
    //    ApplySplitPaneStyle(stylesheet.VerticalSplitPaneStyles.SafelyGetStyle(name));
    //}
}
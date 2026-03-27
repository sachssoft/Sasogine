using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

[Obsolete("Should be removed and replaced by GridSplitter")]
public class HorizontalSplitPane : SplitPane
{
    public override Orientation Orientation => Orientation.Horizontal;


    public HorizontalSplitPane()
    {
    }
}
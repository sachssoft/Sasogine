using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;
public class HorizontalSplitPane : SplitPane
{
    public override Orientation Orientation => Orientation.Horizontal;

    public HorizontalSplitPane(string styleName = Stylesheet.DefaultStyleName) : base(styleName)
    {
    }

    protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    {
        ApplySplitPaneStyle(stylesheet.HorizontalSplitPaneStyles.SafelyGetStyle(name));
    }
}
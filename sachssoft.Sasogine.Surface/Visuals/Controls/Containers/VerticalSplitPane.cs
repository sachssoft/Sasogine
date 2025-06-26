using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public class VerticalSplitPane : SplitPane
{
    public override Orientation Orientation
    {
        get
        {
            return Orientation.Vertical;
        }
    }

    public VerticalSplitPane(string styleName = Stylesheet.DefaultStyleName) : base(styleName)
    {
    }

    protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    {
        ApplySplitPaneStyle(stylesheet.VerticalSplitPaneStyles.SafelyGetStyle(name));
    }
}
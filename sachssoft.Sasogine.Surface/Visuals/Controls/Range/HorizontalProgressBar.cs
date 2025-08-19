using System.ComponentModel;
using Sachssoft.Sasogine.Surface.Visuals.Styles;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public class HorizontalProgressBar : ProgressBar
{
    public override Orientation Orientation
    {
        get
        {
            return Orientation.Horizontal;
        }
    }

    [DefaultValue(HorizontalAlignment.Stretch)]
    public override HorizontalAlignment HorizontalAlignment
    {
        get
        {
            return base.HorizontalAlignment;
        }
        set
        {
            base.HorizontalAlignment = value;
        }
    }

    [DefaultValue(VerticalAlignment.Top)]
    public override VerticalAlignment VerticalAlignment
    {
        get
        {
            return base.VerticalAlignment;
        }
        set
        {
            base.VerticalAlignment = value;
        }
    }

    public HorizontalProgressBar(string styleName = Stylesheet.DefaultStyleName) : base(styleName)
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Top;
    }

    protected override void InternalSetStyle(Stylesheet stylesheet, string name)
    {
        ApplyProgressBarStyle(stylesheet.HorizontalProgressBarStyles.SafelyGetStyle(name));
    }
}
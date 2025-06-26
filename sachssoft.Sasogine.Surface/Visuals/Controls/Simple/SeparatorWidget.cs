using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Xml.Serialization;
using sachssoft.Sasogine.Surface.Utility;
using sachssoft.Sasogine.Surface.Visuals;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public abstract class SeparatorWidget : Image
{
    public int Thickness { get; set; }

    [Browsable(false)]
    [XmlIgnore]
    public abstract Orientation Orientation { get; }

    protected SeparatorWidget(string styleName)
    {
        SetStyle(styleName);
    }

    public void ApplySeparatorStyle(SeparatorStyle style)
    {
        ApplyWidgetStyle(style);

        Renderable = style.Image;
        Thickness = style.Thickness;
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        var result = Mathematics.PointZero;

        if (Orientation == Orientation.Horizontal)
        {
            result.Y = Thickness;
        }
        else
        {
            result.X = Thickness;
        }

        return result;
    }

    public override void InternalRender(RenderContext context)
    {
        base.InternalRender(context);
    }

    protected internal override void CopyFrom(Widget w)
    {
        base.CopyFrom(w);

        var separator = (SeparatorWidget)w;
        Thickness = separator.Thickness;
    }
}
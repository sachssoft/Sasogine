using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Controls;

public abstract class StackPanel : Container
{
    public static readonly AttachedPropertyInfo<ProportionType> ProportionTypeProperty =
        AttachedPropertiesRegistry.Create(typeof(StackPanel), "ProportionType", ProportionType.Auto, AttachedPropertyOption.AffectsMeasure);
    public static readonly AttachedPropertyInfo<float> ProportionValueProperty =
        AttachedPropertiesRegistry.Create(typeof(StackPanel), "ProportionValue", 1.0f, AttachedPropertyOption.AffectsMeasure);

    private readonly StackPanelLayout _layout;
    private readonly ObservableCollection<Proportion> _proportions = new ObservableCollection<Proportion>();
    private bool _childrenDirty = true;

    public abstract Orientation Orientation { get; }

    public bool ShowGridLines { get; set; }

    public Color GridLinesColor { get; set; }

    public int Spacing
    {
        get => _layout.Spacing;
        set => _layout.Spacing = value;
    }

    public Proportion DefaultProportion
    {
        get => _layout.DefaultProportion;
        set => _layout.DefaultProportion = value;
    }

    protected StackPanel()
    {
        _layout = new StackPanelLayout(Orientation);
        LayoutContainer  = _layout;
        GridLinesColor = Color.White;

        _proportions.CollectionChanged += (s, e) => InvalidateChildren();
    }

    public int GetCellSize(int index) => _layout.GetCellSize(index);

    private void InvalidateChildren()
    {
        _childrenDirty = true;
    }

    protected void UpdateChildren()
    {
        if (!_childrenDirty)
        {
            return;
        }

        var index = 0;
        foreach (var widget in LayoutChildren)
        {
            if (index < _proportions.Count)
            {
                var prop = _proportions[index];
                SetProportionType(widget, prop.Type);
                SetProportionValue(widget, prop.Value);
            }

            ++index;
        }

        _childrenDirty = false;
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        UpdateChildren();

        return base.InternalMeasure(availableSize);
    }

    protected override void InternalArrange()
    {
        UpdateChildren();

        base.InternalArrange();
    }

    public override void InternalRender(RenderContext context, GameTime t)
    {

        base.InternalRender(context, t);

        if (!ShowGridLines)
        {
            return;
        }

        var bounds = ActualBounds;

        int i;
        for (i = 0; i < _layout.GridLinesX.Count; ++i)
        {
            var x = _layout.GridLinesX[i] + bounds.Left;
            context.FillRectangle(new Rectangle(x, bounds.Top, 1, bounds.Height), GridLinesColor);
        }

        for (i = 0; i < _layout.GridLinesY.Count; ++i)
        {
            var y = _layout.GridLinesY[i] + bounds.Top;
            context.FillRectangle(new Rectangle(bounds.Left, y, bounds.Width, 1), GridLinesColor);
        }
    }

    public override void OnAttachedPropertyChanged(BaseAttachedPropertyInfo propertyInfo)
    {
        base.OnAttachedPropertyChanged(propertyInfo);

        if (propertyInfo.Id == ProportionTypeProperty.Id ||
            propertyInfo.Id == ProportionValueProperty.Id)
        {
            InvalidateChildren();
        }
    }

    #region Style

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not StackPanel source)
            return;

        ShowGridLines = source.ShowGridLines;
        GridLinesColor = source.GridLinesColor;
        Spacing = source.Spacing;
        DefaultProportion = source.DefaultProportion;
    }

    #endregion

    public static ProportionType GetProportionType(Widget widget) => ProportionTypeProperty.GetValue(widget);
    public static void SetProportionType(Widget widget, ProportionType value) => ProportionTypeProperty.SetValue(widget, value);
    public static float GetProportionValue(Widget widget) => ProportionValueProperty.GetValue(widget);
    public static void SetProportionValue(Widget widget, float value) => ProportionValueProperty.SetValue(widget, value);
}

using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Overlays children within one content area.</summary>
public class Panel : ContainerControl
{
    /// <summary>Initializes an overlay panel.</summary>
    public Panel() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Panel";

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        float width = 0, height = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, available);
            width = MathF.Max(width, child.DesiredSize.Width);
            height = MathF.Max(height, child.DesiredSize.Height);
        }

        return new Size2(width, height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        foreach (Control child in Children)
            child.Arrange(contentBounds);
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}

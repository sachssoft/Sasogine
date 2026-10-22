using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Arranges tool groups in insertion order, wrapping complete groups to the next line.</summary>
public class ToolBar : WrapPanel
{
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToolBar";

    /// <summary>Adds a tool group, reserving its ownership immediately.</summary>
    /// <exception cref = "ArgumentException">The child is not a ToolStrip.</exception>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is not ToolStrip)
            throw new ArgumentException("ToolBar children must be ToolStrip groups.", nameof(child));
        base.Add(child);
    }
}

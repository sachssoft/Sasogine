using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>A radio selection rendered as a toggle button.</summary>
public sealed class SelectorButton : RadioButton
{
    /// <summary>Initializes a new selector button instance.</summary>
    public SelectorButton() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "SelectorButton";
    /// <inheritdoc/>
    protected override bool HasGlyph => false;

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds, SelectionState))
            context.FillRectangle(LocalBounds, IsChecked == true ? PressedBackground : Background);
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

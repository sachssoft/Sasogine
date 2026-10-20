namespace Sachssoft.Engine.Controls;
/// <summary>Noninteractive hover content drawn above the root without stealing focus or capture.</summary>
public class ToolTip : ContentControl
{
    /// <summary>Creates a tooltip host.</summary>
    public ToolTip() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToolTip";

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

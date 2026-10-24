namespace Sachssoft.Engine.Controls;
/// <summary>Draws the expanded expander header appearance without activation or an expansion indicator.</summary>
internal sealed class StaticHeaderPresenter : ContentControl
{
    internal StaticHeaderPresenter()
    {
        Padding = new Insets2(28, 7, 10, 7);
        ContentHorizontalAlignment = ControlAlignment.Start;
        IsFocusable = false;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToggleButton";

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = !IsAvailable() ? "Checked.Disabled" : IsPointerOver ? "Checked.Hover" : "Checked.Normal";
        if (!context.DrawSkinPart(this, "Background", LocalBounds, state))
            context.FillRectangle(LocalBounds, IsAvailable() ? PressedBackground : DisabledBackground);
    }
}

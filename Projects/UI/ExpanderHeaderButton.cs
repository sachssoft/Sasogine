namespace Sachssoft.Engine.Controls;
/// <summary>Draws an expansion indicator beside arbitrary header content.</summary>
internal sealed class ExpanderHeaderButton : ToggleButton
{
    internal ExpanderHeaderButton()
    {
        Padding = new Insets2(28, 7, 10, 7);
        ContentHorizontalAlignment = ControlAlignment.Start;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        float y = Bounds.Height / 2;
        if (IsChecked == true)
        {
            context.DrawLine(new(10, y - 2), new(14, y + 2), TextColor, 2);
            context.DrawLine(new(14, y + 2), new(18, y - 2), TextColor, 2);
        }
        else
        {
            context.DrawLine(new(12, y - 4), new(16, y), TextColor, 2);
            context.DrawLine(new(16, y), new(12, y + 4), TextColor, 2);
        }
    }
}

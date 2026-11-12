namespace Sachssoft.Engine.Controls;

/// <summary>Presents one data item through the existing content-control mechanism.</summary>
/// <remarks>Templates receive the source explicitly and can bind it through typed delegates.</remarks>
public class ItemContainer : ContentControl
{
    /// <summary>Creates a presenter for a data item.</summary>
    /// <param name="item">The data item retained by this presentation.</param>
    public ItemContainer(object? item)
    {
        DataContext = item;
        Content = item;
        ContentHorizontalAlignment = LayoutAlignment.Stretch;
        ContentVerticalAlignment = LayoutAlignment.Stretch;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (IsItemSelected && !context.DrawSkinStylePart("ListBox", "Item", LocalBounds, "Selected"))
            context.FillRectangle(LocalBounds, new Color(33, 70, 84));
    }

}

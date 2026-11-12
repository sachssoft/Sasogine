using Sachssoft.Engine;

namespace Sachssoft.Engine.Controls;

/// <summary>Displays one recycled asset preview through the asset view's existing renderer.</summary>
internal sealed class AssetViewItem : Control
{
    /// <summary>Provides the asset view's display mode and appearance.</summary>
    private readonly AssetView _owner;

    /// <summary>Creates a row for the owning asset view.</summary>
    /// <param name="owner">The asset view using this presentation.</param>
    internal AssetViewItem(AssetView owner) => _owner = owner;

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (IsItemSelected && !context.DrawSkinStylePart("ListBox", "Item", LocalBounds, "Selected"))
            context.FillRectangle(LocalBounds, new Color(33, 70, 84));
        object? data = DataContext is SelectorItem selector ? selector.Value : DataContext;
        IControlFont font = GetFont(context);
        if (data is AssetItem asset)
            _owner.DrawAssetPresentation(context, LocalBounds, asset, font);
        else
            context.DrawText(font, _owner.CaptionForAssetData(DataContext), new Vector2(8, 4), _owner.TextColor);
    }
}

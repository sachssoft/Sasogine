using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A selectable asset list with visible-only cached previews and shared TiledPanel geometry.
/// </summary>
public sealed class AssetView : ListBox
{
    private static readonly PropertyChangedEventArgs SelectedAssetArgs = new(nameof(SelectedAsset));
    /// <summary>
    /// Creates a selectable asset view with shared list interaction.
    /// </summary>
    public AssetView() => SelectionChanged += (_, _) => NotifyPropertyChanged(SelectedAssetArgs);

    private IReadOnlyList<AssetItem> _source = Array.Empty<AssetItem>();
    private string _filterText = string.Empty;
    private Predicate<AssetItem>? _filter;
    private static readonly PropertyChangedEventArgs SourceArgs = new(nameof(Source));
    private static readonly PropertyChangedEventArgs FilterTextArgs = new(nameof(FilterText));
    private static readonly PropertyChangedEventArgs FilterArgs = new(nameof(Filter));
    /// <inheritdoc/>
    protected override string DefaultStyleName => "AssetView";
    /// <summary>
    /// Gets or sets the metadata snapshot. Call Refresh after changing an existing source collection.
    /// </summary>
    public IReadOnlyList<AssetItem> Source
    {
        get => _source;
        set { ArgumentNullException.ThrowIfNull(value); if (SetNotify(ref _source, value, SourceArgs)) Refresh(); }
    }
    /// <summary>
    /// Gets or sets a case-insensitive name, identifier or kind filter.
    /// </summary>
    public string FilterText
    {
        get => _filterText;
        set { ArgumentNullException.ThrowIfNull(value); if (SetNotify(ref _filterText, value, FilterTextArgs)) Refresh(); }
    }
    /// <summary>
    /// Gets or sets an optional application or reference-type predicate.
    /// </summary>
    public Predicate<AssetItem>? Filter
    {
        get => _filter;
        set { if (SetNotify(ref _filter, value, FilterArgs)) Refresh(); }
    }
    /// <summary>
    /// Gets the primary selected asset.
    /// </summary>
    public AssetItem? SelectedAsset => SelectedItem?.Value as AssetItem;
    /// <summary>
    /// Rebuilds the filtered metadata list, preserving the primary selection by identifier.
    /// </summary>
    public void Refresh()
    {
        string? selected = SelectedAsset?.Id;
        SelectedIndex = -1;
        Items.Clear();
        foreach (AssetItem asset in Source)
        {
            if (Filter is not null && !Filter(asset)) continue;
            if (FilterText.Length > 0 && !asset.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                && !asset.Id.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                && !asset.Kind.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) continue;
            Items.Add(new SelectorItem(asset.Name, asset));
        }
        for (int i = 0; i < Items.Count; i++)
            if (((AssetItem)Items[i].Value!).Id == selected) { SelectedIndex = i; break; }
    }
    private Vector2 _dragStart;
    private AssetItem? _dragCandidate;
    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        base.OnInput(context, args);
        if (input.Type == ControlInputEventType.PointerDown && input.Button == Sachssoft.Engine.Input.MouseButton.Left)
        {
            _dragStart = input.Position;
            _dragCandidate = SelectedIndex >= 0 && Contains(GetItemBounds(SelectedIndex), input.Position.X, input.Position.Y) ? SelectedAsset : null;
        }
        else if (input.Type == ControlInputEventType.PointerMove && _dragCandidate is AssetItem asset
            && Workspace?.GetCapturedControl(input.Pointer) == this && Vector2.DistanceSquared(_dragStart, input.Position) >= 36)
        {
            Workspace.AssetDragging.Begin(this, asset, input.Pointer, input.Position);
            _dragCandidate = null;
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp) _dragCandidate = null;
    }
    /// <inheritdoc/>
    protected override void OnInteractionCancelled() { _dragCandidate = null; base.OnInteractionCancelled(); }

    /// <inheritdoc/>
    protected override float ItemSpacing => ViewMode == AssetViewMode.Tiles ? Spacing : 0;
    /// <inheritdoc/>
    protected override float ItemExtent => ViewMode == AssetViewMode.Tiles ? TileSize.Height : MathF.Max(40, ItemHeight);
    /// <inheritdoc/>
    protected override int GetColumnCount(float width)
    {
        ValidateSize(TileSize, false);
        if (TileSize.Width <= 0 || TileSize.Height <= 0 || !float.IsFinite(Spacing) || Spacing < 0)
            throw new InvalidOperationException("Invalid asset tile geometry.");
        return ViewMode == AssetViewMode.List ? 1 : TiledPanel.CalculateColumns(width, TileSize.Width, Spacing, 0, Items.Count);
    }
    /// <inheritdoc/>
    protected override float GetItemWidth(float width) => ViewMode == AssetViewMode.List ? width : MathF.Min(width, TileSize.Width);
    /// <inheritdoc/>
    protected override float MeasureItems(IControlFont font) => ViewMode == AssetViewMode.Tiles ? TileSize.Width * 3 + Spacing * 2 : 260;
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        GetVisibleRange(out int first, out int last);
        // Loading never occurs in Draw. Each entry caches success, absence or failure.
        for (int i = first; i < last; i++)
            if (Items[i].Value is AssetItem asset) asset.RequestPreview();
    }
    /// <inheritdoc/>
    protected override void DrawItem(ControlDrawContext context, int index, Bounds2 row, IControlFont font)
    {
        if (Items[index].Value is not AssetItem asset) { base.DrawItem(context, index, row, font); return; }
        using var clip = context.PushClipScope(row);
        bool tiles = ViewMode == AssetViewMode.Tiles;
        var image = tiles ? new Bounds2(row.X + 8, row.Y + 6, MathF.Max(0, row.Width - 16), MathF.Max(0, row.Height - 50))
            : new Bounds2(row.X + 5, row.Y + 4, MathF.Max(0, row.Height - 8), MathF.Max(0, row.Height - 8));
        context.FillRectangle(image, new Color(35, 42, 55));
        if (asset.Preview is AssetPreview preview && !preview.Texture.IsDisposed)
        {
            float scale = MathF.Min(image.Width / preview.Source.Width, image.Height / preview.Source.Height);
            float w = preview.Source.Width * scale, h = preview.Source.Height * scale;
            context.DrawTexture(preview.Texture, preview.Source, new Bounds2(image.X + (image.Width - w) / 2, image.Y + (image.Height - h) / 2, w, h), Color.White);
        }
        else
            context.DrawText(font, asset.PreviewError is null ? "?" : "!", new Vector2(image.X + 8, image.Y + 4), TextColor);
        float x = tiles ? row.X + 8 : image.Right + 8;
        float y = tiles ? row.Bottom - 42 : row.Y + (row.Height - font.Measure(asset.Name).Height) / 2;
        context.DrawText(font, asset.Name, new Vector2(x, y), IsEnabled ? TextColor : DisabledTextColor);
        if (tiles) context.DrawText(font, asset.Kind, new Vector2(x, row.Bottom - 22), DisabledTextColor);
    }
    private StyleProperty<Size2> _tileSize = new(new Size2(132, 132));
    private static readonly PropertyChangedEventArgs TileSizeArgs = new(nameof(TileSize));
    /// <summary>
    /// Gets or sets the TileSize; a local assignment overrides the central style.
    /// </summary>
    public Size2 TileSize { get => _tileSize.Value; set { if (_tileSize.SetLocal(value)) NotifyPropertyChanged(TileSizeArgs); } }
    private StyleProperty<float> _spacing = new(8f);
    private static readonly PropertyChangedEventArgs SpacingArgs = new(nameof(Spacing));
    /// <summary>
    /// Gets or sets the Spacing; a local assignment overrides the central style.
    /// </summary>
    public float Spacing { get => _spacing.Value; set { if (_spacing.SetLocal(value)) NotifyPropertyChanged(SpacingArgs); } }
    private StyleProperty<AssetViewMode> _viewMode = new(AssetViewMode.Tiles);
    private static readonly PropertyChangedEventArgs ViewModeArgs = new(nameof(ViewMode));
    /// <summary>
    /// Gets or sets the ViewMode; a local assignment overrides the central style.
    /// </summary>
    public AssetViewMode ViewMode { get => _viewMode.Value; set { if (_viewMode.SetLocal(value)) NotifyPropertyChanged(ViewModeArgs); } }
    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "TileSize": if (_tileSize.Apply(Converters.Convert<Size2>("Size2", entry.Value))) NotifyPropertyChanged(TileSizeArgs); break;
                case "Spacing": if (_spacing.Apply(Converters.Convert<float>("Single", entry.Value))) NotifyPropertyChanged(SpacingArgs); break;
                case "ViewMode": if (_viewMode.Apply(Converters.Convert<AssetViewMode>("AssetViewMode", entry.Value))) NotifyPropertyChanged(ViewModeArgs); break;
            }
    }
    internal override bool IsStyleLocal(string property) => property switch
    {
        "TileSize" => _tileSize.HasLocalValue,
        "Spacing" => _spacing.HasLocalValue,
        "ViewMode" => _viewMode.HasLocalValue,
        _ => base.IsStyleLocal(property)
    };
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "TileSize": if (_tileSize.ClearLocal()) NotifyPropertyChanged(TileSizeArgs); return;
            case "Spacing": if (_spacing.ClearLocal()) NotifyPropertyChanged(SpacingArgs); return;
            case "ViewMode": if (_viewMode.ClearLocal()) NotifyPropertyChanged(ViewModeArgs); return;
            default: base.ResetStyleProperty(property); return;
        }
    }
    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_tileSize.BeginStyle(new Size2(132, 132))) NotifyPropertyChanged(TileSizeArgs);
        if (_spacing.BeginStyle(8f)) NotifyPropertyChanged(SpacingArgs);
        if (_viewMode.BeginStyle(AssetViewMode.Tiles)) NotifyPropertyChanged(ViewModeArgs);
    }
    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_tileSize.CommitStyle()) NotifyPropertyChanged(TileSizeArgs);
        if (_spacing.CommitStyle()) NotifyPropertyChanged(SpacingArgs);
        if (_viewMode.CommitStyle()) NotifyPropertyChanged(ViewModeArgs);
    }
}

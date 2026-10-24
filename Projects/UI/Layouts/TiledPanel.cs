using System;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Arranges children in equal-size tiles, wrapping to the available width.
/// </summary>
public sealed class TiledPanel : ContainerControl
{
    /// <summary>
    /// Initializes a new tiled panel instance.
    /// </summary>
    public TiledPanel() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "TiledPanel";

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;
    /// <summary>
    /// Gets or sets tile size.
    /// </summary>
    public Size2 TileSize { get => ConfigTileSize; set => ConfigTileSize = value; }
    /// <summary>
    /// Gets or sets spacing.
    /// </summary>
    public float Spacing { get => ConfigSpacing; set => ConfigSpacing = value; }
    /// <summary>
    /// Gets or sets columns.
    /// </summary>
    public int Columns { get => ConfigColumns; set => ConfigColumns = value; }

    private int CountColumns(float width)
    {
        ValidateSize(TileSize, false);
        if (TileSize.Width <= 0 || TileSize.Height <= 0 || !float.IsFinite(Spacing) || Spacing < 0 || Columns < 0)
            throw new InvalidOperationException("Invalid tile layout.");
        return CalculateColumns(width, TileSize.Width, Spacing, Columns, Children.Count);
    }

    internal static int CalculateColumns(float width, float tileWidth, float spacing, int columns, int count) =>
        columns > 0 ? columns : float.IsFinite(width) ? Math.Max(1, (int)Math.Min(int.MaxValue, Math.Floor(((double)width + spacing) / (tileWidth + spacing)))) : Math.Max(1, count);

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        int columns = CountColumns(available.Width), count = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, TileSize);
            if (child.IsVisible)
                count++;
        }

        if (count == 0)
            return Size2.Zero;
        int rows = (count - 1) / columns + 1;
        return new Size2(Math.Min(columns, count) * (TileSize.Width + Spacing) - Spacing, rows * (TileSize.Height + Spacing) - Spacing);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        int columns = CountColumns(contentBounds.Width), index = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
                continue;
            child.Arrange(new Bounds2(contentBounds.X + index % columns * (TileSize.Width + Spacing), contentBounds.Y + index / columns * (TileSize.Height + Spacing), TileSize.Width, TileSize.Height));
            index++;
        }
    }

    private StyleProperty<float> _styleSpacing = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SpacingChangedArgs = new(nameof(Spacing));
    internal float ConfigSpacing
    {
        get => _styleSpacing.Value;
        set
        {
            if (_styleSpacing.SetLocal(value))
                NotifyPropertyChanged(SpacingChangedArgs);
        }
    }

    private StyleProperty<Size2> _styleTileSize = new(new Size2(120, 80));
    private static readonly System.ComponentModel.PropertyChangedEventArgs TileSizeChangedArgs = new(nameof(TileSize));
    internal Size2 ConfigTileSize
    {
        get => _styleTileSize.Value;
        set
        {
            if (_styleTileSize.SetLocal(value))
                NotifyPropertyChanged(TileSizeChangedArgs);
        }
    }

    private StyleProperty<int> _styleColumns = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColumnsChangedArgs = new(nameof(Columns));
    internal int ConfigColumns
    {
        get => _styleColumns.Value;
        set
        {
            if (_styleColumns.SetLocal(value))
                NotifyPropertyChanged(ColumnsChangedArgs);
        }
    }

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Spacing":
                    if (_styleSpacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SpacingChangedArgs);
                    break;
                case "TileSize":
                    if (_styleTileSize.Apply(Converters.Convert<Size2>("Size2", entry.Value)))
                        NotifyPropertyChanged(TileSizeChangedArgs);
                    break;
                case "Columns":
                    if (_styleColumns.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(ColumnsChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Spacing":
                return _styleSpacing.HasLocalValue;
            case "TileSize":
                return _styleTileSize.HasLocalValue;
            case "Columns":
                return _styleColumns.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Spacing":
                if (_styleSpacing.ClearLocal())
                    NotifyPropertyChanged(SpacingChangedArgs);
                return;
            case "TileSize":
                if (_styleTileSize.ClearLocal())
                    NotifyPropertyChanged(TileSizeChangedArgs);
                return;
            case "Columns":
                if (_styleColumns.ClearLocal())
                    NotifyPropertyChanged(ColumnsChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleSpacing.BeginStyle(0f))
            NotifyPropertyChanged(SpacingChangedArgs);
        if (_styleTileSize.BeginStyle(new Size2(120, 80)))
            NotifyPropertyChanged(TileSizeChangedArgs);
        if (_styleColumns.BeginStyle(0))
            NotifyPropertyChanged(ColumnsChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
        if (_styleTileSize.CommitStyle())
            NotifyPropertyChanged(TileSizeChangedArgs);
        if (_styleColumns.CommitStyle())
            NotifyPropertyChanged(ColumnsChangedArgs);
    }
}

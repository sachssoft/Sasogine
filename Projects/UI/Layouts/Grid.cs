using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Two-dimensional Auto/pixel/star layout with limits, margins and row/column spans.</summary>
public class Grid : ContainerControl
{
    private readonly GridAxisLayout _rows = new(), _columns = new();
    /// <summary>Creates a grid.</summary>
    public Grid() : base()
    {
    }

    /// <summary>Gets row propertiess.</summary>
    public IList<RowTrack> Rows => ConfigRowDefinitions;
    /// <summary>Gets column propertiess.</summary>
    public IList<ColumnTrack> Tracks => ConfigColumnDefinitions;
    /// <summary>Gets or sets the gap between tracks.</summary>
    public float Spacing { get => ConfigSpacing; set => ConfigSpacing = value; }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Grid";

    private readonly Dictionary<Control, GridCell> _cells = new();
    /// <summary>Adds a child with placement owned exclusively by this grid.</summary>
    public void Add(Control child, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        SetCell(child, row, column, rowSpan, columnSpan);
        base.Add(child);
    }

    /// <summary>Changes this grid's placement for one child.</summary>
    public void SetCell(Control child, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        if (row < 0 || column < 0 || rowSpan < 1 || columnSpan < 1)
            throw new ArgumentOutOfRangeException(nameof(row));
        _cells[child] = new(row, column, rowSpan, columnSpan);
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        base.Remove(child);
        _cells.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        _cells.Clear();
    }

    /// <summary>Gets the last arranged row height.</summary>
    public float GetRowHeight(int row) => _rows.Sizes[row];
    /// <summary>Gets the last arranged column width.</summary>
    public float GetColumnWidth(int column) => _columns.Sizes[column];
    private (int Row, int Column, int Rows, int Columns) Cell(Control child)
    {
        GridCell d = _cells.TryGetValue(child, out var cell) ? cell : new(0, 0, 1, 1);
        if (d.Row >= _rows.Count || d.Column >= _columns.Count)
            throw new InvalidOperationException("Grid cell is outside its tracks.");
        return (d.Row, d.Column, Math.Min(d.RowSpan, _rows.Count - d.Row), Math.Min(d.ColumnSpan, _columns.Count - d.Column));
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        float gap = Spacing;
        if (!float.IsFinite(gap) || gap < 0)
            throw new InvalidOperationException("Invalid grid spacing.");
        _rows.Configure(Rows);
        _columns.Configure(Tracks);
        for (int pass = 0; pass < 3; pass++)
        {
            _columns.ResolveStars(available.Width, gap);
            _rows.ResolveStars(available.Height, gap);
            foreach (Control child in Children)
            {
                var c = Cell(child);
                float w = !float.IsFinite(available.Width) || _columns.HasAuto(c.Column, c.Columns) ? float.PositiveInfinity : _columns.Span(c.Column, c.Columns, gap);
                float h = !float.IsFinite(available.Height) || _rows.HasAuto(c.Row, c.Rows) ? float.PositiveInfinity : _rows.Span(c.Row, c.Rows, gap);
                child.Measure(context, new Size2(w, h));
                _columns.Grow(c.Column, c.Columns, child.DesiredSize.Width, gap, !float.IsFinite(available.Width));
                _rows.Grow(c.Row, c.Rows, child.DesiredSize.Height, gap, !float.IsFinite(available.Height));
            }
        }

        _columns.ResolveStars(available.Width, gap);
        _rows.ResolveStars(available.Height, gap);
        return new Size2(_columns.Total(gap), _rows.Total(gap));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float gap = Spacing;
        _columns.ResolveStars(bounds.Width, gap);
        _rows.ResolveStars(bounds.Height, gap);
        foreach (Control child in Children)
        {
            var c = Cell(child);
            child.Arrange(new Bounds2(bounds.X + _columns.Offset(c.Column, gap), bounds.Y + _rows.Offset(c.Row, gap), _columns.Span(c.Column, c.Columns, gap), _rows.Span(c.Row, c.Rows, gap)), true);
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (!ConfigShowGridLines)
            return;
        float gap = Spacing;
        for (int r = 1; r < _rows.Count; r++)
            context.FillRectangle(new Bounds2(Padding.Left, Padding.Top + _rows.Offset(r, gap) - gap / 2, ContentBounds.Width, 1), BorderColor);
        for (int c = 1; c < _columns.Count; c++)
            context.FillRectangle(new Bounds2(Padding.Left + _columns.Offset(c, gap) - gap / 2, Padding.Top, 1, ContentBounds.Height), BorderColor);
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

    private ElementCollection<ColumnTrack>? _ownedColumnDefinitions;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColumnDefinitionsCollectionArgs = new("Tracks");
    internal IList<ColumnTrack> ConfigColumnDefinitions => _ownedColumnDefinitions ??= new(this, ColumnDefinitionsCollectionArgs);

    private bool _storedShowGridLines = false;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowGridLinesStorageArgs = new("ShowGridLines");
    internal bool ConfigShowGridLines { get => _storedShowGridLines; set => SetNotify(ref _storedShowGridLines, value, ShowGridLinesStorageArgs); }
    /// <summary>Gets or sets the value for ShowGridLines.</summary>
    public bool ShowGridLines { get => ConfigShowGridLines; set => ConfigShowGridLines = value; }

    private ElementCollection<RowTrack>? _ownedRowDefinitions;
    private static readonly System.ComponentModel.PropertyChangedEventArgs RowDefinitionsCollectionArgs = new("Rows");
    internal IList<RowTrack> ConfigRowDefinitions => _ownedRowDefinitions ??= new(this, RowDefinitionsCollectionArgs);

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
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
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Spacing":
                return _styleSpacing.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Spacing":
                if (_styleSpacing.ClearLocal())
                    NotifyPropertyChanged(SpacingChangedArgs);
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
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
    }
}

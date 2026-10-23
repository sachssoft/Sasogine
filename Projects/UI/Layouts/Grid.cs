using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Two-dimensional automatic, fixed and proportional layout with limits, margins and row/column spans.
/// </summary>
public class Grid : ContainerControl
{
    private readonly GridAxisLayout _rows = new(), _columns = new();
    /// <summary>
    /// Creates a grid.
    /// </summary>
    public Grid()
    {
        Cells = new CellCollection(this);
    }

    /// <summary>
    /// Gets the rows. An empty collection provides one proportional row.
    /// </summary>
    public IList<GridRow> Rows => _rowsCollection ??= new(this, RowsChangedArgs);
    /// <summary>
    /// Gets the columns. An empty collection provides one proportional column.
    /// </summary>
    public IList<GridColumn> Columns => _columnsCollection ??= new(this, ColumnsChangedArgs);
    /// <summary>
    /// Gets placement metadata, separate from child ownership.
    /// </summary>
    public CellCollection Cells { get; }

    /// <summary>
    /// Gets or sets the gap between adjacent rows and columns.
    /// </summary>
    public float Spacing
    {
        get => _styleSpacing.Value;
        set
        {
            if (_styleSpacing.SetLocal(value))
                NotifyPropertyChanged(SpacingChangedArgs);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Grid";

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        base.Remove(child);
        Cells.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        Cells.Clear();
    }

    /// <summary>
    /// Gets the last arranged row height.
    /// </summary>
    public float GetRowHeight(int row) => _rows.Sizes[row];
    /// <summary>
    /// Gets the last arranged column width.
    /// </summary>
    public float GetColumnWidth(int column) => _columns.Sizes[column];
    private GridCell GetCell(Control child)
    {
        GridCell cell = Cells.TryGetValue(child, out var placement) ? placement : new(0, 0, 1, 1);
        if (cell.Row >= _rows.Count || cell.Column >= _columns.Count || cell.RowSpan > _rows.Count - cell.Row || cell.ColumnSpan > _columns.Count - cell.Column)
            throw new InvalidOperationException("Grid cell is outside its rows or columns.");
        return cell;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        float gap = Spacing;
        if (!float.IsFinite(gap) || gap < 0)
            throw new InvalidOperationException("Invalid grid spacing.");
        _rows.Configure(Rows);
        _columns.Configure(Columns);
        for (int pass = 0; pass < 3; pass++)
        {
            _columns.ResolveProportions(available.Width, gap);
            _rows.ResolveProportions(available.Height, gap);
            foreach (Control child in Children)
            {
                GridCell cell = GetCell(child);
                float width = !float.IsFinite(available.Width) || _columns.HasAuto(cell.Column, cell.ColumnSpan) ? float.PositiveInfinity : _columns.Span(cell.Column, cell.ColumnSpan, gap);
                float height = !float.IsFinite(available.Height) || _rows.HasAuto(cell.Row, cell.RowSpan) ? float.PositiveInfinity : _rows.Span(cell.Row, cell.RowSpan, gap);
                child.Measure(context, new Size2(width, height));
                _columns.Grow(cell.Column, cell.ColumnSpan, child.DesiredSize.Width, gap, !float.IsFinite(available.Width));
                _rows.Grow(cell.Row, cell.RowSpan, child.DesiredSize.Height, gap, !float.IsFinite(available.Height));
            }
        }

        _columns.ResolveProportions(available.Width, gap);
        _rows.ResolveProportions(available.Height, gap);
        return new Size2(_columns.Total(gap), _rows.Total(gap));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float gap = Spacing;
        _columns.ResolveProportions(bounds.Width, gap);
        _rows.ResolveProportions(bounds.Height, gap);
        foreach (Control child in Children)
        {
            GridCell cell = GetCell(child);
            child.Arrange(new Bounds2(bounds.X + _columns.Offset(cell.Column, gap), bounds.Y + _rows.Offset(cell.Row, gap), _columns.Span(cell.Column, cell.ColumnSpan, gap), _rows.Span(cell.Row, cell.RowSpan, gap)), true);
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (!ShowGridLines)
            return;
        float gap = Spacing;
        for (int r = 1; r < _rows.Count; r++)
            context.FillRectangle(new Bounds2(Padding.Left, Padding.Top + _rows.Offset(r, gap) - gap / 2, ContentBounds.Width, 1), BorderColor);
        for (int c = 1; c < _columns.Count; c++)
            context.FillRectangle(new Bounds2(Padding.Left + _columns.Offset(c, gap) - gap / 2, Padding.Top, 1, ContentBounds.Height), BorderColor);
    }

    private StyleProperty<float> _styleSpacing = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SpacingChangedArgs = new(nameof(Spacing));
    private ElementCollection<GridRow>? _rowsCollection;
    private ElementCollection<GridColumn>? _columnsCollection;
    private static readonly System.ComponentModel.PropertyChangedEventArgs RowsChangedArgs = new(nameof(Rows));
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColumnsChangedArgs = new(nameof(Columns));
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowGridLinesChangedArgs = new(nameof(ShowGridLines));
    private bool _showGridLines;
    /// <summary>
    /// Gets or sets whether row and column boundaries are drawn.
    /// </summary>
    public  bool ShowGridLines
    {
        get => _showGridLines;
        set => SetNotify(ref _showGridLines, value, ShowGridLinesChangedArgs);
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

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
    }
}

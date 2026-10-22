using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>Arranges ButtonBase controls in equal-size cells using fixed columns, fixed rows or automatic wrapping.</summary>
/// <remarks>Positive Columns takes precedence over Rows. Rows-only layout fills each column from top to bottom.</remarks>
public class ToolBox : ContainerControl
{
    private Size2 _cellSize;
    private int _visibleCount;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "ToolBox";

    /// <summary>Adds a button, toggle, selector or another ButtonBase-derived control.</summary>
    /// <exception cref = "ArgumentException">The child does not derive from ButtonBase.</exception>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is not ButtonBase)
            throw new ArgumentException("ToolBox children must derive from ButtonBase.", nameof(child));
        base.Add(child);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (Columns < 0 || Rows < 0 || !float.IsFinite(Spacing) || Spacing < 0)
            throw new InvalidOperationException("Invalid toolbox dimensions or spacing.");
        float width = 0, height = 0;
        _visibleCount = 0;
        foreach (Control child in Children)
        {
            child.Measure(context, available);
            if (!child.IsVisible)
                continue;
            width = MathF.Max(width, child.DesiredSize.Width);
            height = MathF.Max(height, child.DesiredSize.Height);
            _visibleCount++;
        }

        _cellSize = new Size2(width, height);
        if (_visibleCount == 0)
            return Size2.Zero;
        GetDimensions(available.Width, out int columns, out int rows);
        return new Size2(columns * width + (columns - 1) * Spacing, rows * height + (rows - 1) * Spacing);
    }

    private void GetDimensions(float availableWidth, out int columns, out int rows)
    {
        if (Columns > 0)
        {
            columns = Columns;
            rows = (_visibleCount - 1) / columns + 1;
        }
        else if (Rows > 0)
        {
            rows = Rows;
            columns = (_visibleCount - 1) / rows + 1;
        }
        else
        {
            float stride = _cellSize.Width + Spacing;
            columns = float.IsFinite(availableWidth) && stride > 0 ? Math.Max(1, (int)Math.Min(_visibleCount, Math.Floor(((double)availableWidth + Spacing) / stride))) : Math.Max(1, _visibleCount);
            rows = (_visibleCount - 1) / columns + 1;
        }
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        if (_visibleCount == 0)
        {
            foreach (Control child in Children)
                child.Arrange(Bounds2.Zero);
            return;
        }

        GetDimensions(bounds.Width, out int columns, out int rows);
        bool byColumn = Columns == 0 && Rows > 0;
        int index = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(Bounds2.Zero);
                continue;
            }

            int column = byColumn ? index / rows : index % columns;
            int row = byColumn ? index % rows : index / columns;
            child.Arrange(new Bounds2(bounds.X + column * (_cellSize.Width + Spacing), bounds.Y + row * (_cellSize.Height + Spacing), _cellSize.Width, _cellSize.Height));
            index++;
        }
    }

    private StyleProperty<int> _columns = new(0);
    private static readonly PropertyChangedEventArgs ColumnsChangedArgs = new(nameof(Columns));
    /// <summary>Gets or sets the fixed column count; zero allows Rows or automatic width-based wrapping.</summary>
    public int Columns
    {
        get => _columns.Value;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_columns.SetLocal(value))
                NotifyPropertyChanged(ColumnsChangedArgs);
        }
    }

    private StyleProperty<int> _rows = new(0);
    private static readonly PropertyChangedEventArgs RowsChangedArgs = new(nameof(Rows));
    /// <summary>Gets or sets the fixed row count when Columns is zero; zero enables automatic wrapping.</summary>
    public int Rows
    {
        get => _rows.Value;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_rows.SetLocal(value))
                NotifyPropertyChanged(RowsChangedArgs);
        }
    }

    private StyleProperty<float> _spacing = new(4f);
    private static readonly PropertyChangedEventArgs SpacingChangedArgs = new(nameof(Spacing));
    /// <summary>Gets or sets the gap between adjacent cells.</summary>
    public float Spacing
    {
        get => _spacing.Value;
        set
        {
            if (!float.IsFinite(value) || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_spacing.SetLocal(value))
                NotifyPropertyChanged(SpacingChangedArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "Columns":
                    if (_columns.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(ColumnsChangedArgs);
                    break;
                case "Rows":
                    if (_rows.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(RowsChangedArgs);
                    break;
                case "Spacing":
                    if (_spacing.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SpacingChangedArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "Columns" => _columns.HasLocalValue,
        "Rows" => _rows.HasLocalValue,
        "Spacing" => _spacing.HasLocalValue,
        _ => base.IsStyleLocal(property)
    };
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Columns":
                if (_columns.ClearLocal())
                    NotifyPropertyChanged(ColumnsChangedArgs);
                return;
            case "Rows":
                if (_rows.ClearLocal())
                    NotifyPropertyChanged(RowsChangedArgs);
                return;
            case "Spacing":
                if (_spacing.ClearLocal())
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
        if (_columns.BeginStyle(0))
            NotifyPropertyChanged(ColumnsChangedArgs);
        if (_rows.BeginStyle(0))
            NotifyPropertyChanged(RowsChangedArgs);
        if (_spacing.BeginStyle(4f))
            NotifyPropertyChanged(SpacingChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_columns.CommitStyle())
            NotifyPropertyChanged(ColumnsChangedArgs);
        if (_rows.CommitStyle())
            NotifyPropertyChanged(RowsChangedArgs);
        if (_spacing.CommitStyle())
            NotifyPropertyChanged(SpacingChangedArgs);
    }
}

using System;

namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>
/// Describes a control's zero-based position and positive spans within a grid.
/// </summary>
public readonly record struct GridCell
{
    /// <summary>
    /// Creates a placement. Bounds are checked against the rows and columns during layout.
    /// </summary>
    public GridCell(int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        if (row < 0)
            throw new ArgumentOutOfRangeException(nameof(row));
        if (column < 0)
            throw new ArgumentOutOfRangeException(nameof(column));
        if (rowSpan < 1)
            throw new ArgumentOutOfRangeException(nameof(rowSpan));
        if (columnSpan < 1)
            throw new ArgumentOutOfRangeException(nameof(columnSpan));
        Row = row;
        Column = column;
        RowSpan = rowSpan;
        ColumnSpan = columnSpan;
    }

    /// <summary>
    /// Gets the starting row.
    /// </summary>
    public int Row { get; }
    /// <summary>
    /// Gets the starting column.
    /// </summary>
    public int Column { get; }
    /// <summary>
    /// Gets the number of rows occupied by the control.
    /// </summary>
    public int RowSpan { get; }
    /// <summary>
    /// Gets the number of columns occupied by the control.
    /// </summary>
    public int ColumnSpan { get; }
}

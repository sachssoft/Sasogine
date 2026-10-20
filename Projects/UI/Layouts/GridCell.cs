namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Stores placement in the owning grid, never on the child control.</summary>
internal readonly record struct GridCell(int Row, int Column, int RowSpan, int ColumnSpan);

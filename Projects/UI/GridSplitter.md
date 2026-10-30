# GridSplitter

Place a splitter in a dedicated cell between the two rows or columns it resizes.
`Direction = Horizontal` changes column widths; `Vertical` changes row heights.
The splitter must be a direct child of the grid. It may span cells across the other axis.

```csharp
var grid = new Grid();
grid.Columns.Add(new GridColumn(240));
grid.Columns.Add(new GridColumn(6));
grid.Columns.Add(new GridColumn(GridLength.Full));

var splitter = new GridSplitter
{
    Direction = ControlOrientation.Horizontal,
    MinimumPaneSize = 80
};
grid.Cells.Put(splitter, row: 0, column: 1);
grid.Children.Add(splitter);
// Place application content in columns 0 and 2.
```

Drag with mouse or touch. Arrow keys resize along the configured axis by
`KeyboardIncrement` logical pixels; Shift multiplies the increment by ten.
Escape, pointer cancellation, capture loss, and detachment cancel a pointer edit.
Outside changes to the edited sizing rules are retained.

Grid dimension minimum and maximum sizes and `MinimumPaneSize` bound interactive
resizing. Hidden, zero-sized panes cannot start a resize. Automatic sizes become
fixed pixel sizes. When both panes are proportional, their combined weight is
redistributed; when only one is proportional, it continues filling the remaining
space. Styling uses inherited control properties such as background, text color,
padding, and border. `UIRegistry` registers the control automatically.

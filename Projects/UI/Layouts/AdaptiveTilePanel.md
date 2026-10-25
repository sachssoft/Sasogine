# AdaptiveTilePanel

Namespace: `Sachssoft.Engine.Controls.Layouts`.

Uniform tiles adapt to the available width (horizontal orientation) or height (vertical orientation). Tile count follows the preferred size; remaining space is distributed evenly across the available slots. The incomplete final line uses the same tile size.

```csharp
var tiles = new AdaptiveTilePanel
{
    PreferredTileSize = new Size2(128, 96),
    MinTileSize = new Size2(48, 36),
    Spacing = 8,
    PreserveAspectRatio = true,
    Orientation = StackOrientation.Horizontal
};
tiles.Children.Add(new Button { Content = "Asset A" });
tiles.Children.Add(new Button { Content = "Asset B" });
var viewer = new ScrollViewer
{
    Content = tiles,
    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
};
```

| Property | Default | Behavior |
|---|---|---|
| `PreferredTileSize` | `128 × 128` | Preferred size and source aspect ratio. |
| `MinTileSize` | `16 × 16` | Both dimensions remain minimums, including with aspect preservation. |
| `Orientation` | `Horizontal` | Fills rows; `Vertical` fills columns. |
| `Spacing` | `8` | Gaps between slots, without an additional outer gap. |
| `PreserveAspectRatio` | `true` | Preserves the preferred ratio. When false, the cross-axis retains its preferred or minimum dimension. |
| `ActualTileSize` | `0 × 0` | Read-only slot size from the last arrangement; zero when empty. |

All configurable properties support central styles, local overrides and `ResetStyleProperty`. The built-in registry includes the panel automatically. Layout remains cached until constraints, children, visibility or relevant properties change. The panel keeps layout styling but draws no background, border or focus decoration.

Hidden children consume no slots or gaps. With an unbounded wrapping axis, visible children form one line using the preferred size raised as necessary to satisfy minimum dimensions. This also works when both measurement axes are unbounded. A viewport smaller than the minimum tile size can overflow; use a `ScrollViewer` with the required scroll axes enabled.

Children retain their own explicit sizes, alignment and margins within each tile slot. `ActualTileSize` describes the slot rather than the displayed child bounds. The panel lays out all children; it does not provide item virtualization. With fewer children than available slots, tiles retain their normal full-row size instead of stretching individual final-row items.

For vertical scrolling, disable the horizontal scroll axis with `HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden` so tiles wrap against the viewport width. For a vertically oriented panel with horizontal scrolling, disable the vertical scroll axis instead. Enabling both scroll axes measures both axes without a limit and therefore uses the natural single-line layout.

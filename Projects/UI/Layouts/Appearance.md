# Layout appearance

`Panel`, `Grid`, `StackPanel`, `WrapPanel`, `TiledPanel`, `AdaptiveTilePanel` and `EdgePanel` retain their styles and local overrides, including padding, spacing, alignment and sizing. Font settings still pass to their children.

Pure layouts do not draw their own background, border or focus decoration, even when a style or local property supplies those values. Children, clipping, input and cached layout continue to work normally.

`ToolBar`, `ToolStrip` and `StatusBar` remain visual controls and retain their decoration. Custom controls derived from a layout can restore default decoration by overriding `DrawsDecoration` to return `true`. An explicit `OnDraw` override remains available for custom drawing.

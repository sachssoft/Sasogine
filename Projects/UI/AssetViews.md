# Asset views, references and render-target colors

`AssetView : ListBox` reuses selection, keyboard navigation, scrolling and activation. `ViewMode` switches between tiles and rows. Tile columns use the same calculation as `TiledPanel`; only visible items are drawn and request previews. `TileSize`, `Spacing` and `ViewMode` support string-based styles and local overrides.

```csharp
var view = new AssetView { Source = entries, MultiSelect = true };
view.FilterText = "texture";
view.ViewMode = AssetViewMode.Tiles;
```

An `AssetItem` describes an `IEngineReferenceable`, display name, kind and optional `Func<AssetPreview?>`. It never resolves a resource during metadata filtering. Preview factories run on the UI thread during visible updates, never in Draw. Results, missing previews and errors are cached per entry and shared between views. Call `InvalidatePreview()` after a resource changes. Textures and source objects remain caller-owned. `AssetItem.FromTexture(textureAsset)` supplies a lazy texture-asset preview. For expensive resources, supply a prebuilt thumbnail or a factory that returns an already prepared GPU texture.

`Source` is a metadata snapshot; replace it or call `Refresh()` after mutating an existing source list. Selection uses list indices; refresh preserves the primary identifier when it remains visible. Use the inherited `SelectedIndices` for multiple selections. Preview caches stay with their entries.

## Reference picker

```csharp
var picker = inspector.AddReference<Texture2DAsset>("Texture",
    () => definition.Texture, value => definition.Texture = value,
    entries, definition, nameof(definition.Texture));
```

`ReferencePicker<T>` uses `asset.Target is T` and an optional additional filter. No reflection or per-target editor registration is required. Browse opens an `AssetPickerDialog` with the same AssetView, restricted to compatible entries and single selection. Search, double-click and Enter are supported. Cancellation preserves the old identifier. Drag a tile onto a compatible reference field to select it directly. User changes replace `Reference<T>` rather than mutating a shared reference instance. Missing IDs remain visible; `AllowClear = false` prevents user clearing. The picker also notices external changes to the mutable `Reference<T>.Id`. Generic pickers can be constructed directly; the non-generic picker and asset controls are included in the automatic UI registry.

## Render-target pipette

```csharp
var source = new RenderTargetColorSource(target, viewportBounds);
workspace.ColorPickSource = source;
// Update source.Viewport when the displayed image moves or resizes.
// Optionally set source.Source to an atlas/image region.
```

Color dialogs automatically expose **Pick pixel** when a workspace source is available. `ColorPickerDialog.PickSource` can override it. The workspace session intercepts input, coalesces pointer previews, accepts with a click and rolls back on Escape, right-click, pointer cancellation or deactivation. The color dialog remains open for final OK/Cancel. Original alpha is retained by default; `workspace.ColorPicking.PreserveAlpha` controls this behavior.

The concrete source reads a 9×9 neighborhood from an unbound `SurfaceFormat.Color` target. Logical viewport coordinates map to texture pixels and optional source regions. Premultiplied RGB is converted to straight RGB by default; set `UnpremultiplyAlpha = false` for straight-alpha content. The source samples the target's stored color space, before any later presentation transforms. The caller owns the target and must keep it valid throughout the gesture. HDR targets require a separate `IColorPickSource` adapter.

When sampling the UI's own render target, render it with `workspace.Draw(context, includePickingOverlay: false)`, unbind and present it, then call `workspace.DrawColorPickingOverlay(context)`. This excludes the pipette cursor and magnifier from readback. The LogoInspector example demonstrates this path, including DPI scaling and target replacement on resize. There is no screen capture or platform-specific desktop sampling.

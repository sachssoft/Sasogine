# Items, templates and editable canvas content

`ItemsControl` presents arbitrary data using explicit factories. It has no dependency on TexturePacker, atlases or a particular model type.

```csharp
var items = new ItemsControl
{
    ItemsSource = models,
    ItemTemplate = new DelegateTemplate<object?, Control>(source =>
        new CanvasItem(source)
        {
            Position = new Point2(40, 60),
            Size = new Size2(180, 100),
            Content = new TextBlock { Text = source?.ToString() ?? string.Empty }
        }),
    ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(
        static _ => new Canvas())
};
```

The default items panel is a vertical `StackPanel`. A panel factory must create an empty, unowned container. Changing the panel transfers existing presentations. Do not add unrelated children to `ItemsPanelRoot`.

Use `Items` for direct data or controls, or `ItemsSource` for any `IEnumerable`. The two modes are mutually exclusive. An `INotifyCollectionChanged` source updates automatically while attached. Call `RefreshItems()` for a source without change notifications. Sources and controls are confined to the UI thread.

Items are synchronized before layout. With a regular panel, Add, Remove, Replace, Move and Reset reuse unchanged presentations by source identity, including repeated references and null items. Item templates must return fresh, unowned controls. A template change recreates presentations. The presentation root receives the source as its observable `DataContext`.

To virtualize data items, choose `VirtualStackPanel`, `VirtualTilePanel`, or `VirtualAdaptiveTilePanel` as `ItemsPanel`. The items host creates a `ScrollViewer` and exposes it through `ItemsScrollViewer`. It keeps containers for the visible range and the configured row buffer. `TryContainerFromIndex` returns null for unrealized items. Indexed sources such as arrays and `ObservableCollection<T>` avoid creating a source snapshot.

```csharp
var ranking = new ItemsControl
{
    ItemsSource = entries,
    ItemsPanel = new DelegateTemplate<ItemsControl, ContainerControl>(
        static _ => new VirtualStackPanel { ItemHeight = 60, Spacing = 8 }),
    ItemTemplate = new RecyclingItemTemplate<TextBlock>(
        static () => new TextBlock(),
        static (row, item) => row.Text = item?.ToString() ?? string.Empty)
};
```

Use a `RecyclingItemTemplate` when supplying a custom template to a virtual panel. Update item-dependent state in the rebind callback or observe the presentation root's `DataContext`. With no custom template the default container updates its content. Keep selection, commands and persistent state on the data model. Recycled presentations can display different items as the viewport moves.

## Data-driven controls

* `ListBox` accepts `ItemsSource`, `ItemTextSelector`, `ItemTemplate`, and `ItemsPanel`. `SelectedValue` is the bindable data value. In a custom template, observe the presentation's `IsItemSelected` state. Existing direct `SelectorItem` lists keep their lightweight renderer unless a panel or template is supplied.
* `AssetView` uses the same list host for bound assets. Its default panel switches between a virtual stack and adaptive tiles, and only realized previews are requested.
* `ComboBox` shares the bound source and template with its popup `ListBox`. `SelectedContentTemplate` and `SelectedTextTemplate` configure the closed display.
* `TreeView` uses `NodeTemplate` to turn source entries into tree nodes. `NodeContentTemplate` optionally recycles the visual content of visible rows. The tree retains expansion, indentation and selection.
* `DataTable` accepts a row `ItemsSource`. `DataTableColumn.CellTemplate` optionally recycles visible cell display controls. Column accessors continue to own sorting, editing and text export.
* `TabControl` supports `HeaderTemplate` and `ContentTemplate`; `Carousel` supports `PageTemplate`. These factories receive each data item and keep the controls' existing page ownership and transition behavior.
* `MenuBar` and `ContextMenu` support `ItemsSource` and an explicit `ItemTemplate` producing a `MenuEntry`, including command items and separators.

`CanvasItem` hosts arbitrary control content and exposes the existing engine transform interfaces. Position and Size change layout; Scale transforms the laid-out content. Rotation is measured in radians. RotationPivot is normalized, with `(0.5, 0.5)` at the center; pivots outside the item are supported.

The canvas supports one active selection and pointer capture:

* Drag the item body to move it.
* Drag an edge or corner to resize its layout size.
* Hold Alt while dragging an edge or corner to change Scale instead.
* Drag the upper handle to rotate around the pivot.
* Drag the pivot cross to move the pivot without moving the visible content.
* Hold Shift to preserve proportions or snap rotation to 15-degree increments.
* Press Escape, lose capture or deactivate to cancel and restore the starting transform.

`IsLocked` and the individual Allow properties restrict editor gestures. Disable `Canvas.IsEditingEnabled` to interact with hosted controls normally, including transformed buttons. `TransformChanged` uses `SelectionTransformState` for Started, Changed, Completed and Cancelled notifications, allowing the application to create one undo transaction per gesture. Its initial `Transform2State` is a snapshot: recipients must not call `UpdateState()` on it.

Factory-created controls own their normal control lifecycle. If a custom template subscribes to a model or owns bindings, unsubscribe in its detach lifecycle. The items host releases its external collection subscription on detach and resynchronizes on attachment.

## Rendering

The default frame-batch renderer preserves exact rotated ancestor and content clips while interpolating texture coordinates and colors. Convex clipping reuses context storage and adds no steady-state managed allocations. The explicit legacy SpriteBatch compatibility path retains its rectangular scissor behavior. Transformed SceneViewport content is rendered into a reusable scene target and composed through the same affine texture path; ordinary untransformed scene viewports keep direct rendering.

ScriptTextBox also supports JavaScript module keywords, line/block comments, quoted strings and multiline template literals through ScriptLanguage.JavaScript.

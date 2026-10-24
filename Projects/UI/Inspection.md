# Inspector controls

Namespace: `Sachssoft.Engine.Controls.Inspection`.

The default `InspectorView` style stretches its content horizontally and anchors it at the top. Collapsing or expanding groups preserves the full available row width, including nested groups and changes in scrollbar visibility.

`InspectorView` is a reusable content host for explicitly described properties. `AddGroup` creates an `InspectorGroup`, which uses the existing `Expander`. Further calls on `group.Fields` produce nested definition groups. Place an inspector inside a `ScrollViewer` when necessary.

```csharp
var transform = inspector.AddGroup("Transform").Fields;
transform.AddSingle("Scale", () => model.Scale, value => model.Scale = value,
    0.5f, 2, 0.01f, model, nameof(model.Scale));
```

`AddText`, `AddBoolean`, `AddSingle` and `AddColor` create ordinary styled controls. `AddField` accepts a custom editor. Existing base styles, local overrides and skins remain applicable; no separate control definitions or property registry are introduced.

## Composite numeric values

`AddVector2`, `AddVector3` and `AddVector4` create a single property row containing X/Y, X/Y/Z or X/Y/Z/W fields. Position, scale and three-dimensional rotation therefore remain grouped rather than becoming separate inspector properties. Each field retains the existing drag, text entry and repeat-arrow behavior.

```csharp
transform.AddVector3("Position", () => model.Position, value => model.Position = value,
    -1000, 1000, 1, model, nameof(model.Position));
```

`MultiValueEditor<T>` holds the complete struct value. Editing one component writes one complete value through the supplied setter and preserves sibling components. It reads the latest source value at the time of an edit, so sibling changes arriving before the next binding pass are retained. Updating values does not replace existing field controls.

`AddValues<T>` and `ValueComponent<T>` describe other structs explicitly, including custom component names and individual ranges. Component editors use `float`; use an appropriate different numeric editor for values requiring integer or double precision beyond Single. No members, enum fields or types are discovered at runtime.

The component row inherits `Grid` styling. Central styles `Inspector.MultiValue` and `Inspector.Component` control spacing and background, while the numeric fields retain `ValueTextBox` styling. There is no additional property registry.

When a source implements `INotifyPropertyChanged`, supply the source and property name to avoid repeatedly reading unchanged values. Without a notification source, the explicitly supplied getter is polled. Bindings follow workspace attachment and detachment and run through `Workspace.Bindings` before layout and after routed input. `Clear()` releases field bindings. Use these APIs on the workspace thread.

`InspectorCollection<T>` accepts a `TrackableCollection<T>`, an item factory, a label getter and a delegate describing selected-item properties. It uses the collection's `Add`, `RemoveAt` and `Move` methods and does not consume its change records. Only selected-item editors are created. Moving an item retains selection identity; changing its values retains existing field controls. Structure and label changes refresh the item list. This implementation targets compact property collections, rather than virtualized asset catalogs.

The built-in registry includes `InspectorView`. Generic collection controls require explicit construction with their data source and factory. There is no runtime type discovery. A working entity/definition example is provided in `Examples/Advanced.08.LogoInspector`.

The complete default value catalog is documented in [Inspection.ValueTypes.md](Inspection.ValueTypes.md). `inspector.Add(title, read, write, source, propertyName)` selects a factory using the explicitly bound value type. It supports precise numeric composites, grouped bounds/boxes/insets and custom typed registration.

`LabelInlineMinWidth` sets the minimum available property-row width for placing the label beside its editor (default: `420` logical pixels). Below it, the label appears above the editor; `0` always keeps labels inline. For example: `inspector.LabelInlineMinWidth = 300;`. Nested inspectors inherit the value unless overridden locally or through a style. Resetting a local override restores its style or inherited value. Long label text may still wrap within the label column.

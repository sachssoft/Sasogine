# Inspector property menus

Every `InspectorView.Add...` field has a trailing ellipsis button. One menu applies to the entire typed property, including grouped vectors, matrix rows, bounds and insets. The public editor return types remain unchanged. Arbitrary `AddField` controls also receive a menu, whose typed operations remain disabled unless configured explicitly.

```csharp
var position = inspector.Add("Position", () => model.Position, value => model.Position = value);
inspector.SetDefault(position, Vector3.Zero);

// Override reset for a style-backed property instead of assigning a hard-coded default.
inspector.GetPropertyMenu(position).ResetAction =
    () => control.ResetStyleProperty(nameof(Control.Padding));
```

Reset uses an explicit typed default, or the initial snapshot captured when the field was created. No constructor, property or attribute discovery is performed. For reusable definition editors, set defaults explicitly rather than relying on the current model value. The initial snapshot of a custom mutable reference type is shallow; use an explicit ResetAction when deep restoration is required. Engine `Reference<T>` fields snapshot their IDs and create new references on reset and paste.

Copy/Paste uses `UIRegistry.InspectorClipboard`, shared by inspectors using that registry. Only exact generic type identities match; values are not serialized, parsed or coerced. This clipboard is independent of the optional platform text clipboard. Structs are boxed only during an explicit Copy action. Availability is evaluated when the menu opens or executes, not by per-frame reflection or polling.

Use `GetPropertyMenu(editor).Menu.Items` for explicit application-specific commands. `ResetAction` can remove a local style override. Built-in actions check ownership so a removed field cannot modify its old model; menus close and bindings detach on Clear. There is no automatic global undo stack or source-member discovery.

`inspector.ShowPropertyMenus = false` hides the trailing buttons, closes their open popups and removes the complete menu column including its gap. Nested inspector groups inherit the setting unless their own property was explicitly assigned. New fields inherit the current visibility. The example header checkbox **Property menus** demonstrates switching it at runtime across all tabs.

Inspector menu buttons and popups use solid rendering without atlas backgrounds. Other menus retain their normal skin; `MenuBar.UseSkin = false` provides the same solid rendering for an application-specific menu.

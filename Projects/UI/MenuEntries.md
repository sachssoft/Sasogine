# Menu entries and separators

`MenuBar.Items`, `ContextMenu.Items` and `MenuItem.Items` contain `MenuEntry` objects. Use `MenuItem` for commands and submenus, and `MenuSeparator` for non-interactive dividers.

```csharp
var menu = new ContextMenu();
menu.Items.Add(new MenuItem { Text = "Open", Action = OpenDocument });
menu.Items.Add(new MenuSeparator());
menu.Items.Add(new MenuItem { Text = "Close", Action = CloseDocument });
```

`MenuItem.IsSeparator` has been removed. Replace `new MenuItem { IsSeparator = true }` with `new MenuSeparator()`.

Separators have no command, shortcut, checkbox or submenu properties. Pointer activation and keyboard navigation ignore them. Popup menus draw horizontal lines; horizontal menu bars draw vertical dividers. Each separator reserves eight logical pixels on the menu's layout axis. The containing menu supplies the skin's existing `Separator` part, or its border color when `UseSkin` is false or the skin provides no separator part.

The common collection type is now `IList<MenuEntry>`. Keep a reference to a specific `MenuItem` when changing its command properties, or use a type check when inspecting a mixed collection. These entries are menu models derived from `Element`, rather than standalone controls; their containing menu handles layout and rendering.

# Toolbar, Toolbox und Statusbar

```csharp
var toolbar = new ToolBar();
var tools = new ToolStrip();
toolbar.Children.Add(tools);
tools.Children.Add(new Button { Content = new TextBlock { Text = "Save" } });
tools.Children.Add(new TextBox { Width = 160 });

var toolbox = new ToolBox { Columns = 3 };
toolbox.Children.Add(new ToggleButton());
toolbox.Children.Add(new SelectorButton());

var statusbar = new StatusBar();
statusbar.Children.Add(new StatusStrip
{
    Content = new TextBlock { Text = "Ready" }
});
```

- **ToolBar → ToolStrip → Controls:** Die Toolbar bricht zwischen Gruppen um. Eine Gruppe, die selbst zu breit ist, kann ihre Controls ebenfalls umbrechen. `Spacing` und `LineSpacing` steuern die Abstände.
- **ToolBox → ButtonBase:** Unterstützt auch ToggleButton, SelectorButton, RadioButton, CheckBox und RepeatButton. Andere Controls werden mit `ArgumentException` abgewiesen. Alle Zellen richten sich nach dem größten sichtbaren Button.
- **StatusBar → StatusStrip → Content:** Eine horizontale Leiste mit beliebigem Inhalt pro Strip, auch Bildern oder interaktiven Controls.

ToolBox-Anordnung:

| Einstellung | Verhalten |
|---|---|
| `Columns = 3` | Drei Spalten, zeilenweise befüllt. |
| `Columns = 0; Rows = 2` | Zwei Zeilen, spaltenweise befüllt. |
| `Columns = 0; Rows = 0` | Automatischer Umbruch nach verfügbarer Breite. |

Wenn beide Werte positiv sind, hat `Columns` Vorrang. Unsichtbare Controls belegen keinen Platz. Fenstergrößen- und Inhaltsänderungen invalidieren den Layoutcache; unveränderte Updates verwenden ihn weiter.

Alle fünf Controls sind automatisch registriert, verwenden zentrale Styles und die vorhandenen Skin-Atlanten. `ICommand`, `CommandParameter`, Tooltips und die sofortige Besitzprüfung bleiben verfügbar. `Add(null)` und doppelte Besitzer werfen Exceptions.

Die Controls-Galerie zeigt alles unter **Bars & toolbox** (`--page bars`). Im Editor werden die neuen Toolbar- und Statusbar-Controls ebenfalls verwendet.

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

## StatusStrip lengths

`StatusStrip.Length` supports `GridLength.Auto` (default), fixed logical pixels and proportional remaining space. Hidden strips occupy neither a slot nor a gap. Length changes invalidate layout; unchanged frames retain their layout cache. Vertical orientation applies the same rules to height. Under unbounded measurement proportional strips use content size. If fixed and automatic slots exhaust the available space, proportional slots receive zero space; fixed slots retain their requested size.

```csharp
statusBar.Add(new StatusStrip { Content = "Ready" }, GridLength.Auto);
statusBar.Add(new StatusStrip { Content = "Document" }, GridLength.Full);
statusBar.Add(new StatusStrip { Content = "FPS: 60" }, 100);
// GridLength.Proportion(2) receives twice the share of GridLength.Full.
```

`Length` is also a central style property (`Auto`, `100`, `*`, `2*`). Local assignments override styles; `ResetStyleProperty(nameof(StatusStrip.Length))` restores the style value. The `Add(strip, length)` overload validates ownership before assigning the length.

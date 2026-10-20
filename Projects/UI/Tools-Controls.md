# Dialoge, Werkzeuge und Script-Editor

Die Registry registriert alle neuen Controls automatisch. Sie verwenden direkte Properties, `INotifyPropertyChanged`, String-Styles und den vorhandenen Atlas-Skin. Reflection wird nicht benötigt.

| Control | Verwendung |
| --- | --- |
| `OpenFileDialog`, `FolderSelectorDialog` | Favoriten links oben, Laufwerke darunter; beide Listen scrollen separat. |
| `DropDownButton` | Beliebiger Button-Inhalt mit `ContextMenu` als `Menu`. |
| `SplitButton` | Hauptaktion mit `Command` und `CommandParameter`, daneben ein unabhängiger Menübutton. |
| `GroupBox` | Beliebige Controls als `Header` und `Content`. |
| `Expander` | `HeaderContent`, `Content`, `IsExpanded`; eingeklappter Inhalt nimmt nicht am Control-Baum teil. |
| `ValueTextBox` | Repeat-Pfeile links/rechts; gemeinsamer Bereich wechselt zwischen Balken und Texteingabe. `Minimum`, `Maximum`, `Step`, `Value` und Mausrad. |
| `Chart` | Linien, Balken oder Stufen über `ChartKind`; feste oder automatische Y-Skalierung. |
| `ScriptTextBox` | Editierbarer Text mit Syntaxfarben für Lua, Python, C# und Klartext. |

## Live-Diagramm

```csharp
var frameTimes = new ChartSeries(240) { Name = "Frame time (ms)" };
var chart = new Chart { AutoScale = false, Minimum = 0, Maximum = 50 };
chart.Series.Add(frameTimes);

// Update: aktuelle Framezeit übernehmen.
frameTimes.Append((float)elapsed.TotalMilliseconds);
```

`ChartSeries` behält die letzten Werte in einem festen Ringpuffer. Datenänderungen verwerfen den Geometriecache, ohne das Layout neu zu berechnen. Nach dem Aufwärmen benötigen Append und Update keine zusätzlichen verwalteten Allokationen. X entspricht der Reihenfolge der Messwerte; die Y-Skala ist für alle Serien gemeinsam. Bei fester Skalierung werden Werte außerhalb des Bereichs abgeschnitten.

## SplitButton und Zahlen

```csharp
var export = new SplitButton
{
    Content = new TextBlock { Text = "Export" },
    Command = exportCommand,
    CommandParameter = document,
    Menu = exportMenu
};

var sensitivity = new ValueTextBox
{
    Minimum = 0, Maximum = 2, Step = 0.05f, Value = 1
};
```

`export.PrimaryButton.IsPressed` lässt sich nach `Workspace.Update` abfragen. Ein nicht ausführbarer Hauptcommand sperrt den Menübutton nicht. Bei `ValueTextBox` öffnet ein Klick das Textfeld; Ziehen verändert den Wert und bleibt im Balkenmodus. Enter übernimmt gültigen Text, Escape verwirft ihn. Fokusverlust zeigt wieder den Balken und verwirft ungültigen Text. Details und Style-IDs: [ValueTextBox](ValueTextBox.md).

## Favoriten und virtuelle Dateisysteme

```csharp
var dialog = new OpenFileDialog();
dialog.Favorites.Add(new FileDialogLocation("Projects", projectDirectory));
await dialog.ShowAsync(root);
```

`Favorites` und `Drives` lassen sich vor dem Öffnen bearbeiten. Während ein Dialog offen ist, aktualisiert `RefreshLocations()` die Listen. Für virtuelle `IFileDialogProvider` kann `UseSystemDrives = false` gesetzt werden. Die Navigation verwendet auch bei Favoriten den konfigurierten Provider.

## Script-Text

```csharp
var editor = new ScriptTextBox
{
    Language = ScriptLanguage.Lua,
    Text = "local speed = 1.25"
};
```

Tokens und Textpositionen werden bei Text-, Sprach- oder Fontänderungen vorbereitet. Farben lassen sich über `KeywordColor`, `StringColor`, `CommentColor` und `NumberColor` stylen. Auswahl, Zwischenablage, Undo/Redo, Tab-Eingabe und Scrollen stammen aus `TextBox`. Die Hervorhebung ist ein Lexer für übliche Sprachkonstrukte, kein Compiler oder Sprachserver; Autovervollständigung und Script-Ausführung sind nicht enthalten.

Eigene Sprachen können über `ISyntaxHighlighter` direkt zugewiesen werden. Details und ein Minimalbeispiel: [Eigene Syntax-Hervorhebung](Syntax-Highlighting.md).

Die zusätzliche Galerie-Seite **Tools & scripts** zeigt alle neuen Bausteine. Die Sprachauswahl enthält auch **Custom script**. Startparameter: `--page tools`.

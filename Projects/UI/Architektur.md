# Controls: direkte Properties, Styles und deterministische Frames

Die Controls verwenden keine separaten Konfigurationsobjekte mehr. Ein Control wird direkt aufgebaut: `new Button { Content = new TextBlock { Text = "Start" }, IsEnabled = true }`. Alte Aufrufe mit Konfigurationsargumenten und der alte Workspace-Typ sind inkompatibel mit dieser Architektur.

## Grundstruktur

- `Element` implementiert `INotifyPropertyChanged` und `SetNotify` mit wiederverwendeten `PropertyChangedEventArgs`.
- `Control` ergänzt Layout, Fokus, Eingabe, Zeichnen und zentrale Styles. Alle konkreten Controls erben davon.
- `StyleProperty<T>` speichert den effektiven Wert und den zuletzt übernommenen Stylewert. Getter lesen nur ein Feld. Keine Property-Metadatenbank und keine Dictionaries pro Control.
- `ButtonBase` enthält Aktivierung und Commands; Button, ToggleButton und RepeatButton bauen darauf auf.
- `TextBlock` ist die Textdarstellung mit Umbruch/Ausrichtung. `Label` ist eine einfache Spezialisierung.
- `Workspace` besitzt Eingaberouting, Fokus, Bindings und eine automatisch initialisierte Factory-/Style-Sammlung.

## Namespaces

| Namespace | Inhalt |
|---|---|
| `Sachssoft.Engine.Controls` | Element, Control, Workspace und konkrete Controls |
| `.Styling` | Style, StyleProperty, Stylesammlung, Skin und explizite Converter |
| `.Layouts` | Grid, StackPanel, WrapPanel, TiledPanel und Layouttypen |
| `.Input` | Eingabesampler, Frames und Pointer |
| `.Drawing` | Zeichen-/Layoutkontext und Fontadapter |
| `.Binding` | Explizite typisierte MVVM-Bindings |

## Workspace im Host

Workspace und Controlbaum werden in `Initialize` erstellt. GPU-Ressourcen wie Skin und Font kommen anschließend in `LoadContent`, vor dem ersten Update. Alle drei ausführbaren Beispiele zeigen diesen Ablauf.

```csharp
_button = new Button { Content = new TextBlock { Text = "Start" }, StyleId = "Primary" };
_workspace = new Workspace(_button);
_workspace.Styles.Register(new Style("Primary", "Button") { ["Padding"] = "12,8" });
// Nach dem Laden der Textur, vor dem ersten Update:
_workspace.SetSkin(skin);
```

Im Update werden Geräte genau einmal gelesen. `SampleReusable` liefert einen geliehenen Frame, der sofort verarbeitet wird:

```csharp
_workspace.Update(_sampler.SampleReusable(mouse, keyboard, touches,
    viewport, scale, elapsed, isActive), _layout);
if (_button.IsPressed)
    StartGame();
```

Im Draw folgt `_workspace.Draw(_draw)`. Layout und Zeichnen verwenden denselben Font, Viewport und Scale. Draw verändert keine Properties, Eingaben oder Styles.

## Button-Zustände

`IsPressed` bedeutet: in diesem Update erfolgreich aktiviert. `IsHeld` beschreibt den gehaltenen Zustand; `IsReleased` die Freigabe im aktuellen Update. `IsHovered` beschreibt den aktuellen Hover-Zustand. Frame-Pulse werden beim nächsten Workspace-Update zurückgesetzt. Events und ICommand mit CommandParameter bleiben verfügbar; Eingabe wird ohne versteckte Timer übergeben.

## Style oder lokaler Wert

Ohne lokale Zuweisung gilt der Stylewert. Jede ausdrückliche Zuweisung ist ein lokaler Override, auch `false`, `0` oder ein anderer Wert, der zufällig dem Standard entspricht. `button.ResetStyleProperty(nameof(Button.Padding))` entfernt diesen Override. Das ist ein expliziter Switch am jeweiligen Control, keine dynamische Property-Registry.

Getter machen keinen Style-Lookup. Styleänderungen werden am Frame-Anfang angewendet. Jede Klasse verarbeitet ihre eigenen Schlüssel in `ApplyStyle`, mit Aufruf von `base.ApplyStyle`. Eine Styletransaktion meldet nur den finalen effektiven Wert, ohne Zwischenwerte aus der Vererbungskette.

## Grid

Platzierung gehört ausschließlich dem Grid:

```csharp
var grid = new Grid();
grid.Rows.Add(new GridRow { Height = GridLength.Auto });
grid.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
grid.Children.Add(_button);
grid.Cells.Put(_button, row: 0, column: 0);
```

Es gibt keine angehängten Zeilen-/Spaltenfelder an Controls. `Children` verwaltet die Controls, `Cells` ausschließlich deren Platzierung. `Cells.Put(control, ...)` legt eine Zuordnung an oder verschiebt eine bestehende. Gleiche Werte lösen keine Cache-Invalidierung aus. `Cells.Remove(control)` entfernt nur die Zuordnung; `Children.Remove(control)` entfernt auch das Control. Weitere Beispiele stehen in [Grid.md](Grid.md).

## Control Tree und Besitz

Ein Control hat genau einen `Parent`. `Children.Add`, `Content` und vergleichbare Hosts reservieren diesen Besitzer sofort, auch vor dem ersten Update. Doppelte Besitzer und Zyklen werfen `InvalidOperationException`, ohne den bisherigen Inhalt zu verändern. Ein identisches `Content` erneut zuzuweisen ist erlaubt; ein doppeltes `Children.Add` nicht. Null ist bei optionalem `Content` erlaubt und gibt den Besitz frei, bei `Children.Add` nicht.

Während eines Updates bleiben die für die Traversierung verwendeten Children bis zur sicheren Layout-/Frame-Grenze stabil. `Parent` zeigt bereits den reservierten Besitzer. Ausgeblendete Tab-Seiten und eingeklappter Inhalt behalten ihren Besitzer; nur ihre aktive Traversierung und Workspace-Verbindung werden ausgesetzt. Tooltips verwenden einen stabilen Presenter, Popups und Dialoge werden bereits beim Anfordern reserviert. Beim Zeichnen sind Baumänderungen verboten.

```csharp
first.Content = image;
second.Content = image; // InvalidOperationException: bereits reserviert.

first.Content = null;
second.Content = image; // Erlaubter Besitzerwechsel.
```

## MVVM ohne Reflection

`PropertyBinding<TSource,T>` verwendet Getter-/Setter-Delegates. Das Modell implementiert `INotifyPropertyChanged`; für Zweiwegbindung werden die Rückrichtung-Delegates mitgegeben. Benachrichtigungen markieren die Bindung; Workspace verarbeitet sie geordnet im Update. Werden gleichzeitig Modell und Ziel geändert, hat die Modelländerung Vorrang.

```csharp
var binding = new PropertyBinding<MyModel, string>(
    model, nameof(MyModel.Name), static m => m.Name,
    textBox, nameof(TextBox.Text), value => textBox.Text = value,
    () => textBox.Text, static (m, value) => m.Name = value);
_workspace.Bindings.Add(binding);
```

Beim Entfernen eines Screens die Binding-Instanz aus `Bindings` entfernen und `Dispose` aufrufen. Alle UI-/Modelländerungen gehören auf denselben UI-Thread; für Hintergrundresultate gibt es `Workspace.Post`.

## Eigene Controls

Normales `new MyControl()` benötigt keine Registrierung. Optional kann eine explizite Factory über `root.Registry.Register("MyControl", static () => new MyControl())` ergänzt werden. Standardcontrols sind automatisch registriert. Keine Reflection, Expression-Kompilierung oder Laufzeit-Codegenerierung.

## Performance und Prüfung

Messung und Anordnung werden mit Größen-/Fontschlüsseln gecacht. Eine Änderung invalidiert das betroffene Element und seine Vorfahren; unveränderte Geschwister behalten ihre Messung. Paint-Reihenfolge, Eingaberoute und Eingabepuffer werden wiederverwendet. `Sample` erstellt weiterhin eine eigene Momentaufnahme für Replay; `SampleReusable` und geroutete EventArgs dürfen nicht über den nächsten Aufruf hinaus aufbewahrt werden.

Die Validierung enthält einen prüfbaren Allokationsbenchmark für 100 Buttons über 10.000 wiederverwendete Eingabe-/Update-Frames nach Aufwärmen. Das ist kein Versprechen für sämtliche Controls: Textbearbeitung, neue Sammlungsinhalte und Dokumentaktionen dürfen nach Bedarf Speicher anfordern. Eigene Delegate-/Command-Implementierungen beeinflussen ihre Kosten selbst.

Builds und Prüfprogramm unterstützen .NET 8, 9 und 10. NativeAOT kompiliert die tatsächlichen Bibliotheksquellen mit allen eingebauten Factories. Die ausführbaren DesktopGL-Beispiele prüfen zusätzlich das Rendering.

Zusatzprüfung: 20.000 wechselnde Padding-/Margin-Zuweisungen erzeugen 0 verwaltete Bytes; Insets2 wird dabei mit einem expliziten Vergleich ohne Boxing verarbeitet.

## Button-Inhalte

`ButtonBase` erbt direkt von `ContentControl`. Beschriftungen werden als `TextBlock` in `Content` gesetzt; Buttons besitzen keine eigenen `Text`-, `WrapText`- oder Textausrichtungs-Properties. Das gilt auch für Toggle-, Repeat-, Radio-, Selector- und Checkbox-Controls. `ContentHorizontalAlignment` und `ContentVerticalAlignment` richten den beliebigen Inhalt aus. Textformatierung und Text-Styles gehören zum enthaltenen `TextBlock`.

```csharp
var caption = new TextBlock { Text = "Speichern" };
var button = new Button { Content = caption };
caption.Text = "Gespeichert";
```

Bilder und zusammengesetzte Layouts bleiben beliebiger Content. TextBox, ListBox-Einträge und andere Datenmodelle behalten ihre eigenen fachlichen Textwerte.

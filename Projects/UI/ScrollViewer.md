# ScrollViewer

`ScrollViewer` unterstützt Touch-Wischen direkt über dem Inhalt. Der Inhalt folgt dem Finger innerhalb seiner Scrollgrenzen; horizontales und vertikales Scrollen lassen sich wie bisher über die separaten Scrollbars konfigurieren.

```csharp
var viewer = new ScrollViewer
{
    Content = contentPanel,
    IsSwipeEnabled = true,       // Standard: Touch-Wischen aktiv.
    IsMouseDragEnabled = false, // Optional: auch Ziehen mit der primären Maustaste.
    SwipeThreshold = 12,
    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
};
```

- Erst ab der Wischschwelle übernimmt der Viewer die Pointer-Capture und bricht einen gedrückten Child-Button ab. Kleine Berührungen bleiben normale Klicks. `IsSwiping` meldet den aktiven Zustand auch über `INotifyPropertyChanged`.
- Beide verfügbaren Achsen können während einer Geste gleichzeitig scrollen. Für die Übernahme zählt die vorherrschende Bewegungsrichtung. Ein zweiter Pointer kann eine laufende Geste nicht steuern. Loslassen, Pointer-Abbruch, Deaktivierung, Inhaltswechsel und das Ausschalten von Swipe räumen den Gestenzustand auf.
- Der innere ScrollViewer hat Vorrang, solange er in der Wischrichtung weiter scrollen kann. An seiner Grenze kann der äußere Viewer beim nächsten Move übernehmen; dabei wird nur die neue Bewegung angewendet. Eine begonnene innere Scrollgeste führt im umgebenden Carousel nicht zusätzlich zu einem Seitenwechsel. Auf einer vom Viewer nicht scrollbaren Achse bleibt Carousel-Swipe möglich.
- Scrollbars, Slider und andere `RangeControl`-Editoren, TextBoxen, ValueTextBoxen, Joysticks, Kurven-, Rotations- und Szenen-Controls behalten ihre direkte Bedienung. Der Viewer startet über diesen Controls keine Wischgeste.
- `Hidden` behält seine vorhandene Bedeutung: Die Achse wird für den Inhalt begrenzt und nicht gescrollt. `Auto` scrollt bei Überlauf; `Always` reserviert die Scrollbar auch ohne Überlauf. Mausrad und direkte Scrollbar-Bedienung bleiben erhalten.
- `IsSwipeEnabled`, `IsMouseDragEnabled` und `SwipeThreshold` sind zentral stylebar. Lokale Werte haben Vorrang; `ResetStyleProperty` stellt den aktuellen Stylewert wieder her. Die vorhandene typisierte `Style.SwipeThreshold`-Kurzform hat für Carousel einen Fallback von 24; der ScrollViewer selbst verwendet standardmäßig 12 logische UI-Einheiten.
- Positionsänderungen verwenden die vorhandenen Scrollbar-Werte. Unveränderter Inhalt behält seine Messung; Inhalt, Größe, Font, Sichtbarkeitsregeln oder Scrollbar-Dicke invalidieren den passenden Cache. Layout und Eingabe laufen in `Update`; `Draw` scrollt nicht nachträglich.

Es gibt kein automatisches Nachgleiten: Beim Loslassen bleibt der erreichte Scrollwert stehen. Die Eingabe verwendet die logischen Workspace-Koordinaten und damit dessen vorhandene DPI-Umrechnung.

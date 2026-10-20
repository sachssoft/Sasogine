# ValueTextBox

Eine Zeile: **Repeat-Pfeil links · gemeinsamer Wertebereich · Repeat-Pfeil rechts**.

- **Normal:** Balken mit Thumb und numerischer Werteanzeige.
- **Ziehen:** Nach Überschreiten der Drag-Schwelle ändert sich der Wert; beim Loslassen bleibt der Balken sichtbar. Auch Zurückziehen zur Startposition wird nicht als Klick gewertet.
- **Einfacher Klick:** Der Wert springt nicht. Stattdessen erscheint im gleichen Bereich ein Textfeld mit ausgewähltem Inhalt.
- **Enter:** Gültige Eingabe übernehmen und zum Balken zurückkehren. Ungültige Eingabe bleibt zur Korrektur im Textfeld.
- **Escape:** Unbestätigten Text verwerfen; beim Ziehen den Startwert wiederherstellen.
- **Fokusverlust:** Gültige Eingabe übernehmen, ungültige verwerfen; wieder den Balken zeigen.
- **Pfeile halten:** Schrittweise ändern und wiederholen. An Minimum und Maximum ist die jeweilige Richtung deaktiviert.

```csharp
var field = new ValueTextBox
{
    Minimum = 0,
    Maximum = 2,
    Step = 0.05f,
    Value = 1,
    Height = 36
};
```

`BeginEdit()` öffnet die Eingabe, `CancelEdit()` verwirft sie. `CommitEdit()` validiert und übernimmt den Text, ohne selbst den Modus zu wechseln. `IsEditing` ist abfragbar und meldet Änderungen über `INotifyPropertyChanged`. F2/Enter/Leertaste öffnen die Eingabe bei fokussiertem Balken; Richtungstasten und Mausrad ändern den Wert. Zahlen verwenden die invariante Kultur mit Dezimalpunkt.

`Editor`, `RangeBar`, `DecreaseButton` und `IncreaseButton` sind zugänglich. `RangeBar` ist jetzt ein `ValueBar` statt eines `Slider`; Quellcode, der explizit den alten Typ verwendet, muss angepasst werden. Es gibt keinen zusätzlichen Balken unter dem Textfeld.

Styles: `ValueTextBox`, `ValueTextBox.Arrow`, `ValueTextBox.Editor`, `ValueTextBox.Range` und `ValueBar`. Die Pfeilbreite steuert `ButtonWidth`. `RangeBar.ThumbWidth` und `RangeBar.DragThreshold` sind ebenfalls stylefähig. Die Pfeile erben `RepeatDelay` und `RepeatInterval` von `RepeatButton`. Die vorhandenen Atlas-Regionen werden weiterverwendet; eigene Skins können `Background`, `Fill` und `Thumb` des Balkens ersetzen.

Die Galerie zeigt das Control unter **Tools & scripts → Settings**.

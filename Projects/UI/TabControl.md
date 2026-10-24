# TabControl: Platzierung

```csharp
var tabs = new TabControl
{
    TabStripPlacement = TabStripPlacement.Left,
    HeaderWidth = 160,
    HeaderHeight = 38
};
```

`TabStripPlacement` unterstützt `Top` (Standard), `Left`, `Bottom` und `Right`.
Oben und unten liegen die Tabs nebeneinander, links und rechts untereinander. Die Beschriftungen bleiben horizontal lesbar.

- `HeaderHeight` bestimmt die Höhe jedes Tabs. `HeaderWidth` bestimmt die Breite einer seitlichen Leiste. Eine notwendige Scrollbar kommt mit `ScrollBarThickness` hinzu.
- Überlange Leisten scrollen auf der jeweiligen Achse. Der ausgewählte Tab wird in den sichtbaren Bereich gescrollt.
- Pfeiltasten wechseln im fokussierten Headerbereich links/rechts beziehungsweise oben/unten. `Home`, `End` und `Ctrl+Tab` bleiben verfügbar.
- Platzierung und Breite sind Stylewerte mit lokalen Overrides und `INotifyPropertyChanged`. Auch `style["TabStripPlacement"] = "Right"` ist möglich; die Konvertierung benötigt keine Reflection.
- Ein Wechsel zur Laufzeit behält die Auswahl und vorhandenen Headerinstanzen bei. Der Layoutcache wird nur bei Änderungen erneuert.

Bestehende Tab-Skins werden weiterverwendet; andere Controls werden nicht umgebaut.

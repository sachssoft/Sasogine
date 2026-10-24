# IconValueDisplay

Ein HUD-Control für Leben, Munition, Währung oder Zeit: Icon und Wert werden gemeinsam angeordnet.

```csharp
var gold = new IconValueDisplay
{
    Icon = new Image { Width = 24, Height = 24 }, // Eigene Textur am Image setzen.
    Value = 2500,
    ValueFormat = "N0",
    IconPlacement = IconPlacement.Left,
    Spacing = 8
};

gold.Value = 3000;

var timer = new IconValueDisplay
{
    Icon = "Zeit",
    Value = TimeSpan.FromSeconds(92),
    ValueFormat = @"mm\:ss",
    IconPlacement = IconPlacement.Top
};
```

- `IconPlacement`: `Left`, `Right`, `Top` oder `Bottom`. Eine separate `Orientation` ist nicht erforderlich. Die Inhalte stehen nebeneinander oder untereinander und sind auf der Querachse zentriert.
- `Icon` und `Value` akzeptieren Controls oder Daten. Controls werden direkt präsentiert, andere Objekte als Text. `null` entfernt den jeweiligen Inhalt. Der Abstand entfällt, wenn einer der Inhalte fehlt oder unsichtbar ist.
- `IconPresenter` und `ValuePresenter` geben die tatsächlich dargestellten Controls schreibgeschützt zurück. Die beiden privaten Content-Hosts bleiben wiederverwendbar. `Add` belegt den Value-Slot; `Remove` und `Clear` leeren die zugehörigen Slots. Die vorhandenen Besitz- und Zyklusprüfungen gelten weiterhin.
- `ValueFormat` verwendet das Format von `IFormattable`, etwa `N0`, `F1` oder `mm\:ss` für `TimeSpan`. Ohne Format gilt `ToString()`. Eigene Controls und Objekte ohne `IFormattable` ignorieren das Format. `ValueFormatProvider` erlaubt beispielsweise `CultureInfo.InvariantCulture`; standardmäßig gilt die aktuelle Kultur bei der Konvertierung.
- Daten und Kultur werden nicht pro Frame abgefragt. Veränderliche Objekte zur Aktualisierung erneut zuweisen. Formatänderungen aus Styles werden im nächsten `Workspace.Update` verarbeitet. Fehler bei einer direkten Zuweisung erhalten den bisherigen Inhalt.
- Zentrale Styles unterstützen `IconPlacement`, `Spacing`, `ValueFormat` und die üblichen Control-Properties. Lokale Overrides lassen sich mit `ResetStyleProperty` zurücksetzen. Generierter Text verwendet die Vordergrundfarben des Displays; eigene Controls behalten ihre Farben. Schriftfamilie und Schriftstil werden über die vorhandene Font-Vererbung aufgelöst.
- Eigenschaften melden Änderungen über `INotifyPropertyChanged`. Unveränderte Frames formatieren keine Werte neu; Text-Presenter und Layout bleiben gecacht. Für eigene Darstellungen kann direkt ein Control eingesetzt werden; eine zusätzliche Template-Infrastruktur wird nicht benötigt.

# Header und Inhaltsgruppen

`HeaderContentControl : ContentControl` übernimmt Header-Daten, Header-Höhe und das Layout über dem eigentlichen Inhalt. `GroupBox` nutzt diese Basis; `Header` und `Expander` bleiben davon abgeleitete Gruppen.

```csharp
var section = new Header
{
    HeaderContent = "Darstellung",
    Content = settingsPanel
};

var details = new Expander
{
    HeaderContent = "Details",
    Content = "Einfacher Text ist ebenfalls möglich.",
    IsExpanded = false
};
```

- `Header` sieht wie ein aufgeklappter `Expander` aus, einschließlich Header-Höhe, Einrückung und Skin. Er besitzt keinen Pfeil und keine Einklappfunktion.
- `GroupBox` behält seinen Rahmen und unterstützt jetzt ebenfalls `HeaderContent` und den gemeinsamen Objektinhalt.
- `HeaderContent` und `Content` akzeptieren Controls oder beliebige Objekte. Andere Werte werden beim Zuweisen in Text umgewandelt; vorhandene Text-Presenter werden wiederverwendet.
- `HeaderContentPresenter` und `ContentPresenter` sind lesbar und nullable. Eine zusätzliche `Header`-Property gibt es nicht; der Header-Host bleibt privat.
- `HeaderHeight` gilt als Mindesthöhe; ein höherer Header-Inhalt erhält ausreichend Platz. Styles, lokale Overrides und Schriftvererbung bleiben erhalten.
- Einklappen entfernt den Inhalt nur aus Layout und Traversierung. Die Eigentümerschaft bleibt bestehen, sodass derselbe Inhalt wieder aufgeklappt werden kann.

Da `GroupBox.Content` jetzt das gemeinsame `object?` verwendet, für Layoutzugriffe `ContentPresenter` oder eine explizite Typprüfung verwenden.

Migration: `group.Header = control` wird zu `group.HeaderContent = control`; für das dargestellte Control `HeaderContentPresenter` verwenden.

# Carousel

Seitenwechsel ohne Tab-Leiste und ohne `TabItem`: Jede Seite ist direkt ein Control.

```csharp
var carousel = new Carousel
{
    Orientation = ControlOrientation.Horizontal,
    NavigationMode = CarouselNavigationMode.Arrows
        | CarouselNavigationMode.Swipe
        | CarouselNavigationMode.Keyboard,
    ShowIndicators = true,
    IsLooping = true,
    Transition = CarouselTransition.Slide,
    TransitionDuration = 0.25f
};

carousel.Items.Add(new TextBlock { Text = "Erste Seite" });
carousel.Items.Add(new Image { Source = previewTexture });
carousel.Items.Add(new Button { Content = "Dritte Seite" });

var workspace = new Workspace(carousel);
// Wie üblich: Skin vor dem ersten Update setzen, dann workspace.Update und workspace.Draw.

carousel.Next();
carousel.Previous();
carousel.GoTo(1);
```

- `SelectedIndex` und `SelectedItem` sind schreibbar und melden Änderungen über `INotifyPropertyChanged`. `SelectionChanged` meldet einen Wechsel der ausgewählten Seiteninstanz. Index `-1` beziehungsweise `SelectedItem = null` blendet die Seiten aus. Die erste hinzugefügte Seite wird automatisch ausgewählt, sofern die Auswahl nicht ausdrücklich geleert wurde.
- `Items` reserviert auch unsichtbare Seiten sofort. Doppelte Verwendung, fremde Eigentümer und Zyklen werden zurückgewiesen. Eine private Host-Instanz besitzt jeweils die eigentliche Seite. Entfernen gibt die Seite sofort frei; Änderungen der aktiven Traversierung werden an der sicheren Frame-Grenze übernommen. `Items.Move` erhält die Auswahl anhand der Seiteninstanz. Die Collection unterstützt `INotifyCollectionChanged`.
- `Orientation` bestimmt horizontale oder vertikale Navigation. Die entsprechenden Pfeiltasten sowie `Home` und `End` funktionieren, wenn Tastaturereignisse zum Carousel geroutet werden. Pfeil-Buttons und anklickbare Punkte belegen eigene Bereiche außerhalb von `ViewportBounds`. Punkte bleiben unabhängig von den `NavigationMode`-Flags über `ShowIndicators` steuerbar.
- `Swipe` unterstützt Touch und Ziehen mit der primären Maustaste. Erst ab `SwipeThreshold` übernimmt das Carousel die Geste und bricht die Aktivierung des berührten Child-Controls ab. Kurze Berührungen bleiben normale Klicks. Überwiegend quer zur Navigation verlaufende Bewegungen navigieren nicht. Pro Carousel wird ein Pointer verfolgt. Ein Swipe löst beim Loslassen einen Seitenwechsel aus; die Seite folgt dem Finger nicht während des Ziehens.
- `Transition`: `None`, `Slide` oder `Fade`. `TransitionDuration` ist in Sekunden angegeben; null Sekunden wechseln sofort. Animationen verwenden ausschließlich die an `Workspace.Update` übergebene Zeit. `Draw` verändert weder Animation noch Layout. Slide verschiebt die gecachten Seitenslots durch Arrange, ohne pro Animationsschritt neu zu messen. Fade verändert nur die Deckkraft privater Hosts. Eigenschaften der eigentlichen Seiten werden nicht überschrieben.
- Während eines Übergangs sind höchstens zwei Seiten in der Traversierung. Pointer-Eingaben auf die Seiten sind währenddessen gesperrt; die Navigation bleibt bedienbar. Eine neue Navigation beendet den bisherigen Übergang und startet vom bisherigen Ziel aus. Änderungen an Collection, Effekt oder Orientierung beenden laufende Übergänge ebenfalls sicher.
- `AutoPlayInterval` ist standardmäßig `0` und damit ausgeschaltet. Ein positiver Wert aktiviert zeitgesteuertes `Next()`. Während Gesten und Übergängen pausiert der Zähler. Ohne `IsLooping` bleibt die letzte Seite stehen; eine ausdrücklich geleerte Auswahl wird nicht automatisch wieder geöffnet. Große Zeitschritte führen höchstens einen automatischen Wechsel pro Update aus.
- Styles unterstützen Navigation, Übergang, Zeitwerte, Abstände, Größen und die üblichen Control-Properties. Lokale Zuweisungen haben Vorrang; `ResetStyleProperty` stellt den Stylewert wieder her. Bei der vorhandenen typisierten `Style.Orientation`-Kurzform gilt weiterhin `StackOrientation`; die String-Einträge `"Horizontal"` und `"Vertical"` werden beim Carousel über den expliziten `ControlOrientation`-Converter gelesen.

Die Navigation verwendet die zentralen Styles `Carousel.Previous`, `Carousel.Next` und `Carousel.Indicator`, basierend auf `Button` beziehungsweise `ToggleButton`. Dadurch funktionieren die vorhandenen Skins ohne ein neues Atlasbild. `PreviousButton` und `NextButton` können bei Bedarf direkt angepasst werden. `NavigationButtonSize`, `IndicatorSize`, `IndicatorSpacing` und `Spacing` legen die Maße fest.

`Carousel`, seine Converter und Factory werden automatisch und ohne Reflection registriert. Unveränderte Frames behalten Presenter, Layout und aktive Seiten bei.

# EdgePanel

Zwei Gruppen an gegenüberliegenden Kanten, ohne Grid. Horizontal bedeutet `Start` links und `End` rechts; vertikal oben und unten.

```csharp
var actions = new EdgePanel
{
    Orientation = StackOrientation.Horizontal,
    Spacing = 8
};

actions.Put(new TextBlock { Text = "Bereit" }, PanelEdge.Start);
actions.Put(new Button { Content = "Speichern" }, PanelEdge.End);
actions.Put(new Button { Content = "Abbrechen" }, PanelEdge.End);
```

- `Put` fügt ein freies Control hinzu oder verschiebt dasselbe Child zwischen Start und End. Ein identischer Aufruf behält den Layoutcache bei.
- Innerhalb beider Gruppen bleibt die Einfügereihenfolge erhalten. Im Beispiel steht „Speichern“ links von „Abbrechen“.
- Ohne `Put` über `Children.Add` hinzugefügte Controls gehören zu Start. `Remove` und `Clear` entfernen auch die Zuordnung; fremde Eigentümerschaft und Zyklen werden zurückgewiesen.
- `Orientation` und `Spacing` verwenden das bestehende Styling von `StackPanel`. Der freie Platz liegt zwischen den Gruppen. Unsichtbare Controls reservieren keinen Platz.
- Bei Platzmangel hat Start Vorrang. Die restlichen Slots werden begrenzt, sodass sich die Gruppen nicht überlagern. Das Panel bricht nicht um und besitzt keine proportionale Grid-Aufteilung.

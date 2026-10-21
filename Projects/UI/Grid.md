# Grid

`Rows` enthält `GridRow`, `Columns` enthält `GridColumn`.
`GridLength.Auto` richtet sich nach dem Inhalt, eine Zahl gibt eine feste Größe an.
`GridLength.Proportion(2)` verteilt den freien Platz proportional mit Gewicht 2.
Ohne eigene Zeilen oder Spalten gibt es jeweils eine proportionale Zeile beziehungsweise Spalte.

```csharp
var grid = new Grid { Spacing = 8 };
grid.Rows.Add(new GridRow { Height = GridLength.Auto });
grid.Rows.Add(new GridRow { Height = GridLength.Proportion() });
grid.Columns.Add(new GridColumn { Width = 160 });
grid.Columns.Add(new GridColumn { Width = GridLength.Proportion(2) });

var button = new Button();
grid.Children.Add(button);
grid.Cells.Put(button, row: 1, column: 1);

// Put verschiebt dieselbe Instanz; es entsteht kein zweiter Eintrag.
grid.Cells.Put(button, row: 0, column: 0, columnSpan: 2);
```

- `Children` verwaltet die Controls; `Cells` verwaltet ausschließlich deren Position.
- `Put` fügt keine Children hinzu. Die Zuordnung kann vor oder nach `Children.Add` erfolgen.
- Ein weiterer `Put` für dieselbe Instanz aktualisiert die Zuordnung. Bei identischer Position und gleichen Spans bleibt der Layoutcache gültig.
- `Put(null, ...)` wirft `ArgumentNullException`.
- `Cells.Remove(control)` entfernt nur die Zuordnung und liefert zurück, ob sie existierte. Das Control bleibt als Child in der ersten Zelle.
- `Children.Remove` entfernt auch die Zuordnung; `Children.Clear` leert beides.
- Verschiedene Controls dürfen dieselbe Zelle belegen. Ohne Zuordnung liegt ein Child in Zeile 0, Spalte 0.
- Indizes beginnen bei 0; Spans müssen mindestens 1 sein und vollständig innerhalb der Zeilen und Spalten liegen. Die Grenzen werden beim Layout geprüft.

Änderungen an Größen, Grenzen und Zuordnungen invalidieren den Layoutcache.
Ein unverändertes Grid verwendet den bestehenden Cache weiter.

Migration: `RowTrack` → `GridRow`, `ColumnTrack` → `GridColumn`, `Tracks` → `Columns`.
Proportionale Größen verwenden `GridLength.Proportion(weight)` statt `GridLength.Star(weight)`.
`grid.Add(control, row, column)` wird durch `grid.Children.Add(control)` und `grid.Cells.Put(control, row, column)` ersetzt.
`SetCell` wird durch `Cells.Put` ersetzt; `Release` heißt jetzt `Remove`.

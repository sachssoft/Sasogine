# Sasogine Logo Inspector

Ein zusätzliches Beispiel mit einer echten `LogoEntity : EntityBase<LogoDefinition>` und live bearbeitbarer Definition. Das Logo wird aus geometrischen Linien aufgebaut und benötigt keine externe Bilddatei.

- Links: Logo mit Titel, Untertitel und vorbereiteter Vektorgeometrie.
- Rechts: Inspector mit Expandern für Kategorien und verschachtelte Definitionen.
- **Transform · 2D:** Position und Skalierung jeweils als gemeinsame X/Y-Zeile, Rotation als Einzelwert.
- **Transform · 3D:** Position, Rotation und Skalierung jeweils als gemeinsame X/Y/Z-Zeile. Die räumlich transformierte Logo-Ebene wird auf die Vorschau projiziert.
- Alle Komponenten unterstützen Ziehen, Texteingabe und Repeat-Pfeile. Die Definition speichert vollständige Vektoren; eine Änderung an X erhält die aktuellen Y-/Z-Werte.
- **Appearance:** Farben über den bestehenden Color Picker, Strichbreite und Rahmen.
- **Accents:** echte `TrackableCollection<LogoAccentDefinition>`, mit Auswahl, **+ / − / ↑ / ↓** und Eigenschaften des ausgewählten Eintrags darunter.

Die Einträge werden ausdrücklich über Getter und Setter beschrieben. Es gibt keine automatische Suche nach Modell-Properties oder dynamische Codegenerierung. Enum-Felder werden automatisch erkannt; ihre Namen und Werte werden einmal pro Typ aus den Enum-Metadaten gelesen und gecacht. Observable Definitionen verwenden `INotifyPropertyChanged`; deren Getter werden bei unveränderten Werten nicht pro Frame abgefragt. Für Modelle ohne Benachrichtigungen ist gezieltes Polling möglich. Geometrie wird nur nach Änderungen neu aufgebaut; `Draw` liest die vorbereiteten Daten.

Der zusätzliche Reiter **Value types** bietet 80 Werttypen zum Ausprobieren: Punkte, Pixel-/Rasterkoordinaten, Größen, Bounds, Boxen, Insets, Farben, Vektoren, Quaternion, Matrix, Tier-Scores und weitere Zahlen-/Zeitwerte. Nur der ausgewählte Editor wird aufgebaut; seine Werte bleiben beim Wechsel erhalten. Der passende Editor wird durch `inspector.Add(...)` automatisch aus der ausdrücklichen typisierten Registrierung gewählt.

`InspectorExampleMode` zeigt einen eigenen Enum ohne Registrierung. Eigene Enums funktionieren direkt mit `inspector.Add(...)`.

Ganzzahlige Komponenten und große Zahlen werden in ihrem echten Zahlentyp bearbeitet. **Enter** oder Fokusverlust übernimmt die Texteingabe; **Escape** verwirft sie. Bounds verwenden Position/Größe, Boxen Min/Max und 3D-Insets zwei zusammengehörige Zeilen. Details und Erweiterung: `Projects/UI/Inspection.ValueTypes.md`.

## Start

Im vorhandenen Sasogine-Checkout:

```powershell
dotnet run --project Examples/Advanced.08.LogoInspector/08.LogoInspectorGL.csproj
```

Parameter: `--skin modern`, `--skin light`, `--skin futuristic`, `--skin medieval`, `--skin dark`, `--font <eigene.ttf>`, `--snapshot <bild.png>`, `--values Bounds3`, `--values PixelPoint3`, `--verify` und `--self-test`.

Direkt in diesem Beispielordner `Logo Inspector starten.cmd` öffnen. `Werttypen starten.cmd` öffnet die Werttypen-Galerie. Die fertige EXE mit ihren benötigten Dateien liegt unter `publish/win-x64/`. Alternativ die EXE-ZIP vollständig entpacken und dort die Start-CMD öffnen. Die NativeAOT-Ausgabe benötigt keine separate .NET-Installation. Windows x64 und OpenGL werden vorausgesetzt; Fonts kommen vom eigenen System.

## Dateien

`LogoDefinition` enthält die Entity-Daten und die untergeordneten Definitionen. `LogoEntity` hält den Geometriecache. `LogoCanvas` zeichnet diesen Cache. `LogoInspectorView` beschreibt die editierbaren Eigenschaften mit den wiederverwendbaren Controls unter `Sachssoft.Engine.Controls.Inspection`. `InspectorChecks` prüft Eingabe, Rückbindung, Collections, Cache und Lebensdauer.

Bei Übernahme des vollständigen Controls-Projekts aus einer ZIP muss die frühere `Projects/UI/Root.cs` entfernt werden: Die UI-Verwaltung heißt jetzt `Workspace`.

## Assets und Pipette

`Assets und Referenzen starten.cmd` öffnet den neuen Reiter **Assets**:

- `AssetView` ist eine `ListBox` mit Kacheln oder Zeilen, Suche, Mehrfachauswahl und gecachten Vorschauen. Nur sichtbare Einträge fordern Vorschauen an.
- **Texture / Browse** öffnet denselben View als typgefilterten Reference Picker. Eine Kachel kann auch direkt auf das Referenzfeld gezogen werden. Die Entity-Definition speichert eine echte `Reference<Texture2DAsset>`.
- **Color picker & render-target pipette** öffnet den Farbdialog. **Pick pixel** blendet ihn vorübergehend aus; links im Logo einen Pixel auswählen. Eine Lupe zeigt die Nachbarpixel. Klick übernimmt den Pixel, Escape verwirft die Pipettenauswahl. Erst **OK** im Farbdialog übernimmt die Farbe in die Definition.

Das Beispiel rendert in ein eigenes RenderTarget. Pipette und Lupe werden anschließend separat gezeichnet und deshalb nicht selbst als Farbe aufgenommen. Die Texturvorschauen sind kleine prozedurale Beispielmaterialien; es wird kein Asset Store vorausgesetzt. Eigene registrierte Assets werden als `AssetItem`-Metadaten eingespeist. Details: `Projects/UI/AssetViews.md`.

## Property-Menüs

Jede Property-Zeile hat rechts eine **⋮**-Taste: **Reset**, **Copy value** und **Paste value**. Ein Vektor oder eine mehrzeilige Bounds-/Insets-Property erhält ein gemeinsames Menü für den vollständigen Wert.

Reset stellt den ausdrücklich gesetzten Vorgabewert wieder her; ohne eigene Vorgabe gilt der beim Anlegen des Felds gespeicherte Wert. Position und Skalierung haben im Logo-Beispiel ausdrücklich festgelegte Defaults. Copy/Paste verwendet eine typisierte Zwischenablage innerhalb der UI-Registry; unterschiedliche Typen können nicht ineinander eingefügt werden. Referenzen werden über ihre ID kopiert und nicht als gemeinsam veränderliches Reference-Objekt weitergegeben.

Eigene Defaults: `inspector.SetDefault(editor, Vector3.Zero)`. Zum Entfernen eines Style-Overrides kann `inspector.GetPropertyMenu(editor).ResetAction` ausdrücklich `control.ResetStyleProperty(...)` aufrufen. Zusätzliche Menüeinträge können über `GetPropertyMenu(editor).Menu.Items` ergänzt werden.

`inspector.ShowPropertyMenus = false` blendet die Menü-Tasten aus, schließt offene Property-Menüs und entfernt die komplette Menü-Spalte samt Abstand. Verschachtelte Gruppen erben die Einstellung, sofern sie nicht ausdrücklich eine eigene Vorgabe erhalten. Neue Felder übernehmen die aktuelle Sichtbarkeit. Die Checkbox **Property menus** im Beispiel schaltet die Tasten zur Laufzeit in allen Reitern um.

Inspector-Menütasten und ihre Popups werden schlicht mit soliden Flächen gezeichnet, ohne Atlas-Hintergründe. Andere Menüs verwenden weiterhin ihren normalen Skin. Mit `MenuBar.UseSkin = false` kann auch ein eigenes Menü solide dargestellt werden.

## Fenstergröße und DPI

Beim Vergrößern oder Verkleinern des Fensters wird das Layout automatisch neu aufgeteilt. Die UI wird dabei nicht auf eine feste Entwurfsauflösung gestreckt. `Workspace.Dpi` bleibt unabhängig von der Fenstergröße: **96 = 100 %**, **144 = 150 %**, **192 = 200 %**.

```csharp
workspace.Dpi = 144;
```

Das Beispiel verwendet denselben DPI-Wert für Maus/Touch, Layout und Zeichnung. `DPI 150 Prozent starten.cmd` startet es direkt mit 150 %. Alternativ `Logo Inspector starten.cmd --dpi 144` verwenden. `--verify-resize --snapshot bild.png` prüft echte Fenstergrößenänderungen sowie den DPI-Wechsel. Eine unveränderte Fenstergröße/DPI verwendet den vorhandenen Layout-Cache weiter.

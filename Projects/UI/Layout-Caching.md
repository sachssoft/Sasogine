# Layout-Caching

`Control` behÃ¤lt `DesiredSize` und `Bounds`. Messung wird nur bei ungÃ¼ltigem Layout, geÃ¤nderter verfÃ¼gbarer GrÃ¶ÃŸe oder anderem Standardfont erneut ausgefÃ¼hrt. Anordnung wird nur bei ungÃ¼ltigem Layout oder geÃ¤ndertem Slot beziehungsweise Begrenzungsverhalten erneuert. Ã„nderungen invalidieren den betroffenen Pfad zu den Eltern; unverÃ¤nderte Geschwister verwenden ihre passenden Cache-EintrÃ¤ge weiter.

`TextBlock` und `Label` speichern zusÃ¤tzlich den vorbereiteten Anzeigetext, TextmaÃŸe und lokale Zeichenposition. Ohne Text-, Schrift- oder relevante UmbruchÃ¤nderung bleiben die TextmaÃŸe erhalten. Unumgebrochener Text muss bei einer reinen Breiten- oder AusrichtungsÃ¤nderung nicht neu gemessen werden. Farbe und Deckkraft beeinflussen den Layout-Cache nicht. Gleiche Property-Werte lÃ¶sen keine neue Berechnung aus.

`Workspace.Update` bereitet ungÃ¼ltige Daten vor. `Draw` verwendet das vorbereitete Layout; `TextBlock.DrawText` misst und umbricht keinen Text. Es werden keine vollstÃ¤ndigen Controls als Texturen zwischengespeichert. Das Batching des Renderers ist durch diese Ã„nderung unverÃ¤ndert.

Bei externer Ã„nderung eines Fontobjekts ohne Property-Benachrichtigung `textBlock.InvalidateLayout()` aufrufen. Das verwirft dessen TextmaÃŸe und invalidiert das Layout. Eigene Controls rufen nach Ã„nderungen nicht beobachtbarer Layoutressourcen ebenfalls `InvalidateLayout()` auf. Eigene zusÃ¤tzliche Caches kÃ¶nnen diese virtuelle Methode Ã¼berschreiben und mÃ¼ssen die Basismethode aufrufen.

Fehlgeschlagene Messung oder Anordnung bleibt ungÃ¼ltig und wird beim nÃ¤chsten Update erneut versucht.

## PrÃ¼fung

- Headless-Suite: 18 Gruppen, 2.074 Assertions unter .NET 8, 9 und 10 sowie tatsÃ¤chlichem NativeAOT-Publish und -Start.
- ZusÃ¤tzliche FÃ¤lle: unverÃ¤nderte Frames, reine Farbe/Deckkraft, gleicher Textwert, neue Textinhalte, Textausrichtung, Breite/Umbruch, Fontwechsel, externe FontÃ¤nderung, ViewportgrÃ¶ÃŸe und Wiederholung nach Layoutfehlern.
- 100 Buttons Ã¼ber 10.000 unverÃ¤nderte Input-/Update-Frames: 0 verwaltete Allokationen.
- GrafikprÃ¼fung: 105 echte Draws, keine `IControlFont.Measure`-Aufrufe wÃ¤hrend Draw; neue Ausrichtung und neuer Text geprÃ¼ft. Screenshot: `cache.png` neben der PrÃ¼fanwendung.

```cmd
dotnet run --project Validation/Validation.csproj -c Release -f net10.0
dotnet run --project Validation/Graphics/GraphicsProbe.csproj -c Release
dotnet publish Validation/Validation.csproj -c Release -f net10.0 -p:TargetFrameworks=net10.0 -r win-x64 -p:PublishAot=true
```

Die PrÃ¼fprojekte verwenden die aktuellen Controls-Quelldateien und die bestehende Basic-DLL unter `Projects/Basic/bin/Debug/<Framework>`. FÃ¼r NativeAOT werden die Ã¼blichen Windows-C++-Buildtools benÃ¶tigt. `Validation/Check-NoReflection.ps1` prÃ¼ft zusÃ¤tzlich die Quellen auf Reflection-APIs.

Der eigene Renderer und seine Grafikprüfungen sind in [FrameBatch](FrameBatch.md) beschrieben.

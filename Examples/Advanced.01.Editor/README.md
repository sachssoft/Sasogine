# Advanced.01.Editor

Ein kleiner 2D-Editor als verständliche Grundlage für eigene Sasogine-Werkzeuge. Eigenständiges Beispiel, keine weitere Galerie-Seite.

- Objektbaum und Zeichenfläche mit gemeinsamer Auswahl.
- Rechtecke anlegen, duplizieren, löschen und per Maus oder Touch verschieben.
- Inspector für Name, Position, Größe, Sichtbarkeit und Farbe; bestehender ColorPickerDialog mit Sasogine-Farbmodellen.
- Raster und optionales Einrasten auf 20 Pixel.
- Undo/Redo mit maximal 64 Schritten; ein kompletter Drag ist ein Schritt. Escape und Eingabeverlust brechen ihn ab.
- JSON öffnen und speichern, ohne Reflection oder automatische Serializer-Erkennung.
- Zentrale String-Styles und eigenes Canvas mit `StyleProperty<T>` und `ApplyStyle`; direkte Erstellung ohne zusätzliche Registrierung.


Alle Controls verwenden direkte Properties. Der Host erstellt `_root` in `Initialize`, lädt Skin und Font vor dem ersten Update und ruft danach `_root.Update(...)` und `_root.Draw(...)` auf. Eingaben werden einmal mit `SampleReusable` erfasst; `Root.Dispose()` löst Bindungen und Control-Verbindungen.


## Starten

`dotnet run --project 01.EditorGL.csproj`

Mit bereits gebauten Basic-/Controls-DLLs: `dotnet run --project 01.EditorGL.csproj -p:UseBuiltEngine=true -p:SasogineBuildConfiguration=Debug`

`EXE erstellen.cmd` erzeugt eine eigenständig startbare Windows-x64-Version unter `publish/win-x64`. Im fertigen EXE-Paket: `Editor starten.cmd` öffnen.

## Bedienung

Auswahl über Baum oder Zeichenfläche. Ziehen verschiebt; Pfeiltasten bewegen um 1 Pixel, Umschalt+Pfeil um 20 Pixel. Löschen auf der Zeichenfläche oder über die Werkzeugleiste. Strg+N/O/S: neue Szene/öffnen/speichern. Strg+Z/Y: Dokumenthistorie; Textfelder behalten ihre eigene Text-Historie. Name und Zahlen werden direkt übernommen. Bei neuer Szene und beim Öffnen werden ungespeicherte Änderungen abgefragt; beim Schließen des Fensters bitte vorher speichern.

Gespeichert wird standardmäßig `%LOCALAPPDATA%/Sachssoft/EditorExample/document.json`. `--document <Pfad.json>` legt die Zieldatei fest. Öffnen ändert die aktuelle Zieldatei; Speichern ersetzt sie. Die Datei enthält nur die Rechtecke, keine UI-Zustände. Dokumentgröße: 960 × 540, maximal 256 Rechtecke. Dies ist bewusst kein vollständiger Szeneneditor; Gruppen und Mehrfachauswahl sind nicht Teil dieses Beispiels.

## Aufbau

`EditorDocument` hält Daten, Auswahl und Historie; `EditorFile` liest/schreibt explizite JSON-Felder mit Versions- und Werteprüfung. `EditorView` verdrahtet die Controls, `EditorCanvas` zeigt eigenes Zeichnen und Pointer-Capture. Der Host erfasst Eingaben einmal pro Frame und verwendet denselben Font/Viewport/Scale beim Layout und Zeichnen.

Der Skin wird aus `Projects/UI/Skins/SachssoftDark` referenziert. Keine Systemschrift ist enthalten. Ohne Standardfont: `--font <Datei.ttf>`.

## Prüfung

`01.EditorGL.exe --self-test` prüft Dokumentaktionen und JSON ohne Fenster. `01.EditorGL.exe --verify --snapshot editor.png` prüft echtes Canvas-Dragging, Inspector-Synchronisation, Undo und Abbruch und speichert eine Vorschau. `--help` zeigt alle Optionen.

Geprüft: Release-Build ohne Warnungen, 27 Dokumentprüfungen und tatsächliches DesktopGL-Rendering mit Maus-/Touch-Dragging, Inspector-Änderungen, Undo sowie Abbruch durch Escape und Deaktivierung. Die veröffentlichte Windows-EXE wurde über die Start-CMD ausgeführt.

## Windows-EXE erstellen

`EXE erstellen.cmd` veröffentlicht eine eigenständig startbare x64-Anwendung nach `publish/win-x64`. Optional mit bereits gebauten Debug-Engine-DLLs:

```cmd
"EXE erstellen.cmd" -p:UseBuiltEngine=true -p:SasogineBuildConfiguration=Debug
```

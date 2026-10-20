# Sasogine FrameBatch / SpriteBatch / Myra

Gemessen: 2026-10-20T11:29:29.0780519+02:00; .NET 10.0.12; Microsoft Windows 10.0.22631; Intel(R) UHD Graphics 620; CPU: Intel64 Family 6 Model 142 Stepping 12, GenuineIntel; 8 logische Prozessoren.

| Buttons | Sasogine SpriteBatch ms | Sasogine FrameBatch ms | Myra ms | FrameBatch weniger Zeit als SpriteBatch | Draw-Calls S/F/M | Median-Sieger |
|---:|---:|---:|---:|---:|---|---|
| 100 | 6.384 | 1.985 | 6.102 | 68.9% | 200/20/200 | Sasogine FrameBatch |
| 1000 | 52.471 | 6.640 | 55.113 | 87.3% | 2000/50/2000 | Sasogine FrameBatch |
| 5000 | 63.870 | 8.130 | 74.153 | 87.3% | 10000/108/10000 | Sasogine FrameBatch |

6 wechselnde Runden, je 60 gemessene echte Frames und 20 Warmup-Frames pro Bibliotheksblock. Standardmäßig alle sechs Bibliotheksreihenfolgen. Median der Batch-Mittelwerte; Rohdaten einschließlich UI-Aufrufzeiten und Zustandszählern in raw.csv. Wiederholungsläufe zum Prüfen der Stabilität verwenden.

## Kontrollierter Vergleich

Beide Sasogine-Bäume verwenden dieselben aktuellen Control-Quellen und denselben Layout-/Text-Cache. Nur ControlRenderer ist unterschiedlich. Beide Fontpfade verwenden dieselbe FontStashSharp-Fontinstanz; FrameBatch erhält native Glyph-Quads. Myra 1.6.8 wird unverändert verwendet. MonoGame DesktopGL 3.8.6, FontStashSharp 1.6.2. Geometrie und Anzahl der Controls werden vor und nach den Messungen geprüft.

Eine reale Update-/Draw-/Present-Schleife direkt auf dem 1200 × 800 Backbuffer. VSync aus, variabler Timestep, InactiveSleepTime=0. Kein Readback, kein GPU-Drain, kein RenderTarget-Wechsel und keine Ergebnisanzeige während der Messung. Aufbau, Warmup und Ausgabe liegen außerhalb. Framezeit umfasst die Ereignisschleife zwischen Frames und normales Present. UI-Aufrufzeit umfasst Root.Update + Root.Draw beziehungsweise Myras vollständiges Desktop.Render. Das sind CPU-Wandzeiten einschließlich möglicher Treiberwartezeiten, keine isolierten GPU-Zeiten.

## Was FrameBatch ändert

Wiederverwendete Vertex-/Indexpuffer; CPU-Transformation und Clipping; keine SpriteBatch-Neustarts pro Control. Texture-Gruppen dürfen nur an späteren Gruppen vorbeiziehen, wenn ihre konservativen Rechtecke disjunkt sind. Überlappungen, Transparenz und explizite Kompatibilitätsbarrieren behalten ihre Reihenfolge. Buffergrenzen erzwingen geordnete Flushes. Layout und Zeichendaten der Controls werden nicht verändert. Der alte SpriteBatch-Pfad bleibt für Vergleich und bestehende Custom-Fonts/-Controls erreichbar.

## Geltungsbereich

Unveränderte UI mit 100, 1.000 oder 5.000 sichtbaren Buttons, je einem Textinhalt und gleichen StackPanel-Ebenen. Einfarbige Hintergründe, identische Schrift und Größe 10; keine Spiellogik, Atlas-Skins, Animationen oder aktive Eingaben im Performance-Szenario. Maus synthetisch außerhalb, Keyboard leer. Darstellung mit Atlas-Skin, Clipping, Transparenz, DPI und Kompatibilitätsbarrieren wird separat geprüft. Ergebnisse hängen von Hardware, Treiber, Energiezustand und Hintergrundlast ab und sind keine allgemeine Geschwindigkeitsgarantie für komplette Spiele.

TieredCompilation=0.

Controls SHA-256: `712615037578A4A151B39C8A03FD133A10D367C9B22DAA905FFB0E65DD470659`.

Myra SHA-256: `89FDF258917A14EA05FBA234EABA9D148FC8375EFF4D0698EE46AC6A415DB956`.

Font SHA-256: `74F2B3D0C20CF7380EB121A09FD7CDFDC1CCDD12A00DB83CAEC0FEB48B4DB9F7`.

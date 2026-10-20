# Sasogine / Myra: laufende UI-Spielschleife

Dieser Vergleich verwendet die aktuelle Controls-Version einschließlich Layout-/Text-Cache und Myra 1.6.8. Die UIs werden im normalen Spielfenster mit Update, Draw und Present ausgeführt. Keine künstliche GPU-Synchronisierung und kein Readback während der Messung.

- 100, 1.000 und 5.000 sichtbare Buttons mit Text und identischer Geometrie.
- Acht Runden in wechselnder AB/BA-Reihenfolge; pro Bibliotheksblock 20 Warmup-Frames und 60 gemessene Frames.
- Framezeit einschließlich normaler Bildausgabe; zusätzlich UI-Aufrufzeit und tatsächliche Grafik-Draw-Calls.
- Am Ende zeigt das Fenster den Median-Sieger. **Esc** schließt das Fenster.

[Ergebnis aus zwei vollständigen Läufen](Ergebnis.md)

## Starten

Fertiges EXE-Paket entpacken und **Benchmark starten.cmd** ausführen. Keine installierte .NET-Runtime erforderlich; Windows x64 und eine installierte Segoe-UI-Schrift werden benötigt.

Aus Quellcode: Diesen Ordner unter `Sasogine/Benchmarks/Controls.Myra.GameLoop` verwenden. Benötigt werden .NET 10 SDK, aktuelle Controls-Quellen unter `Projects/UI` und die gebaute Basic-DLL unter `Projects/Basic/bin/Release/net10.0`. Das Engine-Unterprojekt baut die vorhandenen Controls-Quellen separat mit Release-Optimierung. Beim ersten Restore benötigt NuGet Internet.

```cmd
"Benchmark starten.cmd"
"Benchmark starten.cmd" --quick --exit
"Benchmark starten.cmd" --exit --output Results-Eigener-Lauf
"Benchmark starten.cmd" --rounds 8 --frames 120 --warmup 30
```

`--quick` prüft nur 100 Buttons. `--font <Datei.ttf>` wählt eine andere Schrift. Ohne `--output` entsteht ein Zeitstempelordner unter `Results-GameLoop`. Ein expliziter Zielordner wird wiederverwendet und seine Ergebnisdateien werden ersetzt. `EXE erstellen.cmd` veröffentlicht unter `publish/win-x64`.

## Einordnung

Das ist eine echte laufende **UI-Spielschleife**, kein vollständiges Spiel: unveränderte UI, keine Animationen, Klicks, Textänderungen, Physik oder zusätzliche Spielgrafik. Sie zeigt, welche Bibliothek dieses Szenario schneller ausführt. Frühere isolierte Draw-Tests verwendeten GPU-Readbacks und sind nicht direkt vergleichbar.

**Frame ms:** reale Zeit für zusammenhängende Frames einschließlich normalem Present. **UI ms:** CPU-Wandzeit von `Root.Update + Root.Draw` beziehungsweise Myras vollständigem `Desktop.Render`. Beide enthalten Update und Rendering; Myras API bündelt diese Schritte. Keine isolierten GPU-Zeiten. Grafische Zähler erfassen tatsächliche API-Draw-Calls und keine Begin/End-Aufrufe.

Median der Batch-Mittelwerte; kleinere Zeit gewinnt. Bei überlappenden Messbereichen kleine Unterschiede vorsichtig beurteilen. Während des Laufs keine weiteren Benchmarks oder Builds ausführen; Energieprofil und Netzbetrieb zwischen Vergleichen gleich halten.

Berichte, Rohwerte, DLL-/Font-Hashes und Methodenbeschreibung stehen in den jeweiligen Ergebnisordnern. Die Ergebnisanzeige wird als `Ergebnis.png` gespeichert. Lizenzen der Abhängigkeiten liegen unter `Licenses`.

## SpriteBatch

SpriteBatch bündelt Zeichenarbeit. Viele kleine Batches, wechselnde Texturen und wechselnde Clipping-Bereiche können die Kosten erhöhen. Sasogine startet aktuell je Elementabschnitt einen neuen Batch; das ist ein möglicher CPU-Engpass. Ob dies gegenüber Myra Zeit kostet, lässt sich aus einem Lauf allein nicht isoliert beweisen. Die Zähler zeigen außerdem, ob beide Renderer ähnlich viele Grafikaufrufe erzeugen. Layout-/Text-Caching reduziert nicht automatisch die Anzahl der Draw-Calls.

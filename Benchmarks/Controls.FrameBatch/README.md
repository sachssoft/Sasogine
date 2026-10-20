# FrameBatch / SpriteBatch / Myra

Vergleicht die aktuelle Sasogine-UI mit eigenem FrameBatch, denselben Controls mit SpriteBatch und Myra 1.6.8. Beide Sasogine-Pfade besitzen denselben Layout-/Text-Cache. Nur der Renderer unterscheidet sich.

- Echte Update-/Draw-/Present-Schleife, VSync aus, kein Readback während gemessener Frames.
- 100, 1.000 und 5.000 unveränderte sichtbare Buttons mit Text und gleicher Geometrie.
- Sechs Runden mit allen sechs Bibliotheksreihenfolgen. Je Block 20 Warmup-Frames und 60 gemessene Frames.
- Ergebnisfenster zeigt Framezeiten und tatsächliche Draw-Calls. Esc schließt das Fenster.

[Gemessene Ergebnisse](Ergebnis.md)

## Starten

EXE-Paket vollständig entpacken und **Benchmark starten.cmd** ausführen. Windows x64; keine .NET-Installation notwendig. Segoe UI wird aus Windows geladen.

Aus Quellcode: Ordner unter `Benchmarks/Controls.FrameBatch` im Sasogine-Projekt verwenden. .NET 10 SDK, aktuelle Controls-Quellen unter `Projects/UI` und Basic unter `Projects/Basic/bin/Release/net10.0` werden benötigt. Der Wrapper baut aktuelle Controls separat optimiert; beim ersten NuGet-Restore ist Internet erforderlich.

```cmd
"Benchmark starten.cmd"
"Benchmark starten.cmd" --quick --exit
"Benchmark starten.cmd" --exit --output Results-Eigener-Lauf
```

Neue Ergebnisse liegen unter `Results-FrameBatch/<Zeitstempel>`: Bericht, CSV-Rohwerte, Zusammenfassung und Ergebnisbild. Ein expliziter Zielordner wird wiederverwendet und seine Ergebnisdateien werden ersetzt. `EXE erstellen.cmd` erstellt ein eigenständiges Windows-Paket unter `publish/win-x64`. `--font <Datei.ttf>` wählt eine andere Schrift.

Gemessen wird die laufende UI, ohne zusätzliche Spielsimulation oder weitere Spielgrafik. Zeiten enthalten normales Present und mögliche Treiberwartezeiten; keine isolierten GPU-Zeitstempel. Die Grafikprüfung von Skins, DPI, Clipping und Überdeckungen erfolgt separat. Während des Laufs keine weiteren Builds oder Benchmarks ausführen. Lizenzen unter `Licenses`.

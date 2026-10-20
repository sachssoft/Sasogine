# Sasogine / Myra – Rendering-Benchmark

Vergleicht ausschließlich `Root.Draw` und `Desktop.RenderVisual`. Aufbau, Layout und Eingaben liegen außerhalb der Messung.

- 100, 1.000 und 5.000 sichtbare Buttons mit gleichem Text und gleicher Geometrie.
- Acht Runden mit wechselnder Reihenfolge, jeweils 20 Draws; vorher 20 Warmup-Draws pro Baum.
- Fenster zeigt Zeiten, Median-Sieger, Abstand und prozentual weniger Zeit. Nach Abschluss bleibt es geöffnet; **Esc** schließt es.
- Neue Ergebnisse landen in `Results-Rendering/<Zeitstempel>`: Bericht, Rohwerte, Zusammenfassung und Screenshots. `--output <Ordner>` legt einen eigenen Zielordner fest und überschreibt dessen Ergebnisdateien.

## Starten

**Fertiges EXE-Paket:** entpacken und `Benchmark starten.cmd` ausführen. Keine .NET-Installation nötig; Windows x64 und eine installierte Segoe-UI-Schrift werden benötigt.

**Aus Quellcode:** Ordner unter `Sasogine/Benchmarks/Controls.Myra` ablegen. .NET 10 SDK, aktuelle Controls-Quellen unter `Projects/UI` und die gebaute Basic-DLL unter `Projects/Basic/bin/Release/net10.0` werden benötigt. Das Quellcode-Paket enthält den Benchmark; die Engine kommt aus deinem Sasogine-Projekt. Dann `Benchmark starten.cmd` ausführen. Beim ersten NuGet-Restore ist Internet nötig.

```cmd
"Benchmark starten.cmd"
"Benchmark starten.cmd" --quick
"Benchmark starten.cmd" --exit --output Results-Test
"Benchmark starten.cmd" --rounds 8 --frames 40 --warmup 30
```

`--quick` prüft nur 100 Buttons. `--exit` beendet das Programm nach dem Speichern. `--font <Datei.ttf>` wählt eine andere Schrift. `EXE erstellen.cmd` veröffentlicht eine eigenständige Anwendung unter `publish/rendering-win-x64`.

## Zeiten verstehen

Die Werte sind **CPU-Aufrufzeiten in Millisekunden**, keine GPU-Zeiten oder Spiel-FPS. Kleiner ist besser. Gewinner ist die Bibliothek mit dem kleineren Median der Batch-Mittelwerte. Prozent = Zeitersparnis relativ zur langsameren Bibliothek. Überlappende Messbereiche werden angezeigt; kleine Unterschiede können Messrauschen sein.

Beide Renderer teilen GraphicsDevice, RenderTarget und Fontinstanz. Geometrie wird vor und nach den Messungen geprüft. Ein Pixel-Readback synchronisiert vor und nach jedem einzelnen Draw; der Readback selbst ist nicht mitgemessen. Ergebnisanzeige, Clear, Present und Screenshots sind ebenfalls ausgeschlossen. Während eines vollständigen Laufs keine anderen Benchmarks starten.

Die neue Messung steht unter [Results-Rendering-Run1](Results-Rendering-Run1/Vergleich.md). `Results-Run1` enthält den früheren Vergleich mit mehreren Arbeitslasten und gehört nicht zu diesem Rendering-Test.

Versionen: Myra 1.6.8, MonoGame DesktopGL 3.8.6 und FontStashSharp 1.6.2. Sasogine Controls wird aus den aktuellen Quellen separat mit Release-Optimierung gebaut. Lizenzen der Abhängigkeiten liegen unter `Licenses`.

# Sasogine: eigener FrameBatch

**ControlFrameBatch ist jetzt der Standardrenderer der Controls.** Bestehende Initialisierung von ControlDrawContext bleibt gültig. Eigene Vertex-/Indexpuffer, CPU-Clipping und sichere Texturgruppierung ersetzen die SpriteBatch-Neustarts pro Control. Beide Sasogine-Vergleichspfade verwenden denselben aktuellen Layout-/Text-Cache.

Zwei vollständige Läufe mit jeweils sechs Runden, drei Bibliotheken und drei Baumgrößen: insgesamt **6.480 gemessene echte UI-Frames** mit Update, Draw und normalem Present. Kein Readback während der Messung.

| Lauf | Buttons | SpriteBatch ms | FrameBatch ms | Myra ms | FrameBatch weniger Zeit als SpriteBatch |
|---:|---:|---:|---:|---:|---:|
| 1 | 100 | 6.384 | 1.985 | 6.102 | 68.9% |
| 1 | 1000 | 52.471 | 6.640 | 55.113 | 87.3% |
| 1 | 5000 | 63.870 | 8.130 | 74.153 | 87.3% |
| 2 | 100 | 3.436 | 0.835 | 3.207 | 75.7% |
| 2 | 1000 | 50.146 | 6.592 | 51.341 | 86.9% |
| 2 | 5000 | 226.501 | 31.502 | 287.149 | 86.1% |

**FrameBatch gewinnt in beiden Läufen bei allen drei Baumgrößen.** Die absoluten Zeiten schwanken auf dem Testrechner deutlich. Die vollständigen Rohwerte bleiben erhalten; die Werte gelten für diesen UI-Test und diese Hardware, nicht für komplette Spiele.

| Buttons | SpriteBatch / Myra Draw-Calls | FrameBatch Draw-Calls | Reduktion |
|---:|---:|---:|---:|
| 100 | 200 | 20 | 90,0 % |
| 1.000 | 2.000 | 50 | 97,5 % |
| 5.000 | 10.000 | 108 | 98,92 % |

## Verwendung

Vorhandener Code nutzt den FrameBatch automatisch. Für neue Custom-Fonts zusätzlich IControlBatchFont implementieren; die beiden eingebauten Fontadapter unterstützen ihn bereits. Eigene Controls verwenden die Context-Methoden oder context.FrameBatch. SpriteBatch-Zugriff und alte IControlFont-Implementierungen bleiben über einen geordneten Kompatibilitätspfad nutzbar.

## Prüfung

Vier Galerie-Seiten und Modal-Dialog mit Skin sind bei normaler Skalierung pixelgleich zum Referenzpfad. Geprüft sind außerdem DPI, Half-Pixel-Adressierung, Layerdepth, UV-Clipping, Alpha-Überlagerungen, 12.000 Quads über Buffergrenzen und gemischte Custom-Zeichenpfade. Ein Geometrie-DPI-Sonderfall zeigt eine abweichende Randpixeldeckung; weitere Abweichungen sind minimale Farb-Rundungen, keine Änderungen im Bildinneren.

18 Gruppen / 2.074 Assertions unter .NET 8/9/10 und NativeAOT. Der reale Grafiktest wurde ebenfalls als NativeAOT-Anwendung veröffentlicht und gestartet. 1.000 native Text-/Geometrie-Draws: 0 verwaltete Bytes im JIT. Im getesteten NativeAOT/DesktopGL entstehen noch 64 Byte je nichtleerem GPU-Flush innerhalb der beiden MonoGame-Uploads; diese Backend-Kostenstelle ist dokumentiert.

[Lauf 1: Methodik und Daten](Results-Run1/Vergleich.md) · [Lauf 2: Methodik und Daten](Results-Run2/Vergleich.md).

Der Quellcode liegt im Sasogine-Projekt unter Projects/UI; die Implementierung und Migrationshinweise sind in Projects/UI/FrameBatch.md beschrieben. Root-READMEs, Basic-Quellen und gemeinsame Build-Props wurden nicht geändert.

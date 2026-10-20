# Sasogine / Myra: zwei Läufe im Spielbetrieb

Aktuelle Controls mit Layout-/Text-Cache, Intel UHD Graphics 620, .NET 10 Release. Zwei vollständige Läufe mit je acht AB/BA-Runden und 60 gemessenen Frames pro Bibliothek und Baumgröße: **5.760 gemessene Frames**. Beide Läufe verwenden dieselben Programmdateien.

**Kein stabiler Sieger:** Lauf 1 liegt bei allen Größen zugunsten Sasogine, Lauf 2 bei allen Größen zugunsten Myra. Die Abstände innerhalb der Läufe betragen etwa 2–6 %. Hintergrundlast, Energie-/Taktzustand oder Treiber können die Schwankungen beeinflussen; deren konkrete Ursache wurde nicht bestimmt.

| Buttons | Lauf 1 Sasogine / Myra | Sieger Lauf 1 | Lauf 2 Sasogine / Myra | Sieger Lauf 2 | Draw-Calls je Frame, beide |
|---:|---|---|---|---|---:|
| 100 | 1.905 / 1.948 ms | Sasogine | 0.946 / 0.892 ms | Myra | 200 |
| 1000 | 14.688 / 15.257 ms | Sasogine | 11.994 / 11.498 ms | Myra | 2000 |
| 5000 | 65.063 / 66.492 ms | Sasogine | 65.738 / 63.852 ms | Myra | 10000 |

## SpriteBatch

Viele kleine Batches und Texturwechsel können CPU- und Treiberzeit kosten. Hier erzeugen **beide Bibliotheken zwei Draw-Calls und zwei erfasste Texturwechsel pro Button**: Hintergrund und Text verwenden unterschiedliche Texturen. Der Layout-/Text-Cache spart Vorbereitungsarbeit, reduziert diese Grafikaufrufe aber nicht.

Sasogine führt außerdem in `ControlDrawContext.SetControl` bei jedem Elementabschnitt `End/Begin`, Clip-Berechnung und Transformation aus. Das ergibt im getesteten Baum rechnerisch 422 / 4.052 / 20.102 Begin-Aufrufe pro Draw bei 100 / 1.000 / 5.000 Buttons. Viele davon gehören zu leeren Abschnitten und sind keine zusätzlichen Grafik-Draw-Calls. Ihr genauer Zeitanteil wurde nicht isoliert gemessen.

Ein sinnvoller nächster Schritt ist, unnötige Batch-Neustarts zu vermeiden und kompatible Zeichenbefehle bei korrekter Zeichenreihenfolge zusammenzufassen. Für diesen Vergleich wurde der Renderer nicht verändert.

## Messumfang

Normale Update-/Draw-/Present-Schleife direkt im Spielfenster. Kein GPU-Readback oder erzwungener GPU-Drain während gemessener Frames. Vollständiger UI-Frame: Sasogine `Root.Update + Root.Draw`, Myra `Desktop.Render`. Framezeiten enthalten normale Bildausgabe und die Ereignisschleife; keine isolierten GPU-Zeiten. Die UI bleibt unverändert; keine Physik, Spiellogik oder zusätzlichen Spielgrafiken. Die Werte sind keine allgemeine Geschwindigkeitsrangliste der Engines.

[Lauf 1: Methodik und Daten](Results-Run1/Vergleich.md) · [Lauf 2: Methodik und Daten](Results-Run2/Vergleich.md). Rohmessungen, Min/Max, UI-Aufrufzeiten, Zähler und DLL-Hashes liegen in beiden Ergebnisordnern. Frühere isolierte Draw-Messungen mit Readback sind methodisch nicht direkt vergleichbar.

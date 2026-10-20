# Sasogine / Myra: laufende Spielschleife

Gemessen: 2026-10-20T09:30:09.5152296+02:00; .NET 10.0.12; Microsoft Windows 10.0.22631; Intel(R) UHD Graphics 620; CPU: Intel64 Family 6 Model 142 Stepping 12, GenuineIntel; 8 logische Prozessoren.

| Buttons | Sasogine Frame ms | Myra Frame ms | Median-Sieger | Weniger Zeit | UI CPU ms S/M | Draw-Calls S/M | Bereiche überlappen |
|---:|---:|---:|---|---:|---|---|---|
| 100 | 0.946 | 0.892 | Myra | 5.8% | 0.848 / 0.814 | 200 / 200 | ja |
| 1000 | 11.994 | 11.498 | Myra | 4.1% | 11.812 / 11.330 | 2000 / 2000 | ja |
| 5000 | 65.738 | 63.852 | Myra | 2.9% | 65.479 / 63.628 | 10000 / 10000 | ja |

8 AB/BA-Runden, 60 gemessene echte Frames und 20 ungemessene Warmup-Frames je Block. Median der Block-Mittelwerte; Prozent relativ zur langsameren Zeit. Überlappung ist ein Hinweis auf Schwankungen, kein statistischer Signifikanztest.

## Ablauf

Ein gemeinsamer Release-Prozess und ein GraphicsDevice. Pro Tick ein UI-Update und ein Draw auf den normalen 1200 × 800 Backbuffer, danach reguläres Present. Kein RenderTarget-Wechsel, keine Screenshots, kein GetData/Readback und kein GPU-Drain innerhalb der Messblöcke. VSync aus, variabler Timestep und InactiveSleepTime=0. Alle Controls und Fonts werden vor dem Block aufgebaut und aufgewärmt. AB/BA-Reihenfolge wechselt je Runde. Während der Messung zeichnet ausschließlich die aktive Bibliothek; Ergebnisanzeige und Konsolenausgabe finden außerhalb der Blöcke statt.

Framezeit = reale Wandzeit vom Beginn des ersten gemessenen Updates bis nach Present des letzten Frames, geteilt durch Framezahl; einschließlich Ereignisschleife zwischen den Frames, Clear und möglicher Treiberwartezeiten. UI CPU = ausschließlich Aufrufzeiten von Root.Update + Root.Draw beziehungsweise Desktop.Render. Myras öffentliche Render-Methode enthält Layout, Input, Ereignisse und Zeichnen; keine künstliche Aufteilung oder Subtraktion. Draw-Calls und Zustandszähler werden direkt um den jeweiligen UI-Aufruf über GraphicsDevice.Metrics erfasst. Das sind API-Aufrufe, keine GPU-Zeitstempel. Present kann GPU-Arbeit verzögert abarbeiten; keine isolierte GPU-Zeit.

## Geltungsbereich

Unveränderte sichtbare UI: 100, 1.000 oder 5.000 Buttons mit je einem TextBlock/Label und zwei StackPanel-Ebenen. Identische Button-Geometrie und Elementzahl werden geprüft. Einfarbige Hintergründe, gleiche FontStashSharp-Fontinstanz (Segoe UI, Größe 10), keine Atlas-Skins, Klicks, Textänderungen oder Spiellogik. Maus steht synthetisch außerhalb; Keyboard leer. Sasogine nutzt den aktuellen Layout-/Text-Cache. Dies ist eine laufende UI-Spielschleife, kein vollständiges Spiel und keine allgemeine FPS-Aussage über Sasogine oder Myra. Betriebssystem, Treiber, Energiezustand und Hintergrundlast können Ergebnisse ändern. Frühere isolierte Draw-Messungen mit GPU-Readback haben eine andere Methodik und sind nicht direkt vergleichbar.

## SpriteBatch

Sasogine beendet und beginnt aktuell in ControlDrawContext.SetControl je Traversierungsabschnitt einen Batch; Root.DrawTree ruft dies zweimal pro sichtbarem Element auf. Das kann CPU-Verwaltung und Zustandswechsel erzeugen. Nicht jeder Begin/End-Aufruf verursacht einen GPU-Draw, etwa bei leeren Batches. Die gemessenen Draw-Call-Zahlen zeigen tatsächliche Grafik-API-Aufrufe, nicht die Anzahl der Begin/End-Paare. Gleiche Draw-Call-Zahlen beweisen keine gleichen CPU-Kosten. SpriteBatch ist grundsätzlich für gebündeltes 2D-Zeichnen ausgelegt; viele kleine Batches sind ein möglicher Engpass. Dessen genauer Zeitanteil wurde hier nicht durch eine Rendereränderung isoliert.

Myra 1.6.8; MonoGame DesktopGL 3.8.6; FontStashSharp 1.6.2; .NET TieredCompilation=0.

Controls SHA-256: `B5ED676B518E667D15669A6EFCC4159735CB9B70FC8218A43F9D952B643E1091`.

Myra SHA-256: `89FDF258917A14EA05FBA234EABA9D148FC8375EFF4D0698EE46AC6A415DB956`.

Font SHA-256: `74F2B3D0C20CF7380EB121A09FD7CDFDC1CCDD12A00DB83CAEC0FEB48B4DB9F7`.

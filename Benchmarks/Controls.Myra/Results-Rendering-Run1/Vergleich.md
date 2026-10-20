# Sasogine / Myra: nur Rendering

Gemessen: 2026-10-20T08:55:11.3189044+02:00. .NET 10.0.12; Microsoft Windows 10.0.22631; X64.

CPU: Intel64 Family 6 Model 142 Stepping 12, GenuineIntel; 8 logische Prozessoren. Grafik: Intel(R) UHD Graphics 620.

| Buttons | Sasogine ms | Myra ms | Median-Sieger | Abstand ms | Weniger Zeit | Bereiche überlappen |
|---:|---:|---:|---|---:|---:|---|
| 100 | 1.160 | 1.064 | Myra | 0.096 | 8.3% | ja |
| 1000 | 12.994 | 12.930 | Myra | 0.064 | 0.5% | ja |
| 5000 | 56.448 | 58.068 | Sasogine | 1.620 | 2.8% | ja |

8 abwechselnde AB/BA-Runden, 20 Draws pro Batch; 20 Warmup-Draws pro Bibliothek und Baumgröße. Angezeigt wird der Median der Batch-Mittelwerte. Prozent = (langsamere Zeit − kürzere Zeit) / langsamere Zeit. Überlappende Min/Max-Bereiche erlauben keinen belastbaren eindeutigen Sieger. Rohwerte: raw.csv; Min/Max: summary.csv.

Gemessen werden ausschließlich `Root.Draw` und `Desktop.RenderVisual`: CPU-Wandzeit einschließlich möglicher Treiberwartezeiten. Keine GPU-Zeit und keine Spiel-FPS. Aufbau, Layout, Eingaben, Clear, RenderTarget-Wechsel, Present, Ergebnisanzeige und Pixel-Readback liegen außerhalb der Zeitmessung. Vor und nach JEDEM Draw synchronisiert ein Pixel-Readback den Grafiktreiber; dadurch wird Warteschlangenaufbau begrenzt. Das ist ein isolierter Draw-Vergleich, keine Simulation eines kompletten Spieleframes.

Gleicher Prozess, GraphicsDevice und 1200 × 800 RenderTarget; VSync aus. Sichtbare Buttons mit identischer Geometrie, Farbe und je einem Textinhalt in gleichen StackPanel-Ebenen. Geometrie wird geprüft; PNGs dokumentieren beide Bäume. Beide verwenden dieselbe FontStashSharp-Fontinstanz in Größe 10. Sasogine wird aus aktuellen Controls-Quellen optimiert gebaut; Myra 1.6.8, MonoGame DesktopGL 3.8.6, FontStashSharp 1.6.2. Keine internen Rendereränderungen. Keine Aussage über komplexe Anwendungen oder andere Hardware; Hintergrundlast und thermische Effekte bleiben möglich.

TieredCompilation: 0. Schrift SHA-256: `74F2B3D0C20CF7380EB121A09FD7CDFDC1CCDD12A00DB83CAEC0FEB48B4DB9F7`.

Controls SHA-256: `BC766ED92422B6D78E923E74DB742943B09DC60FA6BE61BE91AA0F1F59B146A9`.

Myra SHA-256: `89FDF258917A14EA05FBA234EABA9D148FC8375EFF4D0698EE46AC6A415DB956`.

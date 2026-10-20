# Sasogine Controls / Myra – gemessener Vergleich

Gemessen: 2026-10-19T20:20:30.4056464+02:00. .NET 10.0.12, X64, Microsoft Windows 10.0.22631.

CPU: Intel64 Family 6 Model 142 Stepping 12, GenuineIntel; logische Prozessoren: 8. Grafikadapter (MonoGame): Intel(R) UHD Graphics 620.

Myra **1.6.8**, MonoGame DesktopGL **3.8.6**, FontStashSharp **1.6.2**; beide UIs im selben Release-Prozess. Sasogine Controls wird aus dem aktuellen Quellcode separat optimiert gebaut. Schrift: `segoeui.ttf`, Größe 10; SHA-256 `74F2B3D0C20CF7380EB121A09FD7CDFDC1CCDD12A00DB83CAEC0FEB48B4DB9F7`.

Runden: 7; Frames je Runde: 60; Warmup je Frame-Szenario und Runde: 60. Tiered Compilation: `0`. VSync aus, fester 1200 × 800 RenderTarget. Messung ohne Debugger starten.

Zeit je Operation als Median der Runden, mit Minimum/Maximum in `summary.csv`. Speicher: Median der auf dem ausführenden Thread neu allokierten verwalteten Bytes je Operation. Dies ist weder verbleibender RAM noch VRAM. Rohmessungen und GC-Zähler: `raw.csv`.

| Buttons | Operation | Sasogine µs | Myra µs | Sasogine B | Myra B | Myra / Sasogine |
|---:|---|---:|---:|---:|---:|---:|
| 100 | Construction | 116.562 | 632.638 | 224115.0 | 626328.0 | 5.43 |
| 100 | Draw | 1019.470 | 864.712 | 0.0 | 4440.0 | 0.85 |
| 100 | IdleFrame | 881.970 | 877.255 | 0.0 | 4440.0 | 0.99 |
| 100 | MovingPointerFrame | 910.867 | 886.730 | 0.0 | 4440.0 | 0.97 |
| 100 | ResizeFrame | 981.700 | 1040.670 | 4600.0 | 23760.0 | 1.06 |
| 100 | PropertyWrites | 0.030 | 0.047 | 0.0 | 64.0 | 1.57 |
| 1000 | Construction | 1517.275 | 10558.675 | 1717291.0 | 5464212.0 | 6.96 |
| 1000 | Draw | 11209.148 | 10518.263 | 0.0 | 41040.0 | 0.94 |
| 1000 | IdleFrame | 11116.598 | 13956.833 | 0.0 | 41040.0 | 1.26 |
| 1000 | MovingPointerFrame | 14043.722 | 16280.523 | 0.0 | 41040.0 | 1.16 |
| 1000 | ResizeFrame | 13642.387 | 19106.533 | 41200.0 | 205200.0 | 1.40 |
| 1000 | PropertyWrites | 0.040 | 0.064 | 0.0 | 64.0 | 1.61 |
| 5000 | Construction | 14340.650 | 77555.025 | 8251898.0 | 26805103.0 | 5.41 |
| 5000 | Draw | 59472.442 | 56754.518 | 0.0 | 202040.0 | 0.95 |
| 5000 | IdleFrame | 60518.133 | 56779.843 | 0.0 | 202040.0 | 0.94 |
| 5000 | MovingPointerFrame | 77440.470 | 72527.400 | 0.0 | 202040.0 | 0.94 |
| 5000 | ResizeFrame | 77457.030 | 71987.322 | 202200.0 | 989120.0 | 0.93 |
| 5000 | PropertyWrites | 0.040 | 0.071 | 0.0 | 64.0 | 1.78 |

Ein Verhältnis über 1 bedeutet eine kürzere gemessene Zeit für Sasogine; unter 1 für Myra. Daraus folgt kein allgemeiner Gesamtsieger.

## Was gemessen wurde

- `Construction`: Baum einschließlich Root/Desktop erstellen; erster Layoutdurchlauf und Freigabe sind ausgeschlossen. Schrift/Stylesheet sind bereits geladen. Sasogines automatische Registry gehört zur Root-Erstellung; Myra verwendet sein bereits initialisiertes Stylesheet. Es werden acht Bäume pro Runde gehalten und anschließend freigegeben.
- `Draw`: ausschließlich Root.Draw bzw. Desktop.RenderVisual mit vorbereitetem Layout.
- `IdleFrame`: Sasogine SampleReusable + Root.Update + Root.Draw gegenüber Myra Desktop.Render mit seinen Layout-, Eingabe-, Ereignis- und Zeichenschritten.
- `MovingPointerFrame`: derselbe Frame mit identischen synthetischen Mauspositionen ohne Klicks.
- `ResizeFrame`: zusätzlich Breite des ersten Buttons pro Frame um ein Pixel ändern. Keine neue Baumkonstruktion.
- `PropertyWrites`: ausschließlich zwei direkte Zuweisungen, Padding und Margin am ersten Button, abwechselnd 0/1. Layout und Draw sind ausgeschlossen.

## Grenzen

Alle Buttons und je ein Text-Content liegen sichtbar in zwei gleich aufgebauten StackPanel-Ebenen. Gleiche Geometrie und Elementzahl werden vor und nach den Messungen geprüft. Beide Renderer teilen dieselbe FontStashSharp-Fontinstanz. Einfarbige Hintergründe, keine Atlas-Skins, ScrollViewer, komplexen Templates, Dialoge oder DataTables. Myras RichText-Label und Sasogines TextBlock behalten ihre regulären Implementierungen; eine gleich aussehende Oberfläche bedeutet keine identischen internen Funktionen.

Gemessen wird CPU-Wandzeit beim Aufruf einschließlich möglicher Treiberwartezeiten. Es gibt keine GPU-Zeitstempel: Die Zahlen sind **keine GPU-Renderzeit und keine Spiel-FPS**. Present, VSync, RenderTarget-Wechsel, Löschen, Bildschirmaufnahmen und GPU-Drain liegen außerhalb der Messung. GPU-Drain per Pixel-Readback erfolgt vor/nach jedem Batch; während eines Batches kann Treiber-Backpressure entstehen. Eingaben sind vorgegebene Zustände, keine Betriebssystem-Abfrage. Touch-/Text-/Klick-/Fokus-Last, MVVM, AOT-Startkosten und aktive Stylesystemänderungen sind nicht gemessen.

AB/BA-Reihenfolge wechselt pro Runde. Ergebnisse hängen von Hardware, Treiber, Takt, Energieprofil und Hintergrundlast ab; mehrere vollständige Läufe vergleichen. Für belastbare Ausreißerbeurteilung `raw.csv` und Min/Max in `summary.csv` lesen. Keine Bibliothek wurde für den Benchmark intern verändert.

Sasogine Controls DLL SHA-256: `9880F4BB7CF239427217152476B704C45E7D02688A8F8AF598A3F8B205384E5D`.

Myra DLL SHA-256: `89FDF258917A14EA05FBA234EABA9D148FC8375EFF4D0698EE46AC6A415DB956`.

API-Quelle: [Myra 1.6.8](https://www.nuget.org/packages/Myra/1.6.8), [Desktop.cs, Paket-Commit](https://github.com/MyraUI/Myra/blob/813fca3f7004700903e4ad734789e2cd78acb2a9/src/Myra/Graphics2D/UI/Desktop.cs).

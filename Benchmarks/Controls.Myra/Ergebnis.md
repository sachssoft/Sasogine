# Rendering-Vergleich: Sasogine / Myra

Gemessen am 20.10.2026 auf Intel UHD Graphics 620, .NET 10 Release. Ausschließlich Draw-Aufrufe, ohne Aufbau, Layout oder Eingaben.

| Buttons | Sasogine | Myra | Median-Sieger | Weniger Zeit |
|---:|---:|---:|---|---:|
| 100 | 1,160 ms | 1,064 ms | Myra | 8,3 % |
| 1.000 | 12,994 ms | 12,930 ms | Myra | 0,5 % |
| 5.000 | 56,448 ms | 58,068 ms | Sasogine | 2,8 % |

Median aus acht Runden mit je 20 Draws. Bei allen Größen überlappen die beobachteten Messbereiche; daraus folgt kein belastbarer allgemeiner Sieger. CPU-Aufrufzeiten einschließlich möglicher Treiberwartezeiten, keine GPU-Zeiten oder Spiel-FPS.

[Methodik und Bericht](Results-Rendering-Run1/Vergleich.md) · [Rohwerte](Results-Rendering-Run1/raw.csv) · [Ergebnisbild](Results-Rendering-Run1/Ergebnis.png)

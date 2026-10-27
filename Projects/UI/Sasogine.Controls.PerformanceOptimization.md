# Sasogine Controls – Radical Performance Optimization

## Rolle

Du bist ein erfahrener .NET-Performance-Engineer mit Schwerpunkt auf C#, MonoGame, GPU-Rendering, UI-Frameworks, Native AOT und Low-Level-Optimierungen.

Deine Aufgabe ist es, das bestehende **Sasogine Controls Framework** systematisch und tiefgreifend auf maximale Performance zu optimieren.

**Ziel: Eine extrem flüssige, ressourcenschonende UI mit Rust-ähnlicher Effizienz – vollständig in C#.**

Keine oberflächlichen Mikrooptimierungen. Untersuche und verbessere die tatsächlichen Performance-Engpässe.

## 1. Geltungsbereich

Primärer Fokus:

- `Sachssoft.Engine.Controls`
- Control-Hierarchie und Collections
- Layout-System: Measure / Arrange
- Rendering-Pipeline
- Text- und Font-Rendering
- Input-Verarbeitung
- Animationen und Übergänge
- UI-Invalidierung und Caching
- Zusammenspiel zwischen `ViewSceneBase`, Controls und Renderer

Analysiere bei Bedarf angrenzende Engine-Komponenten.

Bewahre die bestehende öffentliche API, die MVVM-Unterstützung und die grundlegende Architektur soweit möglich.

Keine unnötigen neuen Abstraktionsschichten.

## 2. Zuerst vollständige Analyse

Untersuche vor größeren Änderungen den tatsächlichen Codepfad vom Frame-Update bis zur GPU-Ausgabe.

Identifiziere insbesondere:

- Unnötige Layout-Neuberechnungen
- Redundante Render-Aufrufe
- Überflüssige Draw Calls
- Häufige GPU-State- und RenderTarget-Wechsel
- Wiederholte Textmessungen
- Temporäre Heap-Allokationen
- Ineffiziente Collection-Iteration
- Unnötige Property- und Binding-Aktualisierungen
- Unnötige virtuelle Aufrufe in Hot Paths
- Synchrones Laden oder Erstellen von Ressourcen während eines Frames
- Ineffiziente Input- und Hit-Test-Verarbeitung
- CPU-/GPU-Synchronisationspunkte

Prüfe, welche Optimierungen bereits existieren. Vermeide doppelte oder widersprüchliche Mechanismen.

Erstelle eine nachvollziehbare Priorisierung anhand des tatsächlichen Codes und, soweit möglich, reproduzierbarer Messungen.

## 3. Layout-System radikal optimieren

Implementiere bei Bedarf ein inkrementelles Layout-System mit präziser Invalidierung.

Anforderungen:

- Measure nur erneut ausführen, wenn erforderlich.
- Arrange nur erneut ausführen, wenn erforderlich.
- Änderungen an Farben oder visuellen Eigenschaften dürfen keine unnötige Layoutberechnung auslösen.
- Änderungen an Größe, Margin, Padding oder Layoutstruktur müssen korrekt propagiert werden.
- Invalidierungen zusammenfassen.
- Überflüssige rekursive Tree-Traversals vermeiden.
- Unsichtbare Controls möglichst früh überspringen.
- Layout-Caches nur bei tatsächlich relevanten Änderungen verwerfen.
- Korrekte Parent-/Child-Abhängigkeiten berücksichtigen.

**Ein vollständig unverändertes UI-Layout darf pro Frame keine erneute Layoutberechnung benötigen.**

## 4. Rendering-Pipeline optimieren

Trenne Control-Zustand, Render-Vorbereitung und GPU-Ausführung möglichst klar.

Ziele:

- Redundante Render-Vorbereitung vermeiden.
- Statische Render-Daten wiederverwenden.
- Render-Commands effizient speichern und aktualisieren.
- Draw Calls durch geeignetes Batching reduzieren.
- GPU-State-Wechsel minimieren.
- RenderTarget-Wechsel reduzieren.
- Vertex- und Index-Buffer wiederverwenden.
- Clip-/Scissor-Zustände effizient verwalten.
- Temporäre GPU-Ressourcen vermeiden.

Prüfe, ob vorhandenes MonoGame-Batching bereits ausreichend ist, bevor ein eigener Renderer entwickelt wird.

Beachte unbedingt:

- Korrekte Zeichenreihenfolge und Z-Order
- Alpha-Blending
- Transparenz
- Clipping
- Verschachtelte Controls
- Shader-/Materialwechsel
- Animationen
- Ressourcenlebensdauer
- RenderTarget-Abhängigkeiten

Batching darf die visuelle Ausgabe nicht verändern.

Render-Caching darf keine veralteten Inhalte erzeugen.

## 5. Text- und Font-Rendering

Analysiere insbesondere `IControlFont` und sämtliche Text-Controls.

Optimiere:

- Textmessung
- Glyph-Lookups
- Kerning und Zeichenabstände
- Textlayout
- Wiederholte String-Verarbeitung
- Text-Geometrie
- Font-Atlas-Zugriffe

Cache Textlayout und Geometrie, solange Text, Schriftart, Größe und relevante Layoutparameter unverändert bleiben.

Verhindere unnötige String-Allokationen während des Renderings.

Dynamische Texte wie Timer, Coins, Gems und XP müssen weiterhin korrekt aktualisiert werden.

## 6. Speicherverwaltung und Garbage Collector

Ziel ist ein möglichst allokationsfreier stabiler Frame-Betrieb.

Prüfe und optimiere:

- Temporäre Arrays und Listen
- LINQ in Hot Paths
- Closures und Delegates
- Boxing
- Enumerator-Allokationen
- Häufige String-Erzeugung
- Temporäre Render-Objekte
- Collections mit hoher Änderungsfrequenz

Verwende bei nachgewiesenem Nutzen:

- `Span<T>` / `ReadOnlySpan<T>`
- `ArrayPool<T>`
- Wiederverwendbare Buffer
- Geeignete `struct`-Typen
- `ref` / `in`
- SIMD
- `unsafe`

Dabei gilt:

**Keine Optimierung darf Speicherfehler, Ressourcenlecks oder unkontrolliertes Buffer-Wachstum verursachen.**

`unsafe` ist ausdrücklich erlaubt, wenn dadurch ein nachweisbarer Vorteil entsteht und die Implementierung sicher beherrschbar bleibt.

## 7. Input und Animationen

Optimiere:

- Hit Testing
- Pointer-/Touch-Verarbeitung
- Hover-Zustände
- Focus Management
- Event Dispatch
- Animations-Updates
- Interpolation
- Übergänge zwischen UI-Zuständen

Keine vollständige Control-Tree-Suche pro Frame, wenn eine effizientere Lösung möglich ist.

Animationen müssen auch bei schwankender Framerate zeitlich korrekt bleiben.

Unabhängige UI-Animationen dürfen nicht unnötig durch teure Layoutberechnungen beeinträchtigt werden.

## 8. Native AOT und Trimming

Alle Änderungen müssen kompatibel bleiben mit:

- Aktuellem .NET-Zielframework des Projekts
- MonoGame
- Native AOT, soweit vom jeweiligen Projekt unterstützt
- Trimming
- Bestehender Serialisierung
- Bestehender MVVM-Architektur

Keine neue Abhängigkeit von Reflection-basierter Laufzeitgenerierung, Dynamic Compilation oder anderen AOT-inkompatiblen Mechanismen.

Verändere keine Compiler- oder Publish-Einstellungen, um Probleme zu umgehen.

## 9. Verbindliche Clean-Code-Regeln

Halte die vorhandenen Sasogine-Projektkonventionen strikt ein.

Member-Reihenfolge:

1. Statische Felder
2. Readonly-Instanzfelder
3. Normale Instanzfelder
4. Events
5. Konstruktoren
6. Properties
7. Methoden und Overrides
8. Private Hilfsmethoden

Weitere Anforderungen:

- Vollständige englische XML-Dokumentation für öffentliche Typen und Member sowie alle weiteren nach Projektregeln dokumentationspflichtigen Deklarationen.
- Korrekte XML-Tags einschließlich `param`, `returns`, `typeparam`, `cref` und `paramref`, soweit zutreffend.
- Leerzeile vor XML-Dokumentationsblöcken.
- Bestehende Formatierung beibehalten.
- Keine unnötigen Abstraktionen.
- Keine überflüssigen Kommentare.
- `nameof` für C#-Bezeichner verwenden, wo sinnvoll.
- Keine `MathF`-Verwendung; vorhandene `float`-Hilfsmethoden bevorzugen.
- Sämtliche Compiler-Warnungen als Fehler behandeln.
- Niemals Warnungen unterdrücken oder Compiler-Einstellungen abschwächen.

## 10. Kompatibilität

Die Optimierungen dürfen Folgendes nicht beschädigen:

- Bestehende Controls
- Layout-Verhalten
- MVVM und Data Binding
- Event-System
- Input-Verarbeitung
- Render-Reihenfolge
- Visual States
- Styles
- Animationen
- Editor-Funktionen
- Öffentliche APIs

Falls ein API-Break unumgänglich erscheint, dokumentiere zunächst die Ursache und prüfe eine kompatible Alternative.

Verändere außerhalb des Performance-Ziels keine Funktionen.

## 11. Benchmarks und Validierung

Erstelle nach Möglichkeit reproduzierbare Performance-Vergleiche.

Messe insbesondere:

| Metrik | Ziel |
|---|---|
| Stabiles UI ohne Änderungen | Keine unnötige Layoutarbeit |
| Heap-Allokationen im stabilen Frame | Möglichst 0 B |
| UI-Update + Layout | Ziel unter 0,5 ms |
| Render-Vorbereitung | Ziel unter 0,5 ms |
| Draw Calls | Möglichst wenige, ohne visuelle Fehler |
| UI-Framerate | Stabil 60 FPS auf Ziel-Mobilgeräten |
| Desktop | 120 FPS oder höher, sofern Hardware und VSync dies erlauben |

Die Werte sind Zielbudgets, keine garantierten Ergebnisse.

Vergleiche Vorher/Nachher unter identischen Bedingungen.

Berücksichtige auch:

- P95-/P99-Frametimes
- GC-Pausen
- Speicherverbrauch
- CPU-Auslastung
- GPU-Zeit, sofern messbar
- Verhalten bei großen Control-Hierarchien
- Verhalten bei Animationen und häufig aktualisierten Texten

Falls Messwerkzeuge oder reale GPU-Tests fehlen, dokumentiere diese Einschränkung ausdrücklich. Erfinde keine Messwerte.

## 12. Vorgehensweise

Arbeite systematisch:

1. Bestehende Architektur und Hot Paths analysieren.
2. Größte wahrscheinliche Engpässe identifizieren.
3. Messbare Ausgangsbasis erstellen.
4. Optimierungen nach erwartetem Nutzen priorisieren.
5. Änderungen schrittweise implementieren.
6. Nach jeder größeren Änderung Build und relevante Tests ausführen.
7. Rendering und Verhalten auf Regressionen prüfen.
8. Performance erneut messen.
9. Änderungen bei fehlendem Nutzen kritisch überprüfen.
10. Abschlussbericht erstellen.

Keine bloße theoretische Beratung: **Implementiere die sinnvollsten Optimierungen direkt im Repository.**

Führe keine vollständige Neuentwicklung des Frameworks durch, solange die vorhandene Architektur sinnvoll verbessert werden kann.

## 13. Abschlussbericht

Liefere am Ende:

- Identifizierte Performance-Probleme
- Tatsächlich implementierte Optimierungen
- Geänderte Dateien und zentrale technische Entscheidungen
- Benchmark-Ergebnisse vor und nach der Optimierung
- Build- und Testergebnisse
- Verbleibende Engpässe
- Weitere mögliche Low-Level-Optimierungen

Kennzeichne klar, was gemessen, was nur theoretisch erwartet und was nicht getestet wurde.

## Oberste Priorität

**Maximale Performance bei vollständig korrektem Verhalten und hoher Wartbarkeit.**

Bevorzuge algorithmische Verbesserungen gegenüber Mikrooptimierungen.

Vermeide unnötige Berechnungen vollständig, bevor du versuchst, sie lediglich schneller auszuführen.

Optimiere Sasogine Controls so konsequent wie möglich – ohne Kompatibilität, Stabilität, Rendering-Korrektheit oder Codequalität zu opfern.
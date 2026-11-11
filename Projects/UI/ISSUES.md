# Sasogine Controls – Comprehensive Performance Optimization

## Ziel

Sasogine Controls benötigt eine umfassende und strenge Performance-Optimierung des gesamten UI-Frameworks.

Das wichtigste Ziel ist eine **butterweiche, flüssige und reaktionsschnelle Benutzeroberfläche**, auch bei komplexen Layouts, vielen UI-Elementen, Animationen und umfangreichen Datenmengen.

Aktuell treten insbesondere beim Wischen und Scrollen auf Desktop und Android deutliche Ruckler und kurzzeitige Hänger auf, sobald eine Seite viele Controls enthält.

Die Optimierung darf sich nicht ausschließlich auf `ScrollViewer` oder `ListBox` beschränken.

## 1. Kritische Performanceprobleme beheben

Untersuche systematisch das gesamte UI-Framework auf Performance-Engpässe.

Besonders kritisch sind sämtliche Controls mit Swipe-, Drag-, Scroll- oder Gestensteuerung:

- `ScrollViewer`
- `Carousel`
- `ListBox`
- `ItemsControl`
- `Slider`
- Weitere interaktive und animierte Controls

Diese Controls müssen auch bei umfangreichen und verschachtelten UI-Hierarchien möglichst flüssig reagieren.

### Anforderungen

- Unnötige Layout-Neuberechnungen vollständig vermeiden.
- `Measure()` und `Arrange()` nur bei tatsächlichem Bedarf ausführen.
- Scroll- und Swipe-Bewegungen möglichst ohne vollständige Layout-Invalidierung verarbeiten.
- Input-Verarbeitung und Hit-Testing optimieren.
- Animationen und Übergänge effizient aktualisieren.
- Unnötige Speicherallokationen pro Frame vermeiden.
- GC-bedingte Ruckler minimieren.
- Rendering, Batching, Clipping und Culling optimieren.
- Unveränderte und unsichtbare Controls möglichst nicht unnötig verarbeiten.
- Frame-Time-Spitzen reduzieren und gleichmäßiges Frame-Pacing sicherstellen.

## 2. Gemeinsame Performance-Engpässe untersuchen

Besonders folgende Bereiche überprüfen:

- `Control.OnPropertyChanged()`
- `InvalidateMeasure()`
- `InvalidateArrange()`
- `Workspace.Layout()`
- `Workspace.PrepareTree()`
- `Workspace.RebuildControls()`
- `ScrollViewer.Pan()`
- `RangeControl.ConfigValue`
- `ItemsControl.SynchronizeItems()`

Prüfen, ob Property-Änderungen unnötig vollständige Layout-Durchläufe auslösen.

Insbesondere darf eine reine Scroll- oder Swipe-Bewegung nicht ohne tatsächlichen Bedarf die gesamte UI-Hierarchie neu vermessen.

Gemeinsame Ursachen zentral beheben, damit sämtliche betroffenen Controls davon profitieren.

## 3. Carousel und Gestensteuerung besonders optimieren

`Carousel` und sämtliche vergleichbaren Controls mit Wischgesten benötigen besondere Aufmerksamkeit.

### Anforderungen

- Butterweiche Swipe-Bewegungen und Animationen.
- Keine unnötigen Layout-Berechnungen während einer Geste.
- Effiziente Positionsänderungen und Transformationen.
- Möglichst geringe CPU- und GPU-Belastung.
- Keine unnötige Aktualisierung außerhalb des sichtbaren Bereichs.
- Korrekte Touch- und Maussteuerung.
- Flüssige Übergänge auch bei komplexen Child-Controls.
- Keine kurzfristigen Hänger beim Wechsel zwischen Elementen.

Vorhandene gemeinsame Input-, Layout- und Rendering-Systeme verwenden und gezielt optimieren.

## 4. Virtualized Controls implementieren

Zusätzlich echte UI-Virtualisierung mit Container-Recycling einführen.

Geplante Controls:

- `VirtualListBox`
- `VirtualItemsControl`
- `VirtualDataTable`
- `VirtualTreeView`
- `VirtualGrid`

### Anforderungen

- Nur sichtbare Elemente und einen kleinen Puffer materialisieren.
- Nicht mehr sichtbare Item-Controls effizient wiederverwenden.
- Keine vollständige Erzeugung großer UI-Listen.
- Datenbindungen, Templates, Selektion und Fokus korrekt erhalten.
- Dynamische Collection-Änderungen unterstützen.
- Virtualisierung ohne unnötige Architekturkomplexität implementieren.

Zunächst `VirtualListBox` priorisieren und anschließend weitere Virtual-Controls auf Grundlage wiederverwendbarer Komponenten entwickeln.

## 5. Gesamtes UI-Framework optimieren

Nicht nur einzelne Problemstellen beheben, sondern alle bestehenden Controls und gemeinsamen Komponenten systematisch überprüfen.

Dazu gehören:

- Layout-System
- Rendering-System
- Input- und Gestenverarbeitung
- Animationen
- Datenbindungen
- Templates
- Events
- Speicherverwaltung
- Control-Hierarchien
- Scroll- und Viewport-Verwaltung

Vorhandene Optimierungen und Caches berücksichtigen. Keine unnötigen parallelen Systeme einführen.

## 6. Performance-Tests und Benchmarks

Vor und nach den Optimierungen reproduzierbare Tests durchführen.

Testfälle:

- 50 bis 1.000 UI-Controls auf einer Seite.
- Komplexe verschachtelte Layouts.
- Kontinuierliches Scrolling und Swiping.
- Carousel mit umfangreichen Child-Controls.
- Gleichzeitige Animationen und Eingaben.
- Virtualisierte Listen mit 10.000 bis 100.000 Einträgen.

Messen:

- Durchschnittliche Framezeit.
- P95/P99-Framezeiten und maximale Framezeit.
- Layout-Dauer und Anzahl der Layout-Durchläufe.
- Draw-Calls.
- GC-Allokationen pro Frame.
- CPU- und Speicherverbrauch.
- Anzahl materialisierter UI-Elemente.

Ziel sind stabile 60 FPS oder mehr, soweit die Hardware dies ermöglicht. Gleichmäßige Framezeiten sind wichtiger als hohe durchschnittliche FPS.

## 7. Technische Vorgaben

- Bestehende Architektur beibehalten.
- Keine unnötigen Abstraktionen einführen.
- Bestehende Public APIs möglichst erhalten.
- Native AOT und Trimming vollständig unterstützen.
- Reflection vermeiden.
- Keine unnötigen externen Abhängigkeiten.
- Öffentliche Typen und Member vollständig mit XML dokumentieren.
- Bestehende Bindings, Templates und Events nicht beschädigen.
- DesktopGL und Android berücksichtigen.
- Regressionstests ergänzen.

## 8. Reihenfolge der Umsetzung

1. Gesamtes UI-System analysieren und Engpässe durch Profiling identifizieren.
2. Kritische Probleme in Layout, Rendering und Input-Verarbeitung beheben.
3. ScrollViewer, Carousel und sämtliche Gesten-Controls gezielt optimieren.
4. Weitere bestehende Controls systematisch optimieren.
5. Virtualisierung und Recycling implementieren.
6. Performance-Benchmarks und Regressionstests durchführen.
7. Ergebnisse vergleichen und verbleibende Engpässe dokumentieren.

## Akzeptanzkriterien

- Butterweiche Swipe-, Scroll- und Drag-Interaktionen.
- Flüssige Animationen und Übergänge.
- Deutlich reduzierte Frame-Time-Spitzen.
- Keine unnötigen vollständigen Layout-Durchläufe.
- Effiziente Verarbeitung komplexer UI-Hierarchien.
- Virtualisierte Controls für große Datenmengen.
- Deutlich geringere CPU-, GPU- und Speicherbelastung, soweit messbar.
- Keine funktionalen Regressionen.
- Native-AOT-Kompatibilität bleibt erhalten.

## Wichtige Anweisung

**Performance hat bei dieser Aufgabe höchste Priorität.**

Untersuche nicht nur die bereits bekannten Engpässe, sondern identifiziere eigenständig weitere ineffiziente Implementierungen im gesamten Sasogine-Controls-Projekt.

Optimiere bestehende Komponenten gründlich, aber vermeide unbegründete Architekturänderungen.

Jede wesentliche Optimierung muss nachvollziehbar und möglichst durch Messungen bestätigt werden.

Das langfristige Ziel ist ein äußerst effizientes UI-Framework mit durchgehend butterweichen Interaktionen auf Desktop und Mobile.
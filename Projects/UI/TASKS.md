# Auftrag: Sasogine Controls erweitern und bereinigen

## Ziel

Sasogine Controls soll fehlende Controls für Desktop und Mobile erhalten. Im Mittelpunkt stehen **MVVM**, **Native AOT**, Touch-Bedienung sowie die Wiederverwendung vorhandener Sasogine-Funktionen.

**Grundregel:** Bestehende Architektur und Implementierungen erhalten. Nur gezielt erweitern; keine unnötigen Refactorings oder parallelen Abstraktionen einführen.

## 1. Fehlende Controls

### Carousel

- Für Mobile optimiert, auch auf Desktop verwendbar.
- Horizontale und vertikale Ausrichtung (`Orientation`).
- Swipe- und Drag-Gesten mit optionalem animiertem Einrasten.
- Optionale Navigation über Pfeilschaltflächen, abhängig von der Ausrichtung.
- `ItemsSource`, `ItemTemplate`, `SelectedIndex` und `SelectedItem` nach dem vorhandenen MVVM-Muster.
- Optional zyklische Navigation und Seitenindikatoren.
- Verhalten bei leerer Collection oder dynamischen Änderungen sauber behandeln.

### TabControl

- Bestehendes `TabControl` nur erweitern.
- Position des Tab-Strips konfigurierbar: `Top`, `Bottom`, `Left`, `Right`.
- Orientierung und Layout des Tab-Strips passend zur Position.
- Bestehende Auswahl-, Template-, Fokus- und Styling-Mechanismen erhalten.

### ItemsControl und ComboBox

- Vorhandenes `ItemsControl` prüfen und bei Bedarf als gemeinsame Grundlage erweitern.
- `ComboBox` mit `ComboBoxItem` und Template-Unterstützung.
- `ItemsSource`, `ItemTemplate` sowie ein passendes Content-/Selection-Template gemäß bestehender Architektur.
- `INotifyCollectionChanged` und MVVM-Bindings berücksichtigen.
- Maus-, Tastatur-, Gamepad- und Touch-Bedienung beachten.
- Keine zweite Items-/Template-Infrastruktur implementieren.

## 2. Weitere Mobile-Controls prüfen

Nur implementieren, wenn kein geeignetes vorhandenes Control existiert:

| Control | Zweck |
| --- | --- |
| `PageIndicator` | Seitenanzeige für Carousel und paginierte Ansichten |
| `SegmentedControl` | Auswahl zwischen wenigen Optionen |
| `BottomSheet` | Touchfreundliches Panel vom unteren Bildschirmrand |
| `Switch` | Touchfreundlicher Ein-/Aus-Schalter |
| `RangeSlider` | Auswahl eines Wertebereichs |
| `SwipeView` | Kontextaktionen durch Wischgesten |
| `RefreshView` | Optionales Pull-to-Refresh |
| `CollectionView` | Größere Listen mit möglicher Virtualisierung |

Vorhandene `Slider`, `ScrollViewer`, `Popup`, `Dialog`, `TextBox` und Navigationscontrols bevorzugt erweitern statt ersetzen.

## 3. Plattformabhängige Texteingabe

**Sasogine stellt keine eigene virtuelle Tastatur bereit.**

- **Desktop:** Physische Tastatur über die vorhandene Texteingabe.
- **Android:** Native Android-Bildschirmtastatur beziehungsweise System-IME.
- **iOS:** Native Apple-Bildschirmtastatur.
- **Sasogine Controls:** Nutzt vorhandene Plattformdienste und Schnittstellen, um die systemeigene Texteingabe anzufordern.
- `TextBox` soll Fokus, Öffnen und Schließen der nativen Tastatur, Texteingabe und gegebenenfalls mehrzeilige Eingabe korrekt behandeln.
- IME, Kompositionstext, Auswahl und Eingabemethoden beachten, soweit von den vorhandenen Plattformdiensten unterstützt.
- Neue Interfaces wie `ITextInputService` nur dann einführen, wenn Sasogine keine geeignete Schnittstelle besitzt.

**Nicht erlaubt:** Eine eigene `VirtualKeyboard`-Klasse oder ein von Sasogine gezeichnetes Tastaturlayout als Ersatz für Android/iOS.

## 4. Gemeinsame Anforderungen

- **Native AOT und MVVM sind gleichrangige Hauptziele.** Reflection möglichst vermeiden und bestehende Binding-Mechanismen nutzen.
- `DataContext`, Commands, Templates, `ItemsSource` und `ObservableCollection<T>` passend zum jeweiligen Control berücksichtigen.
- Desktop-Eingaben (Maus, Tastatur) und Mobile-Eingaben (Touch) unterstützen; Gamepad dort, wo das vorhandene System dies vorsieht.
- Fokusverwaltung, Eingabezustände, Touch-Zielgrößen und Styles konsistent halten.
- Vorhandene Sasogine-Typen, Enums, Interfaces und Services wiederverwenden.
- Vollständige, korrekte englische XML-Dokumentation für öffentliche Typen und Member; keine Compiler-Warnungen unterdrücken.
- Bestehenden Codestil, Member-Reihenfolge und Architektur beibehalten.

## 5. Redundante Typen bereinigen

- Klassen, Enums und Interfaces auf Überschneidungen mit bestehenden Sasogine-Bestandteilen prüfen.
- Vor einer Entfernung sämtliche Referenzen und Auswirkungen analysieren.
- Nur nachgewiesen redundante Typen entfernen; keine Funktionen oder Kompatibilität unbeabsichtigt verlieren.
- Betroffene Löschungen und Breaking Changes ausdrücklich dokumentieren.
- Kein großflächiger Umbau ohne konkrete Notwendigkeit.

## 6. Vorgehensweise

1. Vorhandene Controls, Schnittstellen, Templates und Eingabedienste erfassen.
2. Fehlende Funktionen und Redundanzen auflisten.
3. `ItemsControl`/Templates gezielt prüfen und ergänzen.
4. `ComboBox`, `TabControl` und `Carousel` minimal-invasiv umsetzen.
5. Mobile-Texteingabe an die nativen Betriebssystemtastaturen anbinden.
6. Weitere Mobile-Controls nach Bedarf ergänzen.
7. Redundanzen erst nach Verwendungsprüfung entfernen.
8. Build mit aktivierten Warnings-as-Errors sowie AOT-relevante Pfade prüfen und Änderungen dokumentieren.

## Abnahmekriterien

- Bestehende Controls funktionieren unverändert weiter, sofern keine ausdrücklich dokumentierte Änderung erforderlich ist.
- Carousel, TabControl und ComboBox erfüllen ihre beschriebenen Anforderungen.
- Mobile-Textfelder verwenden die native Android- beziehungsweise iOS-Tastatur.
- Keine eigene virtuelle Sasogine-Tastatur und keine unnötigen Doppelimplementierungen.
- MVVM, Native AOT, Stilkonventionen und XML-Dokumentation werden eingehalten.

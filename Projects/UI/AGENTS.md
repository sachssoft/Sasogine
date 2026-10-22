# Sasogine Controls – Codex-Konventionen

Diese Regeln gelten für das Controls-Projekt. Bestehende Architektur und APIs haben Vorrang vor allgemeinen Framework-Mustern. Vor Änderungen betroffene Basisklassen, vorhandene Hilfsfunktionen und Tests untersuchen. Keine unnötigen Neuentwicklungen oder großflächigen Refactorings.

## Struktur und Code-Stil

- Ein benannter Typ pro passend benannter C#-Datei. Keine verschachtelten Typen oder Partial-Klassen.
- Vorhandene Namespaces beibehalten: `Sachssoft.Engine.Controls` sowie `Styling`, `Layouts`, `Input`, `Drawing`, `Binding`.
- Direkte, klar benannte Control-Properties; keine separaten Control-Konfigurationsklassen oder Attached-Grid-Properties.
- C# kompakt, sauber und gut lesbar formatieren: kurze Ausdrücke in einer Zeile, komplexe Bedingungen und Kontrollfluss sinnvoll umbrechen, logische Abschnitte mit Leerzeilen trennen. Keine unnötigen Abstraktionen oder Duplikate.
- Bestehende öffentliche Verträge möglichst erhalten; Breaking Changes nur bei nachgewiesenem Bedarf.
- Keine Platzhalter, stillschweigend ignorierten Fehler oder ungetesteten Erfolgsmeldungen.

## Properties und Styling

- `Element` und vorhandenes `INotifyPropertyChanged`-Muster nutzen: `SetNotify`, gecachte `PropertyChangedEventArgs`, keine überflüssigen Änderungsmeldungen.
- `StyleProperty<T>` für Stylewerte und lokale Overrides verwenden; direkt zugängliche Properties beibehalten.
- `ApplyStyle` explizit überschreiben und Basisklassenimplementierung aufrufen.
- Änderungen gezielt invalidieren, nicht pauschal das gesamte Layout oder Rendering aktualisieren.

## Control-Hierarchie, Items und Templates

- Vorhandenes `ContentControl` weiterverwenden, nicht parallel neu implementieren.
- Fehlende gemeinsame Grundlagen gezielt ergänzen: `HeaderedContentControl`, `ItemsControl` und nur bei echtem Bedarf `HeaderedItemsControl`.
- `GroupBox`, `Expander`, `ListBox`, `ComboBox`, `TreeView` und ähnliche Controls auf gemeinsame Mechanismen prüfen; ihre bestehenden Funktionen und Optimierungen erhalten.
- `ItemsControl`: `Items`, `ItemsSource`, `ItemTemplate` und bei Bedarf `ItemsPanel` klar definieren; direkte Items und Datenquelle dürfen nicht widersprüchlich sein.
- Quellen über passende allgemeine Schnittstellen anbinden, insbesondere `IEnumerable` und `INotifyCollectionChanged`. Keine Abhängigkeit von `ObservableCollection<T>` vorschreiben: Sasogine besitzt bereits umfangreichere Collection-Funktionen.
- Collection-Änderungen (`Add`, `Remove`, `Replace`, `Move`, `Reset`) korrekt und möglichst inkrementell verarbeiten; keine unnötige Neuerzeugung aller Items.
- Templates über ein schlankes `ITemplate`-Konzept und explizite, typisierte Factory-Delegates. Bei Bedarf `ContentTemplate`, `HeaderTemplate` und `ItemTemplate` unterscheiden. Keine Reflection oder versteckte Aktivierung.
- Eigentümerschaft, Parent/Child-Beziehungen, DataContext, Templatewechsel und Abmeldung von Events eindeutig behandeln.
- Sichtbarkeitsoptimierung, Virtualisierung und Recycling bestehender Controls nicht durch eine allgemeine ItemsControl-Implementierung verschlechtern. Nicht jedes Daten-Item muss zwingend ein vollständiges Child-Control erzeugen.

## Schlanke MVVM-Schnittstellen

- `INotifyPropertyChanged`, `INotifyCollectionChanged` und `ICommand` unterstützen; bestehende Sasogine-Binding-Funktionen wiederverwenden.
- `DataContext` nur als notwendige Grundlage für Controls und Templates bereitstellen, einschließlich nachvollziehbarer Vererbung und Aktualisierung.
- Keine eigene umfangreiche MVVM-, Reactive- oder Expression-Engine erstellen. Komplexe MVVM-Aufgaben übernehmen externe Lösungen wie ReactiveUI oder Sasopulse.
- Keine unnötigen Binding-Interfaces und keine spezifische `ObservableCollection`-Pflicht.
- Event-Abonnements beim Wechsel von Quellen, Controls oder Templates zuverlässig lösen.

## AOT, Trimming und Performance

- Native AOT und Trimming sind feste Architekturvorgaben. Keine Runtime-Reflection, Member-Discovery, `Activator.CreateInstance` für unbekannte Typen oder Runtime-Codegenerierung.
- Explizite Delegates und vorhandene registrierte Factories bevorzugen; keine neuen Reflection-Abhängigkeiten über Templates, Bindings oder Serialisierung einführen.
- `Workspace.Update` verarbeitet Input und Bindings in festgelegter Reihenfolge; `Draw` darf keine Zustands-, Binding- oder Layoutänderungen auslösen.
- Measure/Arrange, Dirty Flags, Caching, Clipping, Batching und vorhandene Renderer-Optimierungen beachten.
- SpriteBatch nicht pauschal verbieten, sondern nur sinnvoll einsetzen; Effizienz anhand von Messungen bewerten.
- Im wiederkehrenden Update-/Draw-Pfad unnötige Allokationen und Arbeit vermeiden.
- Keine unbelegten Performance-Vergleiche mit Myra, Gum oder ImGui als Tatsachen darstellen.

## Vollständige XML-Dokumentation

- Alle öffentlichen und geschützten Typen, Konstruktoren, Member, Events und Enum-Werte auf **Englisch** mit aussagekräftiger XML-Dokumentation versehen.
- `<summary>` erklärt Zweck und tatsächliches Verhalten, nicht nur den Namen. Passende `<param>`, `<typeparam>`, `<returns>`, `<exception>`, `<remarks>` und ggf. `<example>`/`<see>` ergänzen.
- Bei Properties insbesondere Änderungsverhalten, null-Werte, Vererbung oder Einschränkungen erläutern, wenn relevant.
- Bei Overrides und Interface-Implementierungen `<inheritdoc/>` nur verwenden, wenn die geerbte Dokumentation tatsächlich ausreichend ist.
- Dokumentation nach jeder Verhaltensänderung synchron halten.

## Prüfung und Vorgehensweise

- Vor Umsetzung nach bestehenden äquivalenten Mechanismen suchen. Kleinste passende Änderung wählen.
- Build, relevante Tests und vorhandenes Layout-/Allokations-Prüfprogramm ausführen, soweit in der Umgebung möglich.
- Besonders Collection-Änderungen, Items- und Templatewechsel, DataContext, Commands sowie Event-Abmeldungen prüfen.
- Native AOT durch **tatsächlichen NativeAOT-Publish** prüfen; `<IsAotCompatible>true</IsAotCompatible>` allein ist kein Nachweis. Trimming-Warnungen untersuchen statt unterdrücken.
- Performance getrennt für Erstellung, Layout, Update und Draw messen; z. B. 1.000 und 5.000 Controls mit reproduzierbarem Aufbau. Werte nur als Benchmark bezeichnen, wenn wirklich gemessen.
- Abschließend kurz nennen: geänderte Dateien, umgesetztes Verhalten, ausgeführte Prüfungen und offene Punkte.

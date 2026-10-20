# Controls-Konventionen

- Ein benannter Typ pro passender C#-Datei; keine verschachtelten Typen oder Partial-Klassen.
- Direkte Control-Properties. Keine separaten Control-Konfigurationsklassen, keine angehängten Grid-Felder.
- Element und INotifyPropertyChanged; cached PropertyChangedEventArgs und SetNotify.
- StyleProperty<T> für Stylewerte/lokale Overrides; direkte Getter, explizites ApplyStyle mit Basisklassenaufruf.
- Keine Reflection, Member-Discovery oder Laufzeit-Codegenerierung. Optionale Factories sind explizite Delegates.
- Workspace.Update verarbeitet geordneten Input und Bindings; Draw verändert keinen Zustand.
- Namespaces: Controls sowie Styling, Layouts, Input, Drawing und Binding. Englische XML-Dokumentation.
- Layout und Allokationen mit dem Prüfprogramm prüfen; AOT mit tatsächlichem NativeAOT-Publish validieren.

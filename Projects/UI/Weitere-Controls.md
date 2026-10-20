# Weitere Controls

Alle Controls besitzen direkte Properties und erben INotifyPropertyChanged über Element. [Architektur.md](Architektur.md) beschreibt Styles, MVVM, AOT und Workspace.

- DataTable: Spalten mit typisierten Getter-/Setter-Delegates, Zeilenauswahl, Strg-/Umschalt-Mehrfachauswahl, Strg+A, sortierbare/veränderbare Spalten und Zellbearbeitung. Items/Rows gehören der Tabelle.
- TreeView: TreeViewItem mit Text, Value, Items, IsExpanded, IsEnabled und Icon. SelectedItem verwendet Identität. Einzelauswahl, Tastaturnavigation, zwei ScrollBars; Zyklen und mehrfach verwendete Knoten werden abgelehnt.
- RepeatButton: sofortige Aktivierung und Wiederholung aus der übergebenen Framezeit, ohne eigene Timer.
- NumericSpinner: decimal-Werte, Grenzen, Increment, DecimalPlaces und Kultur. Eigene Styleschlüssel NumericMinimum, NumericMaximum, NumericValue und NumericIncrement. Ein gehaltenes Pfeil-Control wiederholt; ungültiger Text verändert den Wert nicht.
- Calendar/DatePicker: Kalenderwochenraster, Schaltjahre, Begrenzungen und kulturabhängige Anzeige. Today wird ausdrücklich übergeben. DatePicker hat ein Workspace-Popup.
- PasswordBox: verpflichtende Maskierung, kein Klartext über Copy/Cut. Strings bleiben normaler verwalteter Speicher.
- ToolTip/ToolTipContent: auf jedem Control; auch zusammengesetzter Inhalt und ScrollViewer sind möglich.
- Spinner: Animation ausschließlich über die von Workspace übergebene Zeit.
- RotationControl: Kreisregler in Grad, 0° rechts, positive Winkel im Uhrzeigersinn; Maus/Touch, Tastatur, Umschalt-Snapping und Abbruch. AngleDegrees, AngleIncrement und AngleSnap sind stylefähig. Command und CommandParameter werden bei Abschluss berücksichtigt.

Die Galerie enthält `--page advanced`, der 2D-Editor verwendet den RotationControl im Inspector. Der Editor speichert Rotation im JSON-Format Version 2 und liest ältere Dateien der Version 1 mit 0° weiter.

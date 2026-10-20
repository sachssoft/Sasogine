# CurveControl und EditableCurve

CurveControl ist ein interaktiver Kurveneditor und eine Vorschau für Sasogine CurveBase. Architektur: [Architektur.md](Architektur.md).

Ohne Curve-Zuweisung besitzt das Control eine eigene EditableCurve. Andere Engine-Kurven erscheinen als schreibgeschützte Vorschau. IsReadOnly sperrt Bearbeitung. Interpolation unterstützt Step, Linear und Monotone; die monotone kubische Interpolation vermeidet Überschwingen zwischen benachbarten Punkten.

Punkte ziehen; Strg+Klick oder Doppelklick fügt hinzu. Entf oder Rechtsklick entfernt innere Punkte. Umschalt rastet auf SnapX/SnapY ein. Pfeiltasten ändern den gewählten Punkt. Strg+Z/Y: Undo/Redo. Mausrad zoomt um den Zeiger; mittlere Taste oder Alt+Ziehen verschiebt die Ansicht. Home setzt die Ansicht zurück, F passt sie an. Escape und verlorenes Capture setzen den aktuellen Drag zurück.

AddPoint, MoveSelectedPoint, RemoveSelectedPoint, Undo, Redo, SetView, ResetView und FitToCurve sind direkt verfügbar. Ereignisse: CurveChanged, SelectionChanged, ViewChanged und EditStarted/Completed/Cancelled. Eine abgeschlossene Bearbeitung kann Command mit CommandParameter ausführen.

EditableCurve erbt CurveBase und implementiert INotifyPropertyChanged. Points ist schreibgeschützt; AddPoint, SetPoint, RemovePoint und ReplacePoints prüfen Werte und Reihenfolge. X liegt in 0–1 mit festen Endpunkten; Y darf endlich außerhalb 0–1 liegen. Ein unveränderter Auswertungsaufruf benötigt keinen neuen Speicher. Änderungen am Modell invalidieren das angeschlossene CurveControl ausdrücklich über Benachrichtigungen. Workspace.Dispose löst diese Verbindung.

Stylewerte: ShowGrid, ShowPoints, SnapX, SnapY, CurveLineThickness, PointRadius, GridColor, CurveColor, PointColor, CurveViewMinimum und CurveViewMaximum. String-Converter sind explizit und AOT-kompatibel.

Die Galerie öffnet mit `--page curve`. Der dortige Joystick verwendet dieselbe EditableCurve als ResponseCurve. In der Windows-Ausgabe startet `Curve editor starten.cmd` diese Seite.

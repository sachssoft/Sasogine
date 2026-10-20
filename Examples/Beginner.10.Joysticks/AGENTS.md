# Joystick-Beispiel

- Namespace: `Sachssoft.Engine.Controls.Examples`.
- Pro C#-Datei genau ein Typ; keine verschachtelten Typen oder über mehrere Dateien verteilten Partial-Klassen.
- Keine Reflection. Direkte Properties und String-Styles verwenden; Workspace initialisiert die Standard-Registry automatisch.
- Eingabe einmal pro Update erfassen. `Workspace.Update` vor `Workspace.Draw` ausführen und denselben Font, Viewport und Scale verwenden.
- Assets werden aus dem Controls-Projekt referenziert. Keine Systemfonts in das Repository kopieren.

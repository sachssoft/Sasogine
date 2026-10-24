# Window: Inhaltsgröße und Schließen-Button

```csharp
var window = new Window
{
    Title = "Einstellungen",
    SizeToContent = SizeToContent.WidthAndHeight,
    CloseButtonPlacement = CloseButtonPlacement.Left,
    ShowCloseButton = true,
    IsCloseButtonEnabled = true,
    Content = settingsPanel
};
workspace.ShowWindow(window);
```

## SizeToContent

| Modus | Verhalten |
|---|---|
| `Manual` | Bisherige Größenwahl über `Width` und `Height`; Standard. |
| `Width` | Breite aus dem Inhalt, konfigurierte Höhe bleibt erhalten. |
| `Height` | Höhe aus dem Inhalt, konfigurierte Breite bleibt erhalten. |
| `WidthAndHeight` | Beide Dimensionen folgen dem Inhalt. |

Auf automatisch berechneten Achsen werden die eingestellten `Width`-/`Height`-Werte ignoriert, aber gespeichert. Ein Wechsel zurück zu `Manual` aktiviert sie wieder. Titelzeile, Titeltext, Schließen-Bereich, Padding und Mindestgrößen fließen in die Berechnung ein. Die Standard-Mindestgröße bleibt erhalten; bei Bedarf `MinWidth` und `MinHeight` anpassen.

Änderungen am Inhalt aktualisieren ein bereits geöffnetes Fenster. Der Workspace hält Overlays innerhalb seines Viewports. Zu breiter umbruchfähiger Inhalt wird unter dieser Breitenbegrenzung neu gemessen. Beim manuellen Ziehen des Resize-Griffs wechselt das Fenster zu `Manual` und verwendet die aktuell sichtbare Größe als Ausgangspunkt. Dialoge erben diese Funktionen.

## Schließen-Button

- `CloseButtonPlacement.Left` / `.Right`: Darstellung und Hit-Test verwenden dieselbe Titelzeilenseite. Der Titel reserviert entsprechend Platz.
- `ShowCloseButton = false`: Button vollständig ausblenden.
- `IsCloseButtonEnabled = false`: Button sichtbar lassen, aber deaktiviert zeichnen und Pointer-Eingaben nicht auslösen. In diesem Bereich startet auch kein Fensterziehen.
- Das vorhandene `CanClose = false` blendet den Button weiterhin aus. Programmatisches `Close()` und die vorhandene Escape-Behandlung von Dialogen bleiben unabhängig von diesen Button-Einstellungen.
- `CloseButtonBounds` liefert den aktuellen Trefferbereich in Workspace-Koordinaten, bei ausgeblendetem Button ein leeres Rechteck.

Alle neuen Einstellungen unterstützen Styles, lokale Overrides und `ResetStyleProperty`. Bestehende Skins besitzen bereits einen Disabled-Zustand für den Schließen-Button; zusätzliche Atlasbilder sind nicht erforderlich. Titelmetriken werden beim Layout ermittelt, nicht bei jedem Draw.

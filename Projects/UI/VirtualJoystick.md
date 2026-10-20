# VirtualJoystick

Ein analoger Knüppel für Maus und unabhängige Touch-Kontakte. Neue Architektur und Namespaces: [Architektur.md](Architektur.md).

Direkte Properties: MovementMode (Free, Horizontal, Vertical, CustomAxis), PlacementMode (Fixed oder Floating), Radius, ThumbSize, DeadZone, ReturnToCenter, InvertX, InvertY und ResponseCurve. Die Kurve ist ein Sasogine CurveBase; eine EditableCurve kann direkt verwendet werden.

Value liefert den aufbereiteten Vektor, RawValue die rohe Auslenkung. IsActive und ActivePointer beschreiben den aktuellen Kontakt. Started, ValueChanged, Completed und Cancelled bleiben verfügbar; bei Deaktivierung oder verlorenem Capture wird abgebrochen. Eingaben kommen ausschließlich aus dem Workspace-Frame. Zwei Knüppel können unabhängige Touch-IDs halten.

Das XML-/Texture-Atlas-Skin-System unterstützt Base, Thumb, TrackHorizontal und TrackVertical, mit passenden Zuständen. Fehlende Teile verwenden geometrische Ersatzdarstellung. Ein eigener Style wird über StyleId ausgewählt.

Das ausführbare Beispiel Beginner.10.Joysticks zeigt sechs Varianten. Initialize erzeugt Workspace; Update erfasst Geräte einmal und ruft Workspace.Update auf, Draw anschließend Workspace.Draw.

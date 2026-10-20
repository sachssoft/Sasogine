# Styles und optionale Factories

Siehe [Architektur.md](Architektur.md) für StyleProperty, MVVM und Workspace-Lebenszyklus.

Styles enthalten zentrale String-Schlüssel und String-Werte, werden aber ausdrücklich von den jeweiligen Controls verarbeitet. `StyleId` wählt einen zentralen Style. Lokale Propertyzuweisungen bleiben erhalten; `ResetStyleProperty` übernimmt wieder den aktuellen Stylewert. Die Vererbung basiert auf `BasedOn`; unbekannte Basen und Zyklen werden zurückgewiesen.

Die Workspace besitzt ihre eigene Stylesammlung und automatische Standard-Factories. Der Kunde muss keine Registry anlegen. Normales `new Button()` oder ein eigenes `new MyControl()` braucht keine Factory. Nur für das Erstellen über einen Stringnamen wird optional `root.Registry.Register("MyControl", static () => new MyControl())` verwendet.

Converter bleiben ausdrücklich registrierte, generisch typisierte Delegates. Es gibt keine Reflection und keine Property-Metadatenbank. Der Skin verwendet weiterhin den vorhandenen PNG-Atlas und XML-Dateien.

# Architecture Structure

## Scene

Eine Scene besteht aus verschiedenen Diensten (Services) und Komponenten.

```text
Scene
├── Services
│   ├── PackageService
│   └── ...
│
└── Components
    ├── BackgroundComponent
    ├── TilePhysicsComponent
    ├── TileRenderComponent
    └── UserInterfaceComponent (UI)
```
Services

Services stellen zentrale Funktionen bereit, die von mehreren Components verwendet werden können.

Beispiel:

PackageService
Verwaltet Pakete und Ressourcen.
Components

Components kapseln einzelne Funktionen einer Scene.

Beispiele:

BackgroundComponent
Verwaltet Hintergründe und visuelle Ebenen.
TilePhysicsComponent
Zuständig für die physikalische Verarbeitung von Kacheln.
TileRenderComponent
Zeichnet und rendert Kacheln.
UserInterfaceComponent (UI)
Verwaltet die Bedienoberfläche und Benutzerinteraktionen.
Advantages

Durch eine geregelte Architektur bleibt die Programmierung sauberer, übersichtlicher und besser wartbar.

Diese Struktur ist nicht zwingend erforderlich, wird aber für die Zukunft empfohlen, besonders wenn das Spiel weiter wächst und komplexer wird.

Eine klare Trennung von Services und Components erleichtert Erweiterungen, Fehlerbehebung und die Wiederverwendung von Code.
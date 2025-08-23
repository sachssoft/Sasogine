# Sachssoft.Sasogine Package Container

Dieses Paket stellt die abstrakte Basisklasse `PackageBase` für die Verwaltung von Paketdateien 
im ZIP-Format bereit. Es dient als Grundlage für die Implementierung von Paketen mit Manifest, Lizenz, Icon und Vorschau-Bildern.

## Features

- Öffnen und Schließen von Paketdateien (ZIP-Archiv)
- Lesen und Schreiben im Paket (sofern nicht schreibgeschützt)
- Verwaltung von Manifest, Lizenz, Icon und Previews
- Sicheres Ressourcenmanagement mit `IDisposable`
- Unterstützung für verschiedene Manifest-Formate (Standard: JSON)
- Fehlerbehandlung bei ungültigen oder beschädigten Paketen

## Installation

Fügen Sie das Projekt Ihrer .NET 8-Lösung hinzu und referenzieren Sie den Namespace:

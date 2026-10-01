# Sachssoft Sasogine

Sasogine ist ein leichtgewichtiges, erweiterbares und AOT-freundliches Game-Engine-Framework für .NET auf Basis von [MonoGame](https://monogame.net/).

Es stellt wiederverwendbare Engine-Infrastruktur für Assets, Ressourcen, Rendering, Eingabe, Audio, Komponenten, Werkzeuge, Serialisierung und Plattformintegration bereit. Plattformspezifische Dienste und Grafik-Backends bleiben dabei vom Engine-Kern getrennt.

> \[!WARNING\] Sasogine befindet sich derzeit in einer frühen Alpha-Phase. APIs, Projektstruktur und Paketgrenzen können sich bis zu einer stabilen Version noch ändern.
>
> Version `0.11.0-alpha` führt eine größere Architekturüberarbeitung ein. Der Engine-Kern ist nicht mehr direkt an DesktopGL gebunden; Plattformintegrationen und Rendering-Backends werden als getrennte Projekte verwaltet.

## Dokumentation

- **Englische Dokumentation:** Siehe [README.md](README.md)
- **Deutsche Dokumentation:** Siehe [README_DE.md](README_DE.md)
- **Programmier- und Architektur-Richtlinien:**
  - [English](PROGRAMMING_ARCHITECTURE_GUIDELINES_EN.md)
  - [Deutsch](PROGRAMMING_ARCHITECTURE_GUIDELINES_DE.md)
- **Package Build & Publish Guide:** Siehe [PackageBuildInstructions.md](PackageBuildInstructions.md)
- **Changelog:** Siehe [CHANGELOG.md](CHANGELOG.md)
- **Lizenz:** Siehe [LICENSE.md](LICENSE.md)
- **Fehler & Entwicklung:** Siehe [GitHub Issues](https://github.com/sachssoft/Sasogine/issues)

## Anforderungen

- .NET 8.0
- .NET 9.0
- .NET 10.0
- MonoGame 3.8.5.1
- Trimming-kompatibles Design
- Native-AOT-kompatibles Design

Plattformspezifische Projekte verwenden bei Bedarf die entsprechenden .NET-Target-Frameworks wie `net*-windows`, `net*-android` und `net*-ios`.

## Pakete

> \[!IMPORTANT\] Sasogine befindet sich in aktiver Alpha-Entwicklung. Paketgrenzen, APIs und die Verfügbarkeit einzelner Backends können sich bis zu einer stabilen Version noch ändern.

### Engine und Module

| Modul         | Paket                          | Verwendung                                              | NuGet                                                                                                                                    |
|:--------------|:-------------------------------|:--------------------------------------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------|
| **Basic**     | `Sachssoft.Sasogine`           | Backend-unabhängiger Engine-Kern                        | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine)                     |
| **Documents** | `Sachssoft.Sasogine.Documents` | Dokumente, Ressourcen und Serialisierungsintegration    | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Documents.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Documents) |
| **Toolkit**   | `Sachssoft.Sasogine.Toolkit`   | Wiederverwendbare Editor- und Entwicklungswerkzeuge     | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Toolkit.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Toolkit)     |
| **Controls**  | `Sachssoft.Sasogine.Controls`  | UI-Steuerelemente, Layout und Darstellungsinfrastruktur | **Geplant**                                                                                                                              |

### Rendering-Backends

| Backend         | Paket                            | Grafik-API                           | NuGet                                                                                                                                        |
|:----------------|:---------------------------------|:-------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------|
| **DesktopGL**   | `Sachssoft.Sasogine.DesktopGL`   | OpenGL über MonoGame DesktopGL       | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL)     |
| **DesktopVK**   | `Sachssoft.Sasogine.DesktopVK`   | Vulkan über MonoGame Native          | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopVK.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopVK)     |
| **WindowsDX**   | `Sachssoft.Sasogine.WindowsDX`   | Klassisches MonoGame-DirectX-Backend | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX)     |
| **WindowsDX12** | `Sachssoft.Sasogine.WindowsDX12` | DirectX 12 über MonoGame Native      | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX12.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX12) |

### Plattformen

| Plattform   | Paket                                 | Verwendung                           | NuGet                                                                                                                                                  |
|:------------|:--------------------------------------|:-------------------------------------|:-------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Windows** | `Sachssoft.Sasogine.Platform.Windows` | Windows-spezifische Plattformdienste | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Windows.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Windows) |
| **Linux**   | `Sachssoft.Sasogine.Platform.Linux`   | Linux-spezifische Plattformdienste   | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Linux.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Linux)     |
| **macOS**   | `Sachssoft.Sasogine.Platform.MacOs`   | macOS-spezifische Plattformdienste   | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.MacOs.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.MacOs)     |
| **Android** | `Sachssoft.Sasogine.Platform.Android` | Android-Plattformintegration         | **Geplant**                                                                                                                                            |
| **iOS**     | `Sachssoft.Sasogine.Platform.iOS`     | iOS-Plattformintegration             | **Geplant**                                                                                                                                            |

Veröffentlichte Sasogine-Pakete sind außerdem im [Sachssoft-NuGet-Profil](https://www.nuget.org/profiles/tcs-1986) zu finden.

## Rendering-Backends

Rendering-Backends sind von den Plattformdiensten getrennt.

### DesktopGL

`Sachssoft.Sasogine.DesktopGL` verwendet das MonoGame-DesktopGL-Backend.

Typische Desktop-Ziele:

- Windows
- Linux
- macOS

### DesktopVK

`Sachssoft.Sasogine.DesktopVK` verwendet MonoGames Native-Framework zusammen mit Vulkan-Runtimes.

Dieses Backend gilt in Sasogine derzeit als experimentell.

### WindowsDX

`Sachssoft.Sasogine.WindowsDX` verwendet das klassische MonoGame-WindowsDX-Backend.

Es ist für Windows und DirectX-basiertes Rendering vorgesehen.

### WindowsDX12

`Sachssoft.Sasogine.WindowsDX12` verwendet MonoGames Native-Framework zusammen mit der DirectX-12-Runtime.

Dieses Backend gilt in Sasogine derzeit als experimentell.

## Native AOT und Trimming

Native-AOT- und Trimming-Kompatibilität sind wichtige Designziele von Sasogine.

Die Engine bevorzugt:

- Explizite Objekterzeugung
- Explizite Registries und Factories
- Stark typisierte Metadaten
- Vorhersehbare Abhängigkeitsauflösung
- Möglichst geringe Abhängigkeit von Laufzeit-Reflection
- AOT-kompatible Serialisierungspfade
- Trimming-kompatible APIs

Reflection kann in ausgewählten APIs als optionaler Fallback verwendet werden, wenn die jeweilige Laufzeit dies unterstützt. Kernfunktionalität soll Reflection nicht voraussetzen, wenn eine explizite Alternative verfügbar ist.

Vor Reflection-abhängigen Funktionen können Laufzeit-Fähigkeitsprüfungen verwendet werden.

## Entwicklungsstruktur

Der zentrale Quellcode ist nach Verantwortungsbereichen organisiert:

``` text
Projects/
├── Basic/
│   └── Sachssoft.Sasogine.csproj
├── Documents/
│   └── Sachssoft.Sasogine.Documents.csproj
├── Toolkit/
│   └── Sachssoft.Sasogine.Toolkit.csproj
├── Controls/
│   └── Sachssoft.Sasogine.Controls.csproj
├── Platforms/
│   ├── Windows/
│   ├── Linux/
│   ├── MacOs/
│   ├── Android/
│   └── iOS/
└── Backends/
    ├── DesktopGL/
    ├── DesktopVK/
    ├── WindowsDX/
    └── WindowsDX12/
```

Gemeinsame Build- und Paketkonfigurationen werden in den `.props`-Dateien im Repository-Stamm gepflegt.

## Versionierung

Sasogine verwendet derzeit Vorabversionen im Alpha-Stadium.

Während der Alpha-Phase gilt:

- APIs können sich ohne Kompatibilitätsgarantie ändern.
- Typen können zwischen Namespaces oder Assemblies verschoben werden.
- Projekte und Pakete können neu organisiert werden.
- Experimentelle Backends können unvollständig sein.

Versionsspezifische Änderungen sind in [CHANGELOG.md](CHANGELOG.md) dokumentiert.

## Fehler und Entwicklung

Bekannte Fehler, geplante Funktionen und laufende Entwicklung werden über GitHub Issues verwaltet:

https://github.com/sachssoft/Sasogine/issues

Fehlerberichte und Funktionswünsche können dort ebenfalls eingereicht werden.

## Lizenz

Sasogine steht unter der MIT-Lizenz.

Weitere Informationen befinden sich in [LICENSE.md](LICENSE.md).

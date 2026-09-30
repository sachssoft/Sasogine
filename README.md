# Sachssoft Sasogine

Sasogine is a lightweight, extensible and AOT-friendly game engine framework for .NET built on top of [MonoGame](https://monogame.net/).

It provides reusable engine infrastructure for assets, resources, rendering, input, audio, components, tools, serialization and platform integration while keeping platform-specific services and graphics backends separated from the engine core.

> [!WARNING]
> Sasogine is currently in an early alpha stage. APIs, project structure and package boundaries may still change before a stable release.
>
> Version `0.11.0-alpha` introduces a major architectural refactoring. The core engine is no longer tied directly to DesktopGL; platform integrations and rendering backends are maintained as separate projects.

## Documentation

* **English Documentation:** See [README.md](README.md)
* **German Documentation:** See [README_DE.md](README_DE.md)
* **Programming & Architecture Guidelines:**
  * [English](PROGRAMMING_ARCHITECTURE_GUIDELINES_EN.md)
  * [Deutsch](PROGRAMMING_ARCHITECTURE_GUIDELINES_DE.md)
* **Package Build & Publish Guide:** See [PackageBuildInstructions.md](PackageBuildInstructions.md)
* **Changelog:** See [CHANGELOG.md](CHANGELOG.md)
* **License:** See [LICENSE.md](LICENSE.md)
* **Issues & Development:** See [GitHub Issues](https://github.com/sachssoft/Sasogine/issues)

## Requirements

* .NET 8.0
* .NET 9.0
* .NET 10.0
* MonoGame 3.8.5.1
* Trimming-compatible design
* Native AOT-compatible design

Platform-specific projects use their corresponding .NET target frameworks, such as `net*-windows`, `net*-android` and `net*-ios` where required.

## Packages

> [!IMPORTANT]
> Sasogine is under active alpha development. Package boundaries, APIs and backend availability may still change before a stable release.

### Engine and Modules

| Module | Package | Usage | NuGet |
| --- | --- | --- | --- |
| **Basic** | `Sachssoft.Sasogine` | Backend-independent engine core | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine) |
| **Documents** | `Sachssoft.Sasogine.Documents` | Documents, resources and serialization integration | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Documents.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Documents) |
| **Toolkit** | `Sachssoft.Sasogine.Toolkit` | Reusable editor and development tools | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Toolkit.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Toolkit) |
| **UI / Surface** | `Sachssoft.Sasogine.UI` | UI and surface infrastructure | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.UI.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.UI) |

### Rendering Backends

| Backend | Package | Graphics API | NuGet |
| --- | --- | --- | --- |
| **DesktopGL** | `Sachssoft.Sasogine.DesktopGL` | OpenGL through MonoGame DesktopGL | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL) |
| **DesktopVK** | `Sachssoft.Sasogine.DesktopVK` | Vulkan through MonoGame Native | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopVK.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopVK) |
| **WindowsDX** | `Sachssoft.Sasogine.WindowsDX` | Classic MonoGame DirectX backend | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX) |
| **WindowsDX12** | `Sachssoft.Sasogine.WindowsDX12` | DirectX 12 through MonoGame Native | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX12.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX12) |

### Platforms

| Platform | Package | Usage | NuGet |
| --- | --- | --- | --- |
| **Windows** | `Sachssoft.Sasogine.Platform.Windows` | Windows-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Windows.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Windows) |
| **Linux** | `Sachssoft.Sasogine.Platform.Linux` | Linux-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Linux.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Linux) |
| **macOS** | `Sachssoft.Sasogine.Platform.MacOs` | macOS-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.MacOs.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.MacOs) |
| **Android** | `Sachssoft.Sasogine.Platform.Android` | Android platform integration | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Android.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Android) |
| **iOS** | `Sachssoft.Sasogine.Platform.iOS` | iOS platform integration | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.iOS.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.iOS) |

Published Sasogine packages can also be found on the [Sachssoft NuGet profile](https://www.nuget.org/profiles/tcs-1986).

## Rendering Backends

Rendering backends are separate from platform services.

### DesktopGL

`Sachssoft.Sasogine.DesktopGL` uses the MonoGame DesktopGL backend.

Typical desktop targets:

* Windows
* Linux
* macOS

### DesktopVK

`Sachssoft.Sasogine.DesktopVK` uses MonoGame's Native framework together with Vulkan runtimes.

It is currently considered experimental in Sasogine.

### WindowsDX

`Sachssoft.Sasogine.WindowsDX` uses the classic MonoGame WindowsDX backend.

It targets Windows and DirectX-based rendering.

### WindowsDX12

`Sachssoft.Sasogine.WindowsDX12` uses MonoGame's Native framework with the DirectX 12 runtime.

It is currently considered experimental in Sasogine.

## Native AOT and Trimming

Native AOT and trimming compatibility are important design goals of Sasogine.

The engine favors:

* Explicit object creation
* Explicit registries and factories
* Strongly typed metadata
* Predictable dependency resolution
* Minimal reliance on runtime reflection
* AOT-compatible serialization paths
* Trimming-compatible APIs

Reflection may be used as an optional fallback in selected APIs when the runtime supports it. Core functionality should not require reflection where an explicit alternative is available.

Runtime capability checks can be used before invoking reflection-dependent functionality.

## Development Structure

The main source tree is organized by responsibility:

```text
Projects/
├── Basic/
│   └── Sachssoft.Sasogine.csproj
├── Documents/
│   └── Sachssoft.Sasogine.Documents.csproj
├── Toolkit/
│   └── Sachssoft.Sasogine.Toolkit.csproj
├── UI/
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

Shared build and package configuration is maintained in the root-level `.props` files.

## Versioning

Sasogine currently uses prerelease alpha versions.

During the alpha phase:

* APIs may change without compatibility guarantees.
* Types may move between namespaces or assemblies.
* Projects and packages may be reorganized.
* Experimental backends may be incomplete.

See [CHANGELOG.md](CHANGELOG.md) for release-specific changes.

## Issues and Development

Known bugs, planned features and ongoing development are tracked through GitHub Issues:

https://github.com/sachssoft/Sasogine/issues

Bug reports and feature requests can be submitted through the issue tracker.

## License

Sasogine is licensed under the MIT License.

See [LICENSE.md](LICENSE.md) for details.

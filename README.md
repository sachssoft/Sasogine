# Sachssoft Sasogine

Sasogine is a lightweight, extensible, and AOT-friendly game engine framework for .NET based on [MonoGame](https://monogame.net/).

It provides reusable engine infrastructure for assets, resources, rendering, input, audio, components, tools, serialization, and platform integration. Platform-specific services and graphics backends remain separate from the engine core.

> [!WARNING]
> Sasogine is currently in an early alpha stage. APIs, project structure, and package boundaries may change before a stable release.

> Version `0.11.0-alpha` introduces a major architectural overhaul. The engine core is no longer directly tied to DesktopGL. Platform integrations and rendering backends are maintained as separate projects.

## Features

- **Engine Fundamentals:** Application lifecycle, services, strongly typed definitions, factories, and object references.
- **Scenes and Game Objects:** Scene management, viewports, entities, components, and tilemap foundations.
- **Assets and Resources:** Managed loading and reloading of textures, fonts, audio, and frame sets from files, memory, embedded resources, or remote sources.
- **Sprites and Tiles:** Texture atlases, XML/JSON frame import, and frame access by names, indices, or enums.
- **Rendering:** Batched rendering of sprites, tiles, text, and shapes with brushes, pens, gradients, textures, meshes, and models.
- **Controls and User Interfaces:** Extensive UI system featuring numerous controls, layouts, and data binding based on a hybrid retained-mode architecture. The system is under active development and may still contain bugs or limitations. Two visual editors are planned: Sachssoft Controls Designer for skin editing and Sachssoft View Layouter for visual interface layout design.
- **Cameras and Animation:** 2D cameras, isometric and related projections, parallax layers, and position and rotation animations.
- **Geometry and Colors:** Paths, curves, and polygon operations including clipping, offsets, and triangulation, as well as color conversion between RGB, HSL, HSV, linear RGB, and OKLab.
- **Input and Audio:** Keyboard, mouse, touch, and gamepad input, shortcuts, vibration, WAV playback, and MP3/OGG streaming.
- **Localization:** Language switching, fallback languages, localized values, and plural forms.
- **Documents:** Serialization registries and reader/writer extensions for engine types, along with JSON document formatting.
- **Editor Tools:** Object insertion, camera navigation, selection with handles for moving, scaling, and rotating, and vector path editing.
- **Development Utilities:** Scheduled actions, asynchronous loading and progress reporting, easing curves, random utilities, frame statistics, and benchmarks.

## Documentation

- **English Documentation:** See [README.md](README.md)
- **German Documentation:** See [README_DE.md](README_DE.md)
- **Programming and Architecture Guidelines:**
  - [English](PROGRAMMING_ARCHITECTURE_GUIDELINES_EN.md)
  - [German](PROGRAMMING_ARCHITECTURE_GUIDELINES_DE.md)
- **Package Build & Publish Guide:** See [PackageBuildInstructions.md](PackageBuildInstructions.md)
- **Changelog:** See [CHANGELOG.md](CHANGELOG.md)
- **License:** See [LICENSE.md](LICENSE.md)
- **Issues & Development:** See [GitHub Issues](https://github.com/sachssoft/Sasogine/issues)

## Requirements

- .NET 8.0
- .NET 9.0
- .NET 10.0
- MonoGame 3.8.5.1
- Trimming-compatible design
- Native AOT-compatible design

Platform-specific projects use the corresponding .NET target frameworks where required, such as `net*-windows`, `net*-android`, and `net*-ios`.

## Packages

> [!IMPORTANT]
> Sasogine is under active alpha development. Package boundaries, APIs, and the availability of individual backends may change before a stable release.

### Engine and Modules

| Module | Package | Purpose | NuGet |
|:---|:---|:---|:---|
| **Basic** | `Sachssoft.Sasogine` | Backend-independent engine core | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine) |
| **Documents** | `Sachssoft.Sasogine.Documents` | Documents, resources, and serialization integration | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Documents.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Documents) |
| **Toolkit** | `Sachssoft.Sasogine.Toolkit` | Reusable editor and development tools | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Toolkit.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Toolkit) |
| **Controls** | `Sachssoft.Sasogine.Controls` | UI controls, layouts, and presentation infrastructure | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Controls.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Controls) |

### Rendering Backends

| Backend | Package | Graphics API | NuGet |
|:---|:---|:---|:---|
| **DesktopGL** | `Sachssoft.Sasogine.DesktopGL` | OpenGL via MonoGame DesktopGL | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL) |
| **DesktopVK** | `Sachssoft.Sasogine.DesktopVK` | Vulkan via MonoGame Native | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopVK.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopVK) |
| **WindowsDX** | `Sachssoft.Sasogine.WindowsDX` | Classic MonoGame DirectX backend | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX) |
| **WindowsDX12** | `Sachssoft.Sasogine.WindowsDX12` | DirectX 12 via MonoGame Native | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.WindowsDX12.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.WindowsDX12) |

### Platforms

| Platform | Package | Purpose | NuGet |
|:---|:---|:---|:---|
| **Windows** | `Sachssoft.Sasogine.Platform.Windows` | Windows-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Windows.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Windows) |
| **Linux** | `Sachssoft.Sasogine.Platform.Linux` | Linux-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.Linux.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.Linux) |
| **macOS** | `Sachssoft.Sasogine.Platform.MacOs` | macOS-specific platform services | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Platform.MacOs.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Platform.MacOs) |
| **Android** | `Sachssoft.Sasogine.Platform.Android` | Android platform integration | **Planned** |
| **iOS** | `Sachssoft.Sasogine.Platform.iOS` | iOS platform integration | **Planned** |

Published Sasogine packages can also be found in the [Sachssoft NuGet profile](https://www.nuget.org/profiles/tcs-1986).

## Rendering Backends

Rendering backends are separated from platform services.

### DesktopGL

`Sachssoft.Sasogine.DesktopGL` uses the MonoGame DesktopGL backend.

Typical desktop targets:

- Windows
- Linux
- macOS

### DesktopVK

`Sachssoft.Sasogine.DesktopVK` uses MonoGame Native together with Vulkan runtimes.

This backend is currently considered experimental in Sasogine.

### WindowsDX

`Sachssoft.Sasogine.WindowsDX` uses the classic MonoGame WindowsDX backend.

It is intended for Windows and DirectX-based rendering.

### WindowsDX12

`Sachssoft.Sasogine.WindowsDX12` uses MonoGame Native together with the DirectX 12 runtime.

This backend is currently considered experimental in Sasogine.

## Native AOT and Trimming

Native AOT and trimming compatibility are important design goals of Sasogine.

The engine favors:

- Explicit object creation
- Explicit registries and factories
- Strongly typed metadata
- Predictable dependency resolution
- Minimal reliance on runtime reflection
- AOT-compatible serialization paths
- Trimming-compatible APIs

Reflection may be used as an optional fallback in selected APIs when supported by the runtime. Core functionality should not require reflection when an explicit alternative is available.

Runtime capability checks may be used before invoking reflection-dependent functionality.

## Development Structure

The main source code is organized by responsibility:

```text
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

Shared build and package configurations are maintained in the `.props` files at the repository root.

## Versioning

Sasogine currently uses alpha prerelease versions.

During the alpha phase:

- APIs may change without compatibility guarantees.
- Types may move between namespaces or assemblies.
- Projects and packages may be reorganized.
- Experimental backends may be incomplete.

Version-specific changes are documented in [CHANGELOG.md](CHANGELOG.md).

## Issues and Development

Known issues, planned features, and ongoing development are tracked through GitHub Issues:

https://github.com/sachssoft/Sasogine/issues

Bug reports and feature requests can also be submitted there.

## License

Sasogine is licensed under the MIT License.

For more information, see [LICENSE.md](LICENSE.md).
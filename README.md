# Sachssoft Sasogine

Sasogine is a lightweight and extensible game engine built on top of **[MonoGame](https://www.monogame.net/)**.

> [!WARNING]
> Sasogine is in early alpha stage. Not all features are complete yet.
> APIs and features may change, be replaced, or removed in future releases.

## Documentation

- **License**: See [LICENSE.md](LICENSE.md)
- **Changelog**: See [CHANGELOG.md](CHANGELOG.md)

## Requirements

- MonoGame 3.8.4.1
- .NET 8.0

## Downloads

| Platform | Graphics API | Status | NuGet |
|----------|---------------|--------|-------|
| DesktopGL | OpenGL 3.0+ | Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL) |
| WindowsDX | DirectX 11+ | Coming soon | - |
| Vulkan | Vulkan 1.1+ | Planned | Not implemented yet |

> **Future Note:** Sasogine may become a standalone engine in the future, with MonoGame maintained as a separate backend.

## Features

### Asset System
- Audio Assets
- Graphics Assets
- Model Assets
- Data Assets

### Scene System
- Scene Management
- Components
- Component Services

### Object and Definition Model
- Definition and Runtime Objects
- Definition-Based Creation

### Gameplay System
- Gameplay Formats
- Participants
- Randomization
- Tier Systems
- Rules

### Geometry System
- Geometric Primitives
- Mathematical Utilities
- Spatial Operations

### Graphics System
Supported graphics features:

- Texture Atlases
- Primitive Rendering
- Text and Font System
- Rendering Pipeline

### Input and Interaction System
- Input Handling
- User Interaction
- Device Support

### Resource System
- Resource Loading
- Multiple Resource Sources
- Custom Resource Providers

### Application Services
Platform-independent service interfaces:

- Platform Services
- Monetization Services
- External Integrations

### World and Entity System
- World Management
- Entity Management
- Object-Oriented Entity Architecture
- Entity Interaction
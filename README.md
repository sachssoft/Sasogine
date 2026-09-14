# Sachssoft Sasogine

Sasogine is a lightweight and extensible game engine built on top of **[MonoGame](https://www.monogame.net/)**.

> [!WARNING]
> Sasogine is in early alpha stage. Not all features are complete yet.
> APIs and features may change, be replaced, or removed in future releases.

## Documentation

* **Programming & Architecture Guidelines**

  * [English](PROGRAMMING_ARCHITECTURE_GUIDELINES_EN.md)
  * [Deutsch](PROGRAMMING_ARCHITECTURE_GUIDELINES_DE.md)
* **License**: See [LICENSE.md](LICENSE.md)
* **Changelog**: See [CHANGELOG.md](CHANGELOG.md)

## Requirements

* MonoGame 3.8.4.1
* .NET 8.0
* DesktopGL → OpenGL 3.0+
* WindowsDX → DirectX 11+
* Vulkan → Vulkan 1.1+

### Supported Platforms

| Category | Platforms                           |
| -------- | ----------------------------------- |
| Desktop  | Windows, macOS, Linux-based systems |
| Mobile   | Android, iOS                        |

## Downloads

| Library     | Usage                      | Status              | DesktopGL                                                                                                                                                | WindowsDX | Vulkan |
| ----------- | -------------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ------ |
| **Basic**   | Core Engine                | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL)                 |           |        |
| **Toolkit** | Editor & Development Tools | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Toolkit.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Toolkit.DesktopGL) |           |        |
| **UI**      | User Interface             | Planned             |                                                                                                                                                          |           |        |
| **Markup**  | Markup & Serialization     | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Markup.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Markup.DesktopGL)   |           |        |

#### Extensions

| Library     | Usage               | Status     | DesktopGL                                                                                                                                                                      | WindowsDX | Vulkan | Changelog                                       |
| ----------- | ------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------- | ------ | ----------------------------------------------- |
| **Sasodoc** | Document Formatting | Deprecated | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL) |           |        | [See](Projects/Extensions/Sasodoc/CHANGELOG.md) |

> **Note:** The Sasogine Sasodoc extension is deprecated. Starting with **Sasogine 0.2.0-alpha**, its serialization functionality has been integrated into **Sasogine Markup** under the new `Sachssoft.Sasogine.Markup.Serialization` namespace. The standalone extension remains available for earlier Sasogine versions but will no longer be developed.

## Features

(*Coming soon*)

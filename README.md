# Sachssoft Sasogine

Sasogine is a lightweight and extensible game engine built on top of **[MonoGame](https://www.monogame.net/)**.

> [!WARNING]
> Sasogine is in an early alpha stage. Not all features are complete yet.
> APIs and features may change, be replaced, or removed in future releases.

## Documentation

* **Programming & Architecture Guidelines**

  * [English](PROGRAMMING_ARCHITECTURE_GUIDELINES_EN.md)
  * [Deutsch](PROGRAMMING_ARCHITECTURE_GUIDELINES_DE.md)
* **Package Build & Publish Guide**: See [PackageBuildInstructions.md](PackageBuildInstructions.md)
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

### AOT & Trimming

Sasogine is designed to be **AOT- and trimming-compatible**.

Core engine functionality can be used without runtime reflection. APIs that require metadata discovery provide explicit, AOT-friendly alternatives where applicable.

Some optional functionality can use runtime reflection. Reflection-based features are only available when supported by the current runtime and platform and may be unavailable or restricted in Native AOT, trimmed, or platform-constrained environments.

Use `RuntimeCapabilities.IsReflectionSupported` and `RuntimeCapabilities.IsPlatformReflectionSupported` to determine whether reflection-based functionality is available before using reflection-dependent APIs.

> **Note:** Reflection is an optional fallback and is not required for the core definition metadata system. For maximum AOT and trimming compatibility, explicit metadata APIs such as `IDefinitionMetadata` should be preferred.

## Downloads

| Library       | Usage                            | Status              | DesktopGL                                                                                                                                                | WindowsDX | Vulkan |
| ------------- | -------------------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ------ |
| **Basic**     | Core Engine                      | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.DesktopGL)                 |           |        |
| **Toolkit**   | Editor & Development Tools       | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Toolkit.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Toolkit.DesktopGL) |           |        |
| **UI**        | User Interface                   | Planned             |                                                                                                                                                          |           |        |
| **Documents** | Document & Serialization Support | Partially Available | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Documents.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Documents.DesktopGL) |           |        |
| **Markup**    | Markup & Serialization           | Deprecated          | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Markup.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Markup.DesktopGL)   |           |        |

> **Note:** **Sasogine Markup** is deprecated and is being replaced by **Sasogine Documents**. New development should target `Sachssoft.Sasogine.Documents` and its related namespaces.

#### Extensions

| Library     | Usage               | Status     | DesktopGL                                                                                                                                                                      | WindowsDX | Vulkan | Changelog                                       |
| ----------- | ------------------- | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------- | ------ | ----------------------------------------------- |
| **Sasodoc** | Document Formatting | Deprecated | [![NuGet](https://img.shields.io/nuget/v/Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.svg)](https://www.nuget.org/packages/Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL) |           |        | [See](Projects/Extensions/Sasodoc/CHANGELOG.md) |

> **Note:** The Sasogine Sasodoc extension is deprecated. Starting with **Sasogine 0.2.0-alpha**, its serialization functionality is being integrated into **Sasogine Documents** under the `Sachssoft.Sasogine.Documents.Serialization` namespace. The standalone extension remains available for earlier Sasogine versions but will no longer be developed.

## Features

(*Coming soon*)

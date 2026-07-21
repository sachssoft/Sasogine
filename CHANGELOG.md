# Sachssoft Sasogine Changelog

All notable changes to this project will be documented in this file.

> This is an early alpha release. Not all planned features are implemented yet. APIs and features may change in future releases.

<small>

## Changelog Legend

| Tag | Description | Note |
|-|-|-|
| Feature | New feature or API | Adds new functionality |
| Change | Functional change | Changes behavior and may include breaking changes in Alpha and Beta versions |
| Improve | Improvement | Improves or refactors existing functionality and architecture. May include breaking changes in Alpha and Beta versions |
| Bug | Bug fix | Fixes errors or unexpected behavior |

> **Notice:** Breaking changes may occur only in Alpha and Beta versions.

</small>

## [0.0.4-alpha] - (End of August 2026)
- Planned

## [0.0.3-alpha] - In Progress (Target: 2026-07-28)
- [Feature] Add scene runtime settings for game modes and options, such as enabling debug features.
- [Feature] Add `Vector3` conversion support to `Coordinate2`.
- [Feature] Add `IPlatformFileStore` service for platform-specific resource source handling.
- [Feature] Introduce a new lightweight material system.
- [Feature] Expand CameraExtensions with world-space calculations.
- [Change] Refactor the primitive architecture by separating and reorganizing primitive classes.
- [Change] Replace scene-level shader management with default materials. Shader handling is now moved to the material layer.
- [Change] Rename `IPlatformModifier` to `IPlatformKeyModifiers` and clarify platform-specific keyboard modifier handling.
- [Change] Refactor and clean up namespaces
- [Improve] Extend `Coordinate2` with additional `Vector2` conversion overloads.
- [Bug] Fixed missing `IShaderTransform` in `ShaderBase` required for automatic default camera assignment in primitives.

## [0.0.2-alpha] - 2026-07-18

- [Feature] Add `Viewport` and `RenderSize` properties to `ViewportCursorService`.
- [Feature] Add `Service` to `GameConfiguration`.
- [Feature] Add `ToBox` to `Bounds` and `PixelBounds`.
- [Feature] Add `ToBounds` to `Box` and `PixelBox`.
- [Feature] Add constructors with a uniform parameter to `Size` and `PixelSize`.
- [Change] Rename `EffectAdapter` to `Shader` for future graphics backend independence.
- [Change] Remove several unused and unstable classes.
- [Change] Refactor `PackageContextService` change event handling.
- [Improve] Improve Camera System Architecture and transformation handling.
- [Improve] Improve instance creation with a single `GameConfiguration` parameter for better access.
- [Improve] Improve Scene System architecture and add missing summaries.
- [Improve] Improve shader transformation handling by separating camera and object transforms.
- [Improve] Extended and improved the `Coordinate2` structure.
- [Bug] Fix scene manager creation order by creating it after settings initialization.
- [Bug] Fix undefined asset definitions during asset construction.
- [Bug] Fix incorrect world transformation handling.
- [Bug] Fix rendering inconsistencies caused by mixed camera and object transformations.

## [0.0.1-alpha] - 2026-07-14

- [Feature] Initial alpha release
- [Feature] Initial Sasogine engine framework release

 
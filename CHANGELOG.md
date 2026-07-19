# Sachssoft Sasogine Changelog

All notable changes to this project will be documented in this file.

> This is an early alpha release. Not all planned features are implemented yet. APIs and features may change in future releases.

<small>

## Changelog Legend

| Tag | Description | Note |
|-|-|-|
| Feature | New feature or API | Adds new functionality |
| Breaking | Incompatible change | Applies only to Alpha versions and partially Beta versions (e.g. namespace, API changes, removals) |
| Change | Functional change | Changes existing behavior or workflow |
| Improve | Improvement | Optimization or quality improvement |
| Bug | Bug fix | Fixes errors or unexpected behavior |

</small>

## [0.0.3-alpha] - 2026-07-xx
- [Feature] Add `RuntimeMode` support for scene execution modes.
- [Feature] Add `Vector3` conversion support to `Coordinate2`.
- [Feature] Add `IPlatformFileStore` service for platform-specific resource source handling.
- [Breaking] Rename `IPlatformModifier` to `IPlatformKeyModifiers` and clarify platform-specific keyboard modifier handling.
- [Change] Extend `Coordinate2` with additional `Vector2` conversion overloads.
- [Bug] Fixed missing `IShaderTransform` in `ShaderBase` required for automatic default camera assignment in primitives.

## [0.0.2-alpha] - 2026-07-18

- [Feature] Add `Viewport` and `RenderSize` properties to `ViewportCursorService`.
- [Feature] Add `Service` to `GameConfiguration`.
- [Feature] Add `ToBox` to `Bounds` and `PixelBounds`.
- [Feature] Add `ToBounds` to `Box` and `PixelBox`.
- [Feature] Add constructors with a uniform parameter to `Size` and `PixelSize`.
- [Breaking] Rename `EffectAdapter` to `Shader` for future graphics backend independence.
- [Breaking] Remove several unused and unstable classes.
- [Breaking] Improve Camera System Architecture and transformation handling.
- [Breaking] Improve instance creation with a single `GameConfiguration` parameter for better access.
- [Breaking] Improve Scene System architecture and add missing summaries.
- [Breaking] Improve shader transformation handling by separating camera and object transforms.
- [Breaking] Refactor `PackageContextService` change event handling.
- [Improve] Extended and improved the `Coordinate2` structure.
- [Bug] Fix scene manager creation order by creating it after settings initialization.
- [Bug] Fix undefined asset definitions during asset construction.
- [Bug] Fix incorrect world transformation handling.
- [Bug] Fix rendering inconsistencies caused by mixed camera and object transformations.

## [0.0.1-alpha] - 2026-07-14

- [Added] Initial alpha release
- [Added] Initial Sasogine engine framework release

 
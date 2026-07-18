# Sachssoft Sasogine Changelog

All notable changes to this project will be documented in this file.

> This is an early alpha release. Not all planned features are implemented yet. APIs and features may change in future releases.

## [0.0.2-alpha] - 2026-07-xx

- [Added] Add `Viewport` and `RenderSize` properties to `ViewportCursorService`.
- [Added] Add `GameServiceManager` to `GameConfiguration`.
- [Added] Add `ToBox` to `Bounds` and `PixelBounds`.
- [Added] Add `ToBounds` to `Box` and `PixelBox`.
- [Added] Add constructors with a uniform parameter to `Size` and `PixelSize`.
- [Change] Improve instance creation with a single `GameConfiguration` parameter for better access.
- [Change] Improve Camera System Architecture and transformation handling.
- [Change] Improve Scene System architecture and add missing summaries.
- [Change] Rename `EffectAdapter` to `Shader` for future graphics backend independence.
- [Change] Improve shader transformation handling by separating camera and object transforms.
- [Change] Remove several unused and unstable classes.
- [Change] Refactor `PackageContextService` change event handling.
- [Improve] Extended and improved the `Coordinate2` structure.
- [Bug] Fix scene manager creation order by creating it after settings initialization.
- [Bug] Fix undefined asset definitions during asset construction.
- [Bug] Fix incorrect world transformation handling.
- [Bug] Fix rendering inconsistencies caused by mixed camera and object transformations.

## [0.0.1-alpha] - 2026-07-14

- [Added] Initial alpha release
- [Added] Initial Sasogine engine framework release

 
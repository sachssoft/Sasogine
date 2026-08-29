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

## [0.0.5-alpha] - In progress
- [Feature] **Toolkit**: Added a Vector Path Tool with `VectorPathTool` for creating and editing vector paths.
- [Feature] **Toolkit**: Added Selection and Transformation Tools with `SelectionTool` for editing and transforming objects.
- [Feature] **Toolkit**: Added object insertion support with `ObjectInsertTool` for creating and placing objects in the editor.
- [Feature] Added missing enum-based `IndexedFrameSet`.
- [Change] Removed incorrect rendering properties.
- [Change] Removed legacy Texture Atlas System.
- [Change] Removed `abstract` from `Mesh<TVertex>` and changed constructors from `protected` to `public`.
- [Improve] Added constructors to `BasicShader`, including an overload that accepts a `GraphicsDevice`.
- [Bug] Fixed `DiffuseMaterial.Apply()` not applying the shader.
- [Bug] Fixed multiple actions registered for the same input combination being overwritten.

## [0.0.4.1-alpha] - 2026-08-28
- [Feature] Introduced gameplay capability interfaces.
- [Feature] Added gameplay capabilities for movement, rotation, and enable state.
- [Feature] Introduced Selection Tool content interfaces and added selection, locking, movement, rotation, scaling, and resizing support.

## [0.0.4-alpha] - 2026-08-25
- [Feature] Introduced a new Tool System based on `ToolComponentBase` for managing interactive editor tools and their lifecycle.
- [Feature] Introduced the geometry batch `ShapeBatch` for efficient rendering of shapes with fewer draw calls.
- [Feature] Added geometry samplers for Catmull-Rom, B-Spline, and Hermite curves.
- [Feature] Implemented polygon triangulation through a backend interface using `LibTessDotNet`.
- [Feature] Implemented polygon clipping, offsetting and simplification through backend interfaces using `Clipper2`.
- [Feature] Implemented polygon transformation through a backend interface.
- [Feature] Added a polygon mesh generator to `MeshGenerator`.
- [Change] Made `Path` immutable and optimized it for geometry caching.
- [Improve] Implemented change consumption support for `ValueBuffer<T>`
- [Improve] Extended the constructors of `Box`, `PixelBox` and `Size`.
- [Improve] Added 32-bit index support to `Mesh<TVertex>`.
- [Improve] Improved path processing and geometry caching.
- [Improve] Improved path tools for multi-polygon paths.

## [0.0.3.1-alpha] - 2026-08-21
- [Feature] Added `InteractionFlags` for exposing interaction states to tool components.

## [0.0.3-alpha] - 2026-07-28
- [Feature] Added scene runtime settings for game modes and options, such as enabling debug features.
- [Feature] Added `Vector3` conversion support to `Coordinate2`.
- [Feature] Added `IPlatformFileStore` service for platform-specific resource source handling.
- [Feature] Introduced a new lightweight material system.
- [Feature] Expanded CameraExtensions with world-space calculations.
- [Feature] Added specialized texture frame set support for tile-based rendering.
- [Feature] Introduced `IMatrixProvider` interface and added `QuadTransform` and `TileTransform` implementations for flexible rendering transformations.
- [Feature] Introduced an efficient batch rendering system with support for tile-based rendering.
- [Feature] Introduced the Tile World System.
- [Feature] Added GPU mesh rendering support with `IMesh` and `MeshRenderer`.
- [Feature] Introduced mesh generator utilities for creating reusable GPU meshes.
- [Change] Replaced scene-level shader management with default materials. Shader handling is now moved to the material layer.
- [Change] Renamed `IPlatformModifier` to `IPlatformKeyModifiers` and clarified platform-specific keyboard modifier handling.
- [Change] Refactored and cleaned up namespaces.
- [Change] Replaced the Primitive System with the new GPU-based Mesh rendering system.
- [Change] Texture Atlas System marked obsolete. Replaced by the Frame Set System in the next release.
- [Improve] Extended `Coordinate2` with additional `Vector2` conversion overloads.
- [Bug] Fixed missing `IShaderTransform` implementation in `ShaderBase`, required for automatic default camera assignment in primitives.

## [0.0.2-alpha] - 2026-07-18
- [Feature] Added `Viewport` and `RenderSize` properties to `ViewportCursorService`.
- [Feature] Added `Service` to `GameConfiguration`.
- [Feature] Added `ToBox` methods to `Bounds` and `PixelBounds`.
- [Feature] Added `ToBounds` methods to `Box` and `PixelBox`.
- [Feature] Added constructors with a uniform parameter to `Size` and `PixelSize`.
- [Change] Renamed `EffectAdapter` to `Shader` for future graphics backend independence.
- [Change] Removed several unused and unstable classes.
- [Change] Refactored `PackageContextService` change event handling.
- [Improve] Improved Camera System architecture and transformation handling.
- [Improve] Improved instance creation by using a single `GameConfiguration` parameter for better access.
- [Improve] Improved Scene System architecture and added missing summaries.
- [Improve] Improved shader transformation handling by separating camera and object transforms.
- [Improve] Extended and improved the `Coordinate2` structure.
- [Bug] Fixed scene manager creation order by creating it after settings initialization.
- [Bug] Fixed undefined asset definitions during asset construction.
- [Bug] Fixed incorrect world transformation handling.
- [Bug] Fixed rendering inconsistencies caused by mixed camera and object transformations.

## [0.0.1-alpha] - 2026-07-14
- [Feature] Initial alpha release
- [Feature] Initial Sasogine engine framework release

 
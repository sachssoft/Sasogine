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

## [0.2.0-alpha] - Planned
- [Feature] **Toolkit**: Added `SelectionTool` for selecting and transforming objects.

## [0.1.1-alpha] - In progress
- [Feature] Enhanced `EntityCollection` with generic type support.
- [Change] Corrected API types, for example by replacing `Vector2` with `Point2` for position values.

## [0.1.0-alpha] - In progress
- [Feature] **Toolkit**: Added `SelectionTool` for selecting and transforming objects.
- [Feature] **Toolkit**: Added `ObjectInsertTool` for creating and placing objects.
- [Feature] Added `ThreadSafeLazy<T>` for thread-safe lazy initialization.
- [Feature] Introduced `ComponentBase` with support for reusable component services.
- [Feature] Added `IComponentService` for reusable services managed by components.
- [Feature] Established a dedicated build project for source generators and compile-time analyzers.
- [Feature] Introduced `Point2`, `Point3`, `PixelPoint2`, and `PixelPoint3` as framework-independent point types, reducing dependency on MonoGame's `Point`.
- [Feature] Added `ICursorState` with camera-aware cursor handling for services and tools.
- [Feature] Introduced `ToolContext` and `ToolInteractions` for shared tool state and standardized interaction mappings.
- [Feature] Added composable transform definition interfaces, including `ITransformMovable2Definition` and `ITransformRotatable2Definition`.
- [Change] Updated `IInteraction.Update` with `SceneUpdateContext` for component service integration.
- [Change] Reworked the input interaction system with `SceneUpdateContext` updates and a new interaction listener.
- [Change] Renamed `AxisInput<TAxis>` to `Axis<TAxis>`.
- [Change] Moved scheduling utilities from Features to `Common.Schedule` and updated scheduling operations to use `GameTime` and `SceneUpdateContext`.
- [Change] Refactored engine object lifecycle, asset loading, and game contexts.
- [Change] Updated `Path` to use `Box2` for polygon and path bounds.
- [Change] Removed `BoundingBox2D` and replaced it with `Box2` and `Bounds2`.
- [Improve] Improved geometry utilities and shape path generation.
- [Improve] Improved input handling with boxing-free enum conversion and reduced LINQ usage in performance-critical paths.
- [Improve] Refactored input state wrappers, mouse/touch handling, shortcuts, and vibration support.
- [Improve] Refactored `ToolComponentBase` to use shared component services and unified tool interaction and cursor handling.
- [Improve] Enhanced `ValueBuffer<T>` with operators and value conversions.
- [Improve] Optimized `DirectLazy<T>` for lightweight lazy initialization.
- [Improve] Hardened `DisposeManager` with safer resource registration and reverse-order disposal.
- [Improve] Refined `CulturedValue<T>` with optimized immutable modifications, fallback support, and reliable culture-specific lookup.
- [Improve] Updated `EngineObjectBase` with safer idempotent freezing.
- [Improve] Optimized `EngineObjectManager` with indexed ID lookup and safer registration handling.
- [Improve] Improved geometry shape path generation and rounding.
- [Improve] Strengthened `IdentifierFactory` with reliable identifier validation and creation.
- [Improve] Refined common utilities with small fixes, optimizations, and corrections.
- [Improve] Extended `VectorExtension` with additional vector operations and corrected projection calculations.
- [Improve] Expanded `VectorMath` with optimized line and segment geometry utilities.
- [Improve] Unified component services under `IComponentService` and introduced `IUpdatableComponentService` for services participating in the update cycle.
- [Improve] Updated selection definition interfaces to inherit from their corresponding transform definition interfaces, such as `ISelectionMovable2Definition : ITransformMovable2Definition`.
- [Improve] Various minor improvements and optimizations across the codebase.
- [Improve] Added `ISize2Definition` and `ISize3Definition` to provide shared size properties and avoid ambiguous interface member inheritance.
- [Improve] Improved diagnostics and performance monitoring.
- [Improve] Improved gameplay utilities, participant states, tiered scoring, difficulty, movement, and rotation direction definitions.
- [Improve] Expanded vector and geometry path utilities.
- [Improve] Improved shape path generation and geometry calculations.
- [Bug] Fixed rectangle corner rounding calculations.
- [Bug] Corrected scene viewport offsets.
- [Bug] Fixed asset loading state handling.
- [Bug] Prevented unsafe normalization of near-zero vectors.

## [0.0.5.1-alpha] - 2026-08-31
- [Feature] Added support for the skew transform interface in selection tools.
- [Improve] Implemented the non-generic `IList` interface for all mutable collections.
- [Improve] Changed `EngineObjectCollection<T>` from `ICollection<T>` to `IList<T>`.
- [Improve] Marked the legacy asset loading methods in `AssetStore` as obsolete.
- [Bug] Fixed several minor bugs.

## [0.0.5-alpha] - 2026-08-30
- [Feature] **Markup**: Introduced markup support for Sasogine.
- [Feature] **Markup**: Added support for loading `IndexedFrameSet` and `KeyedFrameSet`.
- [Feature] Added missing enum-based `IndexedFrameSet`.
- [Feature] Added `Command` and generic `Command<T>` implementations with execution conditions and `CanExecuteChanged` notifications.
- [Change] Removed incorrect rendering properties.
- [Change] Removed legacy Texture Atlas System.
- [Change] Removed `abstract` from `Mesh<TVertex>` and changed constructors from `protected` to `public`.
- [Change] Refactored dimension-dependent types into explicit 2D and 3D variants.
- [Improve] Added constructors to `BasicShader`, including an overload that accepts a `GraphicsDevice`.
- [Improve] Extended `Size` and `PixelSize` with additional arithmetic operators and immutable methods.
- [Improve] Extended `ISelectionTarget` with new `ISelectionTarget2D` and `ISelectionTarget3D` interfaces.
- [Improve] Refactored selection capability interfaces (`ISelectionMovable`, `ISelectionResizable`, `ISelectionRotatable`) into separate 2D and 3D variants.
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

 
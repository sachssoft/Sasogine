# Polygon tracing

`PolygonOperations.Trace` converts a MonoGame `Texture2D` into backend-neutral vector contours.
The default backend is fully managed C# and has no native runtime dependency.

## Basic usage

```csharp
PolygonTraceResult result = PolygonOperations.Trace(texture);
Path path = result.ToPath();
```

For an exact pixel boundary:

```csharp
var options = new PolygonTraceOptions(
    mode: PolygonTraceMode.Pixel,
    channel: PolygonTraceChannel.Alpha,
    threshold: 0.5f);

PolygonTraceResult result = PolygonOperations.Trace(texture, options);
```

For simplified polygon geometry:

```csharp
var options = new PolygonTraceOptions(
    mode: PolygonTraceMode.Polygon,
    simplificationTolerance: 1f);
```

For cubic Bézier output:

```csharp
var options = new PolygonTraceOptions(
    mode: PolygonTraceMode.Spline,
    simplificationTolerance: 0.75f,
    curveTolerance: 1f,
    cornerThreshold: 135f);
```

## Pipeline

1. Read the `Texture2D` into managed `Color` data.
2. Build a binary mask from alpha, luminance, or a selected color channel.
3. Extract exact directed pixel-boundary contours.
4. Preserve outer contours and holes by winding direction.
5. Remove redundant collinear points.
6. Optionally simplify closed polygons.
7. Optionally fit cubic Bézier curves while preserving detected corners.
8. Return `PolygonTraceResult` without forcing curve data into sampled polygons.

`PolygonTraceResult.ToPath()` samples cubic segments only when polygon-only geometry is required.
This keeps tracing suitable for later conversion to editable vector segments.

## Cross-platform and AOT

The default `ManagedPolygonTracer` uses only managed C# and MonoGame APIs.
It does not use P/Invoke, native libraries, runtime code generation, reflection, or external processes.

Tracing calls `Texture2D.GetData<Color>()`, so it is intended for asset processing, editor operations,
or other non-per-frame workflows rather than repeated rendering-loop execution.

## Texture readback

The managed backend reads texture data through `Texture2D.GetData<Color>()`.
If a texture surface format cannot be read as `Color` data, tracing throws a `NotSupportedException`
with the source texture format instead of silently interpreting incompatible bytes.
For assets that must be traced on every target platform, keep the tracing source in an uncompressed
color format.

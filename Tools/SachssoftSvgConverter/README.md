# Sachssoft SVG Converter

Batch SVG-to-PNG converter based on the provided Sasogine Texture Converter Avalonia project.

## Features
- Add multiple SVG documents, remove selected files, or clear the list
- Select an output directory
- Choose one or more output scales: 1×, 2×, 3×
- Save transparent PNG assets with filenames such as `icon_1x.png`, `icon_2x.png`, `icon_3x.png`
- Skip pre-existing output files by default; optional overwrite
- Avoid collisions among input files with the same base name
- Display individual results and errors

## Build

Requirements: .NET 8 SDK. On Windows, Native AOT publishing also requires the MSVC C++ build tools.

```powershell
dotnet restore
dotnet run -c Release
```

## Publish
Run `publish.bat` (or `publish.ps1`) to publish a Windows x64 self-contained Native AOT build into `publish/win-x64`.

## Implementation notes
Uses `Svg.Skia 5.2.3` (based on SkiaSharp) for SVG loading/rasterization. The legacy `SkiaSharp.Extended.Svg` library is deprecated. SVG files should declare explicit dimensions or a usable viewBox. Output size is based on the SVG picture's bounding rectangle. Rendering external SVG resources may depend on parser support. Input SVG content is treated as untrusted; only process files you trust.

The app reuses the example project's icon as a placeholder; replace `Assets/icon.png` and `Assets/icon.ico` with product-specific artwork before releasing.

The source was assembled without running `dotnet build` because the SDK is not available in this environment; compile/test on Windows before distribution, especially the Native AOT target.

## Sizing options

Choose either scale factors (1×, 2×, 3×, 4×; multiple outputs) or a target size (one output per SVG). In target-size mode you can set width only, height only, or both. With both dimensions and **Keep aspect ratio** enabled, output fits inside the specified dimensions without distortion. Disable it to stretch to exactly the specified dimensions. Single-dimension modes always preserve the aspect ratio. Output filenames include `_1x` etc. or the actual raster dimensions (e.g. `_256x128`).

## Filename suffix

Enable **Add filename suffix** to append a custom suffix before `.png` (for example `_1x`). Without it, single-output conversions use the original name. When multiple scale factors are selected, an automatic `_1x`, `_2x` etc. identifier is appended to avoid name collisions; if custom suffix is enabled, that identifier follows the custom suffix.

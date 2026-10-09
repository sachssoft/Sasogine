# Sachssoft Texture Converter

Small standalone Avalonia desktop utility based on the supplied **Sasogine Shader Generator** UI pattern. The original shader generator is not modified.

## Build

Requires .NET 8 SDK. Restore dependencies from NuGet, then run:

```text
dotnet run --project SasogineTextureConverter.csproj
```

On Windows, a single-file native-AOT publish can be created with `publish.ps1` (requires the usual .NET native AOT toolchain).

## Workflow

1. Select an input image (`.png`, `.jpg`/`.jpeg`, `.bmp`, `.gif`, `.webp`, or `.tga`). TGA support includes uncompressed and RLE true-color images with 24 or 32 bits. GIFs use their first frame.
2. Choose one of the common MonoGame `SurfaceFormat` values: `Color`, `Bgra32`, `Bgr565`, `Bgra5551`, `Bgra4444`, `Alpha8`.
3. Choose output format: PNG, JPEG, WebP, BMP, TGA, or raw packed pixel data (`.raw` + `.json`).
4. Optionally check **Replace existing / original file**. Unchecked, the generated filename has a `_SurfaceFormat` suffix and existing files are protected. Checked, the generated filename uses the original basename and allows overwriting the destination. You may also type or browse a custom output path.
5. Click **Convert**.

## Important: file format vs. SurfaceFormat

`SurfaceFormat` is a GPU pixel layout. **PNG, JPEG, WebP, BMP and TGA do not carry the MonoGame GPU format as metadata.** For raster image output, the app quantizes colors to simulate the selected pixel format and then uses the regular image encoder. `Bgra32` and `Color` look identical as raster exports. JPEG/BMP flatten alpha onto white, and Bgr565 is fully opaque.

For **actual GPU-compatible packed data**, choose **MonoGame raw**. It writes pixel bytes to `.raw`, with matching `.json` dimensions, row order, and `surfaceFormat`. `Color` is 4 bytes per pixel (R, G, B, A); `Bgra32` is (B, G, R, A); `Bgr565`, `Bgra5551`, and `Bgra4444` are little-endian packed 16-bit; `Alpha8` is one byte. These packed representations follow MonoGame's `Graphics.PackedVector` packing conventions.

`*.raw` is a custom pixel-data file, **not** a MonoGame Content Pipeline / `.xnb` file. Load it yourself when creating a `Texture2D` with the matching `SurfaceFormat` and dimensions, then `SetData<byte>(rawBytes)`. GPU support for a specific `SurfaceFormat` is platform-dependent; in particular, `Bgra32` is not universally available. DXT/BC, ETC, ASTC and other compressed GPU formats are intentionally not exposed because the app does not include a real block-compression encoder. For conventional MonoGame content assets, keep using the Content Pipeline where appropriate.

A `.raw` export also writes a `.json` sidecar next to it; enabling replacement may overwrite both files. The app limits input images to 32 million pixels.

## Dependencies

- Avalonia UI 11.3.8 (as in the source template)
- SkiaSharp 2.88.9 (already an Avalonia dependency; explicit reference for image codecs)

The code does not depend on a MonoGame runtime or an OpenGL/DirectX graphics device.

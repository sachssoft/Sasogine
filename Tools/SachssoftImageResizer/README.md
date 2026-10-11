# Sachssoft Image Resizer

Avalonia 11 / .NET 8 batch image resizer using SkiaSharp.

- Select a source folder containing `.png`, `.jpg` and `.jpeg` files.
- Choose a target width or height; the other dimension is calculated proportionally.
- Optional alpha-edge trimming for PNG files before resizing (JPEG files are not alpha-trimmed).
- Output images retain their source file format and extension; JPEG quality is set to 95.
- Optional recursive processing retains the subfolder structure.
- An automatically named sibling output directory (`_resized`, `_resized_1`, etc.) avoids overwriting originals.
- An optional filename suffix (e.g. `_1x`) is appended to each image.

## Run

```sh
dotnet run --project SachssoftImageResizer.csproj
```

Windows AOT publishing: `publish.ps1`. Requires the .NET 8 SDK and AOT toolchain.

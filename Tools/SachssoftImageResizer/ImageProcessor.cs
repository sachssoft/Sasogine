using SkiaSharp;
using System.Text;

namespace Sachssoft.ImageResizer;

internal static class ImageProcessor
{
    public static string Process(string input, int target, bool byWidth, string suffix, bool trim, bool recursive)
    {
        input = Path.GetFullPath(input);
        var parent = Directory.GetParent(input)?.FullName ?? input;
        var baseName = Path.GetFileName(Path.TrimEndingDirectorySeparator(input)) + "_resized";
        var output = Path.Combine(parent, baseName);
        for (var i = 1; Directory.Exists(output); i++) output = Path.Combine(parent, $"{baseName}_{i}");

        var files = Directory.EnumerateFiles(input, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(x => new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToArray();
        if (files.Length == 0) return "No PNG or JPEG files found.";

        Directory.CreateDirectory(output);
        var log = new StringBuilder($"Output: {output}{Environment.NewLine}");
        var success = 0;
        foreach (var file in files)
        {
            try
            {
                using var original = SKBitmap.Decode(file) ?? throw new InvalidDataException("Cannot decode image.");
                var bounds = new SKRectI(0, 0, original.Width, original.Height);
                if (trim && Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase)) bounds = FindBounds(original);
                var width = byWidth ? target : Math.Max(1, (int)Math.Round((double)bounds.Width * target / bounds.Height));
                var height = byWidth ? Math.Max(1, (int)Math.Round((double)bounds.Height * target / bounds.Width)) : target;
                if (width > 32768 || height > 32768) throw new InvalidDataException("Resulting image exceeds 32768 pixels per axis.");
                using var resized = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
                using (var canvas = new SKCanvas(resized))
                using (var paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High })
                {
                    canvas.Clear(Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase) ? SKColors.Transparent : SKColors.White);
                    canvas.DrawBitmap(original, bounds, new SKRect(0, 0, width, height), paint);
                    canvas.Flush();
                }
                // Remove any residual transparent padding after resampling, too.
                SKBitmap finalBitmap = resized;
                SKBitmap? cropped = null;
                if (trim && Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    var finalBounds = FindBounds(resized);
                    if (finalBounds.Width < resized.Width || finalBounds.Height < resized.Height)
                    {
                        cropped = new SKBitmap(new SKImageInfo(finalBounds.Width, finalBounds.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
                        using var cropCanvas = new SKCanvas(cropped);
                        cropCanvas.Clear(SKColors.Transparent);
                        cropCanvas.DrawBitmap(resized, finalBounds, new SKRect(0, 0, finalBounds.Width, finalBounds.Height));
                        finalBitmap = cropped;
                    }
                }
                using var croppedScope = cropped;
                var relative = Path.GetRelativePath(input, file);
                var folder = Path.Combine(output, Path.GetDirectoryName(relative) ?? "");
                Directory.CreateDirectory(folder);
                var extension = Path.GetExtension(file).ToLowerInvariant();
                var name = Path.GetFileNameWithoutExtension(file) + suffix + extension;
                var destination = Path.Combine(folder, name);
                for (var n = 1; File.Exists(destination); n++) destination = Path.Combine(folder, Path.GetFileNameWithoutExtension(name) + $"_{n}{extension}");
                using var image = SKImage.FromBitmap(finalBitmap);
                var format = extension == ".png" ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
                using var data = image.Encode(format, format == SKEncodedImageFormat.Jpeg ? 95 : 100) ?? throw new IOException("Image encoding failed.");
                using var stream = File.Create(destination);
                data.SaveTo(stream);
                success++;
                log.AppendLine($"OK: {relative} → {finalBitmap.Width}×{finalBitmap.Height}");
            }
            catch (Exception ex) { log.AppendLine($"ERROR: {Path.GetFileName(file)}: {ex.Message}"); }
        }
        log.AppendLine($"Finished: {success}/{files.Length} images.");
        return log.ToString();
    }

    private static SKRectI FindBounds(SKBitmap bitmap)
    {
        var left = bitmap.Width; var top = bitmap.Height; var right = -1; var bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
                if (bitmap.GetPixel(x, y).Alpha > 8)
                {
                    left = Math.Min(left, x); top = Math.Min(top, y);
                    right = Math.Max(right, x); bottom = Math.Max(bottom, y);
                }
        return right < 0 ? new SKRectI(0, 0, bitmap.Width, bitmap.Height) : new SKRectI(left, top, right + 1, bottom + 1);
    }
}

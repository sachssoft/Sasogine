using System.Text;
using SkiaSharp;
using Svg.Skia;

namespace Sachssoft.SvgConverter;

/// <summary>Result of a batch conversion operation.</summary>
/// <param name="Created">Number of PNG images written.</param>
/// <param name="Skipped">Number of existing PNG files skipped.</param>
/// <param name="Errors">Number of failed conversions.</param>
/// <param name="Log">Conversion report.</param>
public sealed record ConversionReport(int Created, int Skipped, int Errors, string Log);

/// <summary>Defines how an SVG is resized for PNG output.</summary>
public enum ResizeMode
{
    /// <summary>Produces one PNG for each selected multiplication factor.</summary>
    Scale,
    /// <summary>Produces one PNG at a selected target width and/or height.</summary>
    TargetSize
}

/// <summary>Options for SVG rasterization.</summary>
/// <param name="Mode">Scale factors or a target size.</param>
/// <param name="Scales">Scale factors (used in scale mode).</param>
/// <param name="Width">Target width, or null to infer from target height.</param>
/// <param name="Height">Target height, or null to infer from target width.</param>
/// <param name="KeepAspectRatio">Fit within the specified width and height instead of stretching.</param>
/// <param name="AddSuffix">Whether to append a custom filename suffix.</param>
/// <param name="Suffix">Custom filename suffix when enabled.</param>
public sealed record ResizeOptions(ResizeMode Mode, IReadOnlyList<int> Scales, int? Width, int? Height, bool KeepAspectRatio, bool AddSuffix = false, string Suffix = "");

/// <summary>Rasterizes SVG documents into transparent PNG images with Skia.</summary>
public static class SvgConverter
{
    private const int MaxDimension = 16384;

    /// <summary>Converts input SVG files into PNG files according to the selected sizing options.</summary>
    /// <param name="files">Local SVG files.</param>
    /// <param name="outputFolder">Destination directory.</param>
    /// <param name="options">Sizing configuration.</param>
    /// <param name="overwrite">Whether to replace existing PNGs.</param>
    /// <returns>A report listing successful and failed conversions.</returns>
    public static ConversionReport ConvertBatch(IReadOnlyList<string> files, string outputFolder, ResizeOptions options, bool overwrite)
    {
        Directory.CreateDirectory(outputFolder);
        var log = new StringBuilder();
        var created = 0;
        var skipped = 0;
        var errors = 0;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                using var svg = new SKSvg();
                var picture = svg.Load(file) ?? throw new InvalidDataException("Could not parse SVG.");
                var bounds = picture.CullRect;
                if (!float.IsFinite(bounds.Width) || !float.IsFinite(bounds.Height) || bounds.Width <= 0 || bounds.Height <= 0)
                    throw new InvalidDataException("SVG has no valid viewport size.");

                var stem = Path.GetFileNameWithoutExtension(file);
                var unique = stem;
                for (var index = 2; !names.Add(unique); index++) unique = $"{stem}_{index}";

                IEnumerable<(double Width, double Height, string Suffix)> sizes = options.Mode == ResizeMode.Scale
                    ? options.Scales.Distinct().Select(scale => (Width: (double)bounds.Width * scale, Height: (double)bounds.Height * scale, Suffix: $"_{scale}x"))
                    : [GetTargetSize(bounds.Width, bounds.Height, options)];

                foreach (var size in sizes)
                {
                    try
                    {
                        if (!double.IsFinite(size.Width) || !double.IsFinite(size.Height) ||
                            size.Width <= 0 || size.Height <= 0 || size.Width > MaxDimension || size.Height > MaxDimension)
                            throw new InvalidDataException($"Output must be between 1 and {MaxDimension} pixels per dimension.");

                        var width = (int)Math.Ceiling(size.Width);
                        var height = (int)Math.Ceiling(size.Height);
                        var multiScale = options.Mode == ResizeMode.Scale && options.Scales.Distinct().Count() > 1;
                        var filenameSuffix = options.AddSuffix
                            ? options.Suffix + (multiScale ? size.Suffix : "")
                            : multiScale ? size.Suffix : "";
                        var path = Path.Combine(outputFolder, $"{unique}{filenameSuffix}.png");
                        if (!overwrite && File.Exists(path))
                        {
                            skipped++;
                            log.AppendLine($"SKIP {Path.GetFileName(path)} (already exists)");
                            continue;
                        }

                        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
                        using (var canvas = new SKCanvas(bitmap))
                        {
                            canvas.Clear(SKColors.Transparent);
                            canvas.Scale(width / bounds.Width, height / bounds.Height);
                            canvas.Translate(-bounds.Left, -bounds.Top);
                            canvas.DrawPicture(picture);
                            canvas.Flush();
                        }
                        using var image = SKImage.FromBitmap(bitmap);
                        using var data = image.Encode(SKEncodedImageFormat.Png, 100)
                            ?? throw new InvalidOperationException("PNG encoding failed.");
                        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                        data.SaveTo(stream);
                        created++;
                        log.AppendLine($"OK   {Path.GetFileName(path)} ({width} × {height})");
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        log.AppendLine($"ERROR {Path.GetFileName(file)} ({size.Suffix}): {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors++;
                log.AppendLine($"ERROR {Path.GetFileName(file)}: {ex.Message}");
            }
        }
        return new ConversionReport(created, skipped, errors, log.ToString());
    }

    private static (double Width, double Height, string Suffix) GetTargetSize(float sourceWidth, float sourceHeight, ResizeOptions options)
    {
        var width = options.Width;
        var height = options.Height;
        if (width is null && height is null) throw new ArgumentException("Specify a target width or height.");
        if (width <= 0 || height <= 0) throw new ArgumentException("Dimensions must be positive.");

        double resultWidth, resultHeight;
        if (width is null) { resultHeight = height!.Value; resultWidth = sourceWidth * resultHeight / sourceHeight; }
        else if (height is null) { resultWidth = width.Value; resultHeight = sourceHeight * resultWidth / sourceWidth; }
        else if (options.KeepAspectRatio)
        {
            var factor = Math.Min((double)width.Value / sourceWidth, (double)height.Value / sourceHeight);
            resultWidth = sourceWidth * factor;
            resultHeight = sourceHeight * factor;
        }
        else { resultWidth = width.Value; resultHeight = height.Value; }

        return (resultWidth, resultHeight, $"_{Math.Ceiling(resultWidth):0}x{Math.Ceiling(resultHeight):0}");
    }
}

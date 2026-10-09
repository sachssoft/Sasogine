using SkiaSharp;
using System.Text.Json;

namespace Sachssoft.TextureConverter;

internal enum MonoGamePixelFormat
{
    Color,
    Bgra32,
    Bgr565,
    Bgra5551,
    Bgra4444,
    Alpha8
}

internal static class ImageConverter
{
    /// <summary>
    /// Reads an image, applies MonoGame-compatible pixel quantization and saves the requested image or packed raw data.
    /// </summary>
    /// <param name="inputPath">Path of the image to decode.</param>
    /// <param name="outputPath">Destination file, including a supported extension.</param>
    /// <param name="format">MonoGame surface layout to emulate or pack.</param>
    /// <param name="replace">Whether existing output files may be overwritten.</param>
    /// <returns>A human-readable summary of the exported image and pixel format.</returns>
    public static string Convert(string inputPath, string outputPath, MonoGamePixelFormat format, bool replace)
    {
        inputPath = Path.GetFullPath(inputPath);
        outputPath = Path.GetFullPath(outputPath);

        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        var isRaw = extension == ".raw";
        var metadataPath = isRaw ? Path.ChangeExtension(outputPath, ".json") : null;

        if (!replace && (File.Exists(outputPath) || metadataPath is not null && File.Exists(metadataPath)))
            throw new IOException("Output already exists. Enable 'Replace existing / original file' to overwrite it.");

        var directory = Path.GetDirectoryName(outputPath) ?? throw new IOException("Invalid output directory.");
        Directory.CreateDirectory(directory);

        var source = ReadPixels(inputPath, out var width, out var height);
        var pixels = new SKColor[source.Length];
        for (var i = 0; i < source.Length; i++)
            pixels[i] = Quantize(source[i], format);

        var tempOutput = Path.Combine(directory, "." + Guid.NewGuid().ToString("N") + ".tmp");
        var tempMetadata = isRaw ? Path.Combine(directory, "." + Guid.NewGuid().ToString("N") + ".tmp") : null;

        try
        {
            if (isRaw)
            {
                SaveRaw(tempOutput, pixels, format);
                var metadata = new
                {
                    width,
                    height,
                    surfaceFormat = format.ToString(),
                    bytesPerPixel = format == MonoGamePixelFormat.Alpha8 ? 1 :
                        format is MonoGamePixelFormat.Color or MonoGamePixelFormat.Bgra32 ? 4 : 2,
                    rowOrder = "top-to-bottom",
                    byteOrder = "little-endian (packed 16-bit formats)",
                    dataFile = Path.GetFileName(outputPath)
                };
                File.WriteAllText(tempMetadata!, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                switch (extension)
                {
                    case ".png": SaveSkia(tempOutput, pixels, width, height, SKEncodedImageFormat.Png); break;
                    case ".jpg": SaveSkia(tempOutput, ToOpaque(pixels), width, height, SKEncodedImageFormat.Jpeg); break;
                    case ".webp": SaveSkia(tempOutput, pixels, width, height, SKEncodedImageFormat.Webp); break;
                    case ".bmp": SaveBmp(tempOutput, pixels, width, height); break;
                    case ".tga": SaveTga(tempOutput, pixels, width, height); break;
                    default: throw new NotSupportedException($"Unsupported output format: {extension}");
                }
            }

            // The image is fully read before touching an existing destination.
            File.Move(tempOutput, outputPath, replace);
            if (tempMetadata is not null)
                File.Move(tempMetadata, metadataPath!, replace);
        }
        finally
        {
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
            if (tempMetadata is not null && File.Exists(tempMetadata)) File.Delete(tempMetadata);
        }

        var message = $"Success: {outputPath}{Environment.NewLine}" +
                      $"Dimensions: {width} × {height}{Environment.NewLine}" +
                      $"MonoGame SurfaceFormat: {format}{Environment.NewLine}";

        if (isRaw)
            return message + $"Packed texture data: {new FileInfo(outputPath).Length:N0} bytes{Environment.NewLine}" +
                   $"Metadata: {metadataPath}{Environment.NewLine}" +
                   "Import with Texture2D(width, height, mipMap: false, format) and SetData<byte>(rawBytes).";

        return message + "Note: PNG / JPEG / WebP / BMP / TGA do not preserve the selected MonoGame " +
               "SurfaceFormat as GPU metadata. Use .raw for the real packed pixel layout.";
    }

    private static SKColor[] ReadPixels(string path, out int width, out int height)
    {
        if (Path.GetExtension(path).Equals(".tga", StringComparison.OrdinalIgnoreCase))
            return ReadTga(path, out width, out height);

        using var bitmap = SKBitmap.Decode(path) ?? throw new InvalidDataException("The image cannot be decoded.");
        width = bitmap.Width;
        height = bitmap.Height;
        CheckDimensions(width, height);
        return bitmap.Pixels;
    }

    private static SKColor[] ReadTga(string path, out int width, out int height)
    {
        using var reader = new BinaryReader(File.OpenRead(path));
        var idLength = reader.ReadByte();
        var colorMapType = reader.ReadByte();
        var imageType = reader.ReadByte();
        reader.BaseStream.Seek(9, SeekOrigin.Current); // Color map and origin fields
        width = reader.ReadUInt16();
        height = reader.ReadUInt16();
        var depth = reader.ReadByte();
        var descriptor = reader.ReadByte();

        if (colorMapType != 0 || imageType is not (2 or 10) || depth is not (24 or 32))
            throw new NotSupportedException("Only uncompressed or RLE true-color TGA (24/32-bit) is supported.");

        CheckDimensions(width, height);
        reader.BaseStream.Seek(idLength, SeekOrigin.Current);
        var pixels = new SKColor[checked(width * height)];
        var topOrigin = (descriptor & 0x20) != 0;
        var rightOrigin = (descriptor & 0x10) != 0;

        var imageWidth = width;
        var imageHeight = height;

        void SetPixel(int index, SKColor color)
        {
            var x = index % imageWidth;
            var y = index / imageWidth;
            if (!topOrigin) y = imageHeight - 1 - y;
            if (rightOrigin) x = imageWidth - 1 - x;
            pixels[y * imageWidth + x] = color;
        }

        SKColor ReadPixel()
        {
            var b = reader.ReadByte();
            var g = reader.ReadByte();
            var r = reader.ReadByte();
            var a = depth == 32 ? reader.ReadByte() : (byte)255;
            return new SKColor(r, g, b, a);
        }

        var count = pixels.Length;
        for (var i = 0; i < count;)
        {
            if (imageType == 2)
            {
                SetPixel(i++, ReadPixel());
                continue;
            }

            var packet = reader.ReadByte();
            var runLength = (packet & 0x7F) + 1;
            if (runLength > count - i)
                throw new InvalidDataException("Invalid TGA RLE packet length.");

            if ((packet & 0x80) != 0)
            {
                var color = ReadPixel();
                for (var n = 0; n < runLength; n++) SetPixel(i++, color);
            }
            else
            {
                for (var n = 0; n < runLength; n++) SetPixel(i++, ReadPixel());
            }
        }

        return pixels;
    }

    private static void CheckDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height > 32_000_000)
            throw new InvalidDataException("Image dimensions are invalid or exceed the 32-million-pixel limit.");
    }

    private static SKColor Quantize(SKColor color, MonoGamePixelFormat format)
    {
        static byte Q(byte value, int max)
        {
            var reduced = (value * max + 127) / 255;
            return (byte)((reduced * 255 + max / 2) / max);
        }

        return format switch
        {
            MonoGamePixelFormat.Bgr565 => QuantizeOpaque(color),
            MonoGamePixelFormat.Bgra5551 => new SKColor(Q(color.Red, 31), Q(color.Green, 31), Q(color.Blue, 31),
                color.Alpha >= 128 ? (byte)255 : (byte)0),
            MonoGamePixelFormat.Bgra4444 => new SKColor(Q(color.Red, 15), Q(color.Green, 15), Q(color.Blue, 15),
                Q(color.Alpha, 15)),
            MonoGamePixelFormat.Alpha8 => new SKColor(255, 255, 255, color.Alpha),
            _ => color
        };

        static SKColor QuantizeOpaque(SKColor color)
        {
            color = BlendWhite(color);
            return new SKColor(Q(color.Red, 31), Q(color.Green, 63), Q(color.Blue, 31), 255);
        }
    }

    private static SKColor BlendWhite(SKColor color)
    {
        if (color.Alpha == 255) return color;
        var a = color.Alpha;
        return new SKColor(
            (byte)((color.Red * a + 255 * (255 - a) + 127) / 255),
            (byte)((color.Green * a + 255 * (255 - a) + 127) / 255),
            (byte)((color.Blue * a + 255 * (255 - a) + 127) / 255), 255);
    }

    private static SKColor[] ToOpaque(SKColor[] pixels)
    {
        var opaque = new SKColor[pixels.Length];
        for (var i = 0; i < pixels.Length; i++) opaque[i] = BlendWhite(pixels[i]);
        return opaque;
    }

    private static void SaveSkia(string path, SKColor[] pixels, int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Pixels = pixels;
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 95) ?? throw new NotSupportedException($"The {format} encoder is not available.");
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }

    private static void SaveBmp(string path, SKColor[] pixels, int width, int height)
    {
        var rowBytes = checked((width * 3 + 3) & ~3);
        var pixelBytes = checked(rowBytes * height);
        using var writer = new BinaryWriter(File.Create(path));

        writer.Write((ushort)0x4D42); // BM
        writer.Write(checked(54 + pixelBytes));
        writer.Write(0);
        writer.Write(54);
        writer.Write(40); // BITMAPINFOHEADER
        writer.Write(width);
        writer.Write(height); // Bottom-up
        writer.Write((ushort)1);
        writer.Write((ushort)24);
        writer.Write(0);
        writer.Write(pixelBytes);
        writer.Write(2835);
        writer.Write(2835);
        writer.Write(0);
        writer.Write(0);

        var padding = new byte[rowBytes - width * 3];
        for (var y = height - 1; y >= 0; y--)
        {
            for (var x = 0; x < width; x++)
            {
                var c = BlendWhite(pixels[y * width + x]);
                writer.Write(c.Blue);
                writer.Write(c.Green);
                writer.Write(c.Red);
            }
            writer.Write(padding);
        }
    }

    private static void SaveTga(string path, SKColor[] pixels, int width, int height)
    {
        if (width > ushort.MaxValue || height > ushort.MaxValue)
            throw new InvalidDataException("TGA requires dimensions of 65535 pixels or less.");

        using var writer = new BinaryWriter(File.Create(path));
        writer.Write((byte)0); // No ID
        writer.Write((byte)0); // No color map
        writer.Write((byte)2); // Uncompressed true color
        writer.Write(new byte[9]); // Color map specification and image origin
        writer.Write((ushort)width);
        writer.Write((ushort)height);
        writer.Write((byte)32);
        writer.Write((byte)0x28); // 8 alpha bits, top-left origin

        foreach (var c in pixels)
        {
            writer.Write(c.Blue);
            writer.Write(c.Green);
            writer.Write(c.Red);
            writer.Write(c.Alpha);
        }
    }

    private static void SaveRaw(string path, SKColor[] pixels, MonoGamePixelFormat format)
    {
        using var writer = new BinaryWriter(File.Create(path));
        foreach (var c in pixels)
        {
            switch (format)
            {
                case MonoGamePixelFormat.Color:
                    writer.Write(c.Red);
                    writer.Write(c.Green);
                    writer.Write(c.Blue);
                    writer.Write(c.Alpha);
                    break;
                case MonoGamePixelFormat.Bgra32:
                    writer.Write(c.Blue);
                    writer.Write(c.Green);
                    writer.Write(c.Red);
                    writer.Write(c.Alpha);
                    break;
                case MonoGamePixelFormat.Bgr565:
                    writer.Write((ushort)((Pack(c.Red, 31) << 11) | (Pack(c.Green, 63) << 5) | Pack(c.Blue, 31)));
                    break;
                case MonoGamePixelFormat.Bgra5551:
                    writer.Write((ushort)(((c.Alpha >= 128 ? 1 : 0) << 15) |
                        (Pack(c.Red, 31) << 10) | (Pack(c.Green, 31) << 5) | Pack(c.Blue, 31)));
                    break;
                case MonoGamePixelFormat.Bgra4444:
                    writer.Write((ushort)((Pack(c.Alpha, 15) << 12) | (Pack(c.Red, 15) << 8) |
                        (Pack(c.Green, 15) << 4) | Pack(c.Blue, 15)));
                    break;
                case MonoGamePixelFormat.Alpha8:
                    writer.Write(c.Alpha);
                    break;
                default: throw new NotSupportedException($"Unsupported MonoGame SurfaceFormat: {format}");
            }
        }
    }

    private static int Pack(byte component, int max) => (component * max + 127) / 255;
}

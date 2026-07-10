using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    public static class ImageDetection
    {
        public static ImageFormatType DetectFormat(Stream stream)
        {
            if (stream == null || stream.Length < 4)
                return ImageFormatType.Unknown;

            long originalPos = stream.Position;

            Span<byte> header = stackalloc byte[32];
            int read = stream.Read(header);
            stream.Position = originalPos;

            return DetectHeader(header[..read]);
        }

        public static async ValueTask<ImageFormatType> DetectFormatAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (stream.Length < 4)
                return ImageFormatType.Unknown;

            long originalPos = stream.Position;

            byte[] header = new byte[32];

            int read = await stream.ReadAsync(
                header.AsMemory(),
                cancellationToken);

            stream.Position = originalPos;

            return DetectHeader(header.AsSpan(0, read));
        }

        private static ImageFormatType DetectHeader(ReadOnlySpan<byte> header)
        {
            if (header.Length < 4)
                return ImageFormatType.Unknown;

            // PNG
            if (header.Length >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47)
            {
                return ImageFormatType.Png;
            }

            // JPG
            if (header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
            {
                return ImageFormatType.Jpeg;
            }

            // BMP
            if (header[0] == (byte)'B' &&
                header[1] == (byte)'M')
            {
                return ImageFormatType.Bmp;
            }

            // GIF (ignorieren)
            if (header[0] == (byte)'G' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F')
            {
                return ImageFormatType.Unknown;
            }

            // TGA (heuristisch)
            if (header.Length >= 18)
            {
                byte imageType = header[2];
                if (imageType == 2 || imageType == 10)
                {
                    return ImageFormatType.Tga;
                }
            }

            // DDS
            if (header[0] == (byte)'D' &&
                header[1] == (byte)'D' &&
                header[2] == (byte)'S' &&
                header[3] == (byte)' ')
            {
                return ImageFormatType.Dds;
            }

            return ImageFormatType.Unknown;
        }
    }
}
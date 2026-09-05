using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Provides methods for detecting image formats from streams.
    /// </summary>
    public static class ImageDetection
    {
        /// <summary>
        /// Detects the image format of the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the image data.
        /// </param>
        /// <returns>
        /// The detected image format, or
        /// <see cref="ImageFormatType.Unknown"/> when the format
        /// cannot be detected.
        /// </returns>
        public static ImageFormatType DetectFormat(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek || stream.Length < 4)
                return ImageFormatType.Unknown;

            long originalPosition = stream.Position;

            Span<byte> header = stackalloc byte[32];
            int read = stream.Read(header);

            stream.Position = originalPosition;

            return DetectHeader(header[..read]);
        }

        /// <summary>
        /// Asynchronously detects the image format of the specified stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the image data.
        /// </param>
        /// <param name="cancellationToken">
        /// The token used to cancel the detection operation.
        /// </param>
        /// <returns>
        /// The detected image format, or
        /// <see cref="ImageFormatType.Unknown"/> when the format
        /// cannot be detected.
        /// </returns>
        public static async ValueTask<ImageFormatType> DetectFormatAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek || stream.Length < 4)
                return ImageFormatType.Unknown;

            long originalPosition = stream.Position;

            byte[] header = new byte[32];

            int read = await stream.ReadAsync(
                header.AsMemory(),
                cancellationToken).ConfigureAwait(false);

            stream.Position = originalPosition;

            return DetectHeader(header.AsSpan(0, read));
        }

        /// <summary>
        /// Detects an image format from its file header.
        /// </summary>
        /// <param name="header">
        /// The image header bytes.
        /// </param>
        /// <returns>
        /// The detected image format, or
        /// <see cref="ImageFormatType.Unknown"/> when the header
        /// is not recognized.
        /// </returns>
        private static ImageFormatType DetectHeader(
            ReadOnlySpan<byte> header)
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

            // JPEG
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

            // GIF is currently not supported.
            if (header[0] == (byte)'G' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F')
            {
                return ImageFormatType.Unknown;
            }

            // TGA
            if (header.Length >= 18)
            {
                byte imageType = header[2];

                if (imageType == 2 || imageType == 10)
                    return ImageFormatType.Tga;
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
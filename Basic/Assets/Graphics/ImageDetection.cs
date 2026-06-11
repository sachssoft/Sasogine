using System;
using System.IO;

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

            if (read < 4)
                return ImageFormatType.Unknown;

            // =========================
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            // =========================
            if (read >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47)
            {
                return ImageFormatType.Png;
            }

            // =========================
            // JPG: FF D8 FF
            // =========================
            if (header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
            {
                return ImageFormatType.Jpeg;
            }

            // =========================
            // BMP: "BM"
            // =========================
            if (header[0] == (byte)'B' &&
                header[1] == (byte)'M')
            {
                return ImageFormatType.Bmp;
            }

            // =========================
            // GIF optional (nicht im Enum, aber oft erkannt)
            // =========================
            if (header[0] == (byte)'G' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F')
            {
                return ImageFormatType.Unknown; // bewusst ignoriert für deine Engine
            }

            // =========================
            // TGA (heuristisch)
            // =========================
            // TGA hat keinen festen Magic Header am Anfang
            // klassisch: uncompressed truecolor = type 2
            if (read >= 18)
            {
                byte imageType = header[2];
                if (imageType == 2 || imageType == 10)
                {
                    // sehr einfache Heuristik
                    return ImageFormatType.Tga;
                }
            }

            // =========================
            // DDS: "DDS "
            // =========================
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
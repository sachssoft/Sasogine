using System;
using System.IO;
using System.IO.Compression;

namespace Sachssoft.Sasogine.Containers
{
    public static class AssetRecognizer
    {
        public static AssetType Detect(PackageAssetSource packageAssetEntry)
        {
            using var stream = packageAssetEntry.Open();
            return Detect(stream);
        }

        public static AssetType Detect(Stream stream)
        {
            Span<byte> header = stackalloc byte[16];
            int read = stream.Read(header);

            if (read >= 8)
            {
                if (header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47 })) // PNG
                    return AssetType.Texture;

                if (header[0] == 0xFF && header[1] == 0xD8) // JPG
                    return AssetType.Texture;

                if (header.StartsWith("GIF8"u8)) // GIF
                    return AssetType.Texture;

                if (header.StartsWith("BM"u8)) // BMP
                    return AssetType.Texture;

                if (header.StartsWith("OggS"u8)) // OGG
                    return AssetType.Sound;

                if (header.StartsWith("RIFF"u8)) // WAV oder AVI
                {
                    if (header.Slice(8).StartsWith("WAVE"u8))
                        return AssetType.Sound;
                }

                if (header.StartsWith("ID3"u8) || (header[0] == 0xFF && header[1] == 0xFB)) // MP3
                    return AssetType.Music;

                if (header.StartsWith("MGFX"u8)) // MonoGame Shader
                    return AssetType.Shader;

                if (header.StartsWith(new byte[] { 0x00, 0x01, 0x00, 0x00 }) ||
                    header.StartsWith("OTTO"u8)) // TTF / OTF
                    return AssetType.Font;
            }

            // Fallback → Text oder JSON?
            stream.Position = 0;
            using var reader = new StreamReader(stream, leaveOpen: true);
            char[] buf = new char[1];
            if (reader.Read(buf, 0, 1) == 1)
            {
                if (buf[0] == '{' || buf[0] == '[')
                    return AssetType.Data;
            }

            return AssetType.Unknown;
        }
    }
}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Helper-Klasse zur Erkennung von plattformunabhängigen Audioformaten
    /// </summary>
    public static class AudioDetection
    {
        /// <summary>
        /// Ermittelt das Audioformat anhand der Magic Bytes des Streams.
        /// Unterstützt WAV, OGG und MP3.
        /// </summary>
        public static AudioFormatType DetectFormat(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek || stream.Length < 4)
                return AudioFormatType.Unknown;

            long originalPos = stream.Position;

            Span<byte> header = stackalloc byte[12];

            int bytesRead = stream.Read(header);

            stream.Position = originalPos;

            return DetectHeader(header[..bytesRead]);
        }


        /// <summary>
        /// Ermittelt das Audioformat asynchron anhand der Magic Bytes des Streams.
        /// </summary>
        public static async ValueTask<AudioFormatType> DetectFormatAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek || stream.Length < 4)
                return AudioFormatType.Unknown;

            long originalPos = stream.Position;

            byte[] header = new byte[12];

            int bytesRead = await stream.ReadAsync(
                header.AsMemory(),
                cancellationToken);

            stream.Position = originalPos;

            return DetectHeader(header.AsSpan(0, bytesRead));
        }


        private static AudioFormatType DetectHeader(ReadOnlySpan<byte> header)
        {
            if (header.Length < 4)
                return AudioFormatType.Unknown;


            // WAV: "RIFF....WAVE"
            if (header.Length >= 12 &&
                header[0] == 'R' &&
                header[1] == 'I' &&
                header[2] == 'F' &&
                header[3] == 'F' &&
                header[8] == 'W' &&
                header[9] == 'A' &&
                header[10] == 'V' &&
                header[11] == 'E')
            {
                return AudioFormatType.Wav;
            }


            // OGG: "OggS"
            if (header[0] == (byte)'O' &&
                header[1] == (byte)'g' &&
                header[2] == (byte)'g' &&
                header[3] == (byte)'S')
            {
                return AudioFormatType.Ogg;
            }


            // MP3 Frame Sync: 11111111 111xxxxx
            if (header.Length >= 2 &&
                header[0] == 0xFF &&
                (header[1] & 0xE0) == 0xE0)
            {
                return AudioFormatType.Mp3;
            }


            return AudioFormatType.Unknown;
        }
    }
}
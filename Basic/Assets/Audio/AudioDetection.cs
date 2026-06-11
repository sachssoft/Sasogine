using Sachssoft.Sasogine.Resources;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Helper-Klasse zur Erkennung von plattformunabhängigen Audioformaten
    /// </summary>
    public static class AudioDetection
    {

        /// <summary>
        /// Ermittelt das Audioformat anhand der Magic Bytes des Streams.
        /// Unterstützt nur plattformunabhängige Formate: WAV, OGG, MP3
        /// </summary>
        /// <param name="stream">Stream mit Audiodaten</param>
        /// <returns>Erkanntes AudioFormatType</returns>
        public static AudioFormatType DetectFormat(Stream stream)
        {
            if (stream == null || stream.Length < 4)
                return AudioFormatType.Unknown;

            long originalPos = stream.Position;

            byte[] header = new byte[12];
            int bytesRead = stream.Read(header, 0, int.Min(header.Length, (int)stream.Length));
            stream.Position = originalPos;

            if (bytesRead < 4)
                return AudioFormatType.Unknown;

            // WAV: "RIFF....WAVE"
            if (bytesRead >= 12 &&
                header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F' &&
                header[8] == 'W' && header[9] == 'A' && header[10] == 'V' && header[11] == 'E')
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

            // MP3 Frame Sync: 0xFFEx
            if ((header[0] & 0xFF) == 0xFF && (header[1] & 0xE0) == 0xE0)
            {
                return AudioFormatType.Mp3;
            }

            return AudioFormatType.Unknown;
        }
    }
}
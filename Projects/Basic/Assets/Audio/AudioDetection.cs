using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Provides utility methods for detecting audio formats from stream headers.
/// </summary>
public static class AudioDetection
{
    private const int HeaderSize = 12;

    /// <summary>
    /// Detects the audio format of the specified stream by inspecting
    /// its header bytes.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the audio data.
    /// </param>
    /// <returns>
    /// The detected <see cref="AudioFormatType"/>, or
    /// <see cref="AudioFormatType.Unknown"/> if the format cannot be determined.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static AudioFormatType DetectFormat(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek || stream.Length - stream.Position < 4)
            return AudioFormatType.Unknown;

        long originalPosition = stream.Position;

        Span<byte> header = stackalloc byte[HeaderSize];

        try
        {
            int bytesRead = stream.Read(header);

            return DetectHeader(
                header[..bytesRead]);
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Asynchronously detects the audio format of the specified stream
    /// by inspecting its header bytes.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the audio data.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A value task containing the detected <see cref="AudioFormatType"/>,
    /// or <see cref="AudioFormatType.Unknown"/> if the format cannot be determined.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled.
    /// </exception>
    public static async ValueTask<AudioFormatType> DetectFormatAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek || stream.Length - stream.Position < 4)
            return AudioFormatType.Unknown;

        long originalPosition = stream.Position;
        byte[] header = new byte[HeaderSize];

        try
        {
            int bytesRead = await stream.ReadAsync(
                header.AsMemory(),
                cancellationToken).ConfigureAwait(false);

            return DetectHeader(
                header.AsSpan(0, bytesRead));
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static AudioFormatType DetectHeader(
        ReadOnlySpan<byte> header)
    {
        if (header.Length < 4)
            return AudioFormatType.Unknown;

        if (header.Length >= 12 &&
            header[0] == (byte)'R' &&
            header[1] == (byte)'I' &&
            header[2] == (byte)'F' &&
            header[3] == (byte)'F' &&
            header[8] == (byte)'W' &&
            header[9] == (byte)'A' &&
            header[10] == (byte)'V' &&
            header[11] == (byte)'E')
        {
            return AudioFormatType.Wav;
        }

        if (header[0] == (byte)'O' &&
            header[1] == (byte)'g' &&
            header[2] == (byte)'g' &&
            header[3] == (byte)'S')
        {
            return AudioFormatType.Ogg;
        }

        if (header[0] == (byte)'I' &&
            header[1] == (byte)'D' &&
            header[2] == (byte)'3')
        {
            return AudioFormatType.Mp3;
        }

        if (header[0] == 0xFF &&
            (header[1] & 0xE0) == 0xE0)
        {
            return AudioFormatType.Mp3;
        }

        return AudioFormatType.Unknown;
    }
}
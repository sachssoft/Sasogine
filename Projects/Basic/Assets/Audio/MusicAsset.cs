using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Represents a music asset that creates an <see cref="IMusicPlayer"/>
/// from an audio resource stream.
/// </summary>
/// <remarks>
/// Supported music formats include MP3 and OGG.
/// When <see cref="MusicFormatType.Auto"/> is used, the format is detected
/// automatically from the resource stream.
/// </remarks>
public class MusicAsset :
    AssetBase<IMusicPlayer, MusicAssetDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MusicAsset"/> class
    /// using a default <see cref="MusicAssetDefinition"/>.
    /// </summary>
    public MusicAsset()
        : base(new MusicAssetDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the music asset.
    /// </param>
    public MusicAsset(MusicAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Resolves a default definition when no definition is currently available.
    /// </summary>
    /// <returns>
    /// A new <see cref="MusicAssetDefinition"/>.
    /// </returns>
    protected override MusicAssetDefinition ResolveDefinition()
    {
        return new MusicAssetDefinition();
    }

    /// <summary>
    /// Builds a music player from the specified audio stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded music data.
    /// </param>
    /// <returns>
    /// The music player created from the stream.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream contains no data.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the audio format is unsupported or cannot be detected.
    /// </exception>
    protected override IMusicPlayer? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Length == 0)
        {
            throw new ArgumentException(
                "The music stream contains no data.",
                nameof(stream));
        }

        IMusicPlayer instance = Definition.FormatType switch
        {
            MusicFormatType.Auto =>
                CreateFromDetectedFormat(stream),

            MusicFormatType.Ogg =>
                new OggStreamPlayer(stream),

            MusicFormatType.Mp3 =>
                new Mp3StreamPlayer(stream),

            _ => throw new FormatException(
                $"Unsupported music format '{Definition.FormatType}'.")
        };

        instance.Volume = Definition.Volume;
        instance.Pitch = Definition.Pitch;
        instance.StartOffset = Definition.StartOffset;
        instance.IsLooping = Definition.IsLooping;

        return instance;
    }

    private static IMusicPlayer CreateFromDetectedFormat(Stream stream)
    {
        return AudioDetection.DetectFormat(stream) switch
        {
            AudioFormatType.Ogg =>
                new OggStreamPlayer(stream),

            AudioFormatType.Mp3 =>
                new Mp3StreamPlayer(stream),

            _ => throw new FormatException(
                "Unsupported music format. Only MP3 and OGG are supported.")
        };
    }
}
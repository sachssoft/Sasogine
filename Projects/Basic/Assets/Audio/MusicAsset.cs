using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Represents a managed music asset for the Sasogine audio system.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MusicAsset"/> creates and configures an <see cref="IMusicPlayer"/>
/// from an encoded audio resource stream.
/// </para>
/// <para>
/// Supported music formats include MP3 and OGG. When
/// <see cref="MusicFormatType.Auto"/> is used, the format is detected
/// automatically from the resource stream.
/// </para>
/// </remarks>
public class MusicAsset : AssetBase<IMusicPlayer, MusicAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="MusicAsset"/> class.
    /// </summary>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public MusicAsset(
        string? id = null,
        string? @class = null)
        : base(new MusicAssetDefinition
        {
            Id = id,
            Class = @class,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition containing the music configuration.
    /// </param>
    public MusicAsset(
        MusicAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>
    /// A new <see cref="MusicAssetDefinition"/> instance.
    /// </returns>
    protected override MusicAssetDefinition ResolveDefinition()
    {
        return new MusicAssetDefinition();
    }

    /// <summary>
    /// Builds the runtime music player from the supplied audio stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded music data.
    /// </param>
    /// <returns>
    /// The created and configured <see cref="IMusicPlayer"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The supplied stream contains no data.
    /// </exception>
    /// <exception cref="FormatException">
    /// The configured or detected audio format is unsupported.
    /// </exception>
    protected override IMusicPlayer? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Length == 0)
            throw new ArgumentException("The music stream contains no data.", nameof(stream));

        IMusicPlayer instance = Definition.FormatType switch
        {
            MusicFormatType.Auto => CreateFromDetectedFormat(stream),
            MusicFormatType.Ogg => new OggStreamPlayer(stream),
            MusicFormatType.Mp3 => new Mp3StreamPlayer(stream),
            _ => throw new FormatException(
                $"Unsupported music format '{Definition.FormatType}'.")
        };

        instance.Volume = Definition.Volume;
        instance.Pitch = Definition.Pitch;
        instance.StartOffset = Definition.StartOffset;
        instance.IsLooping = Definition.IsLooping;

        return instance;
    }

    /// <summary>
    /// Creates a music player by detecting the audio format of the supplied stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded music data.
    /// </param>
    /// <returns>
    /// A music player suitable for the detected audio format.
    /// </returns>
    /// <exception cref="FormatException">
    /// The detected audio format is unsupported.
    /// </exception>
    private static IMusicPlayer CreateFromDetectedFormat(Stream stream)
    {
        return AudioDetection.DetectFormat(stream) switch
        {
            AudioFormatType.Ogg => new OggStreamPlayer(stream),
            AudioFormatType.Mp3 => new Mp3StreamPlayer(stream),
            _ => throw new FormatException(
                "Unsupported music format. Only MP3 and OGG are supported.")
        };
    }
}
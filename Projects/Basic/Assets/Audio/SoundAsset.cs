using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Represents a managed sound asset for the Sasogine audio system.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SoundAsset"/> creates and configures an <see cref="ISoundPlayer"/>
/// from an encoded audio resource stream.
/// </para>
/// <para>
/// Supported sound formats include WAV and OGG. When
/// <see cref="SoundFormatType.Auto"/> is used, the format is detected
/// automatically from the resource stream.
/// </para>
/// </remarks>
public class SoundAsset :
    AssetBase<ISoundPlayer, SoundAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="SoundAsset"/> class.
    /// </summary>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public SoundAsset(
        string? id = null,
        string? @class = null)
        : base(new SoundAssetDefinition
        {
            Id = id,
            Class = @class,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition containing the sound configuration.
    /// </param>
    public SoundAsset(
        SoundAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>
    /// A new <see cref="SoundAssetDefinition"/> instance.
    /// </returns>
    protected override SoundAssetDefinition ResolveDefinition()
    {
        return new SoundAssetDefinition();
    }

    /// <summary>
    /// Builds the runtime sound player from the supplied audio stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded sound data.
    /// </param>
    /// <returns>
    /// The created and configured <see cref="ISoundPlayer"/> instance.
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
    protected override ISoundPlayer? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Length == 0)
            throw new ArgumentException("The sound stream contains no data.", nameof(stream));

        ISoundPlayer instance = Definition.FormatType switch
        {
            SoundFormatType.Auto => CreateFromDetectedFormat(stream),
            SoundFormatType.Ogg => new OggStreamPlayer(stream),
            SoundFormatType.Wav => new WavPlayer(stream),
            _ => throw new FormatException(
                $"Unsupported sound format '{Definition.FormatType}'.")
        };

        instance.Volume = Definition.Volume;
        instance.Pitch = Definition.Pitch;

        return instance;
    }

    /// <summary>
    /// Creates a sound player by detecting the audio format of the supplied stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded sound data.
    /// </param>
    /// <returns>
    /// A sound player suitable for the detected audio format.
    /// </returns>
    /// <exception cref="FormatException">
    /// The detected audio format is unsupported.
    /// </exception>
    private static ISoundPlayer CreateFromDetectedFormat(Stream stream)
    {
        return AudioDetection.DetectFormat(stream) switch
        {
            AudioFormatType.Wav => new WavPlayer(stream),
            AudioFormatType.Ogg => new OggStreamPlayer(stream),
            _ => throw new FormatException(
                "Unsupported sound format. Only WAV and OGG are supported.")
        };
    }
}
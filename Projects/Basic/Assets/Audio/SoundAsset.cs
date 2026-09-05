using Sachssoft.Sasogine.Audio;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Represents a sound asset that creates an <see cref="ISoundPlayer"/>
/// from an audio resource stream.
/// </summary>
/// <remarks>
/// Supported sound formats include WAV and OGG.
/// When <see cref="SoundFormatType.Auto"/> is used, the format is detected
/// automatically from the resource stream.
/// </remarks>
public class SoundAsset :
    AssetBase<ISoundPlayer, SoundAssetDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoundAsset"/> class
    /// using a default <see cref="SoundAssetDefinition"/>.
    /// </summary>
    public SoundAsset()
        : base(new SoundAssetDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the sound asset.
    /// </param>
    public SoundAsset(SoundAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Resolves a default definition when no definition is currently available.
    /// </summary>
    /// <returns>
    /// A new <see cref="SoundAssetDefinition"/>.
    /// </returns>
    protected override SoundAssetDefinition ResolveDefinition()
    {
        return new SoundAssetDefinition();
    }

    /// <summary>
    /// Builds a sound player from the specified audio stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the encoded sound data.
    /// </param>
    /// <returns>
    /// The sound player created from the stream.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream contains no data.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the audio format is unsupported or cannot be detected.
    /// </exception>
    protected override ISoundPlayer? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.Length == 0)
        {
            throw new ArgumentException(
                "The sound stream contains no data.",
                nameof(stream));
        }

        ISoundPlayer instance = Definition.FormatType switch
        {
            SoundFormatType.Auto =>
                CreateFromDetectedFormat(stream),

            SoundFormatType.Ogg =>
                new OggStreamPlayer(stream),

            SoundFormatType.Wav =>
                new WavPlayer(stream),

            _ => throw new FormatException(
                $"Unsupported sound format '{Definition.FormatType}'.")
        };

        instance.Volume = Definition.Volume;
        instance.Pitch = Definition.Pitch;

        return instance;
    }

    private static ISoundPlayer CreateFromDetectedFormat(Stream stream)
    {
        return AudioDetection.DetectFormat(stream) switch
        {
            AudioFormatType.Wav =>
                new WavPlayer(stream),

            AudioFormatType.Ogg =>
                new OggStreamPlayer(stream),

            _ => throw new FormatException(
                "Unsupported sound format. Only WAV and OGG are supported.")
        };
    }
}
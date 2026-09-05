namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Defines the configuration used to create and initialize a
/// <see cref="SoundAsset"/>.
/// </summary>
public class SoundAssetDefinition : AssetDefinitionBase<SoundAsset>
{
    /// <summary>
    /// Gets or sets the encoded sound format.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="SoundFormatType.Auto"/>, the format is detected
    /// from the audio stream when the asset is loaded.
    /// </remarks>
    public SoundFormatType FormatType { get; set; }

    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    /// <remarks>
    /// A value of <c>0</c> represents silence and <c>1</c> represents
    /// the default full volume.
    /// </remarks>
    public float Volume { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether playback repeats
    /// after reaching the end of the sound.
    /// </summary>
    public bool IsLooping { get; set; }

    /// <summary>
    /// Gets or sets the playback pitch multiplier.
    /// </summary>
    /// <remarks>
    /// A value of <c>1</c> represents the normal playback pitch.
    /// </remarks>
    public float Pitch { get; set; }

    /// <summary>
    /// Gets or sets the category associated with the sound asset.
    /// </summary>
    public SoundCategory Category { get; set; }
}
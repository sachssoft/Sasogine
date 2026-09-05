using System;

namespace Sachssoft.Sasogine.Assets.Audio;

/// <summary>
/// Defines the configuration used to create and initialize a
/// <see cref="MusicAsset"/>.
/// </summary>
public class MusicAssetDefinition : AssetDefinitionBase<MusicAsset>
{
    /// <summary>
    /// Gets or sets the encoded music format.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="MusicFormatType.Auto"/>, the format is detected
    /// from the audio stream when the asset is loaded.
    /// </remarks>
    public MusicFormatType FormatType { get; set; }

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
    /// after reaching the end of the music.
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
    /// Gets or sets the playback position from which the music starts.
    /// </summary>
    public TimeSpan StartOffset { get; set; }

    /// <summary>
    /// Gets or sets the category associated with the music asset.
    /// </summary>
    public MusicCategory Category { get; set; }
}
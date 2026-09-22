using Sachssoft.Engine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets.Audio;

/// <summary>
/// Defines the configuration used to create and initialize a
/// <see cref="SoundAsset"/>.
/// </summary>
public class SoundAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the encoded sound format.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="SoundFormatType.Auto"/>, the format is detected
    /// from the audio stream when the asset is loaded.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Format")]
    public SoundFormatType FormatType { get; set; }

    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    /// <remarks>
    /// A value of <c>0</c> represents silence and <c>1</c> represents
    /// the default full volume.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Volume")]
    public float Volume { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether playback repeats
    /// after reaching the end of the sound.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Looping")]
    public bool IsLooping { get; set; }

    /// <summary>
    /// Gets or sets the playback pitch multiplier.
    /// </summary>
    /// <remarks>
    /// A value of <c>1</c> represents the normal playback pitch.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Pitch")]
    public float Pitch { get; set; }

    /// <summary>
    /// Gets or sets the category associated with the sound asset.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Category")]
    public SoundCategory Category { get; set; }
}
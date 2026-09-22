using Sachssoft.Engine.Components.Models;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets.Audio;

/// <summary>
/// Defines the configuration used to create and initialize a
/// <see cref="MusicAsset"/>.
/// </summary>
public class MusicAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the encoded music format.
    /// </summary>
    /// <remarks>
    /// When set to <see cref="MusicFormatType.Auto"/>, the format is detected
    /// from the audio stream when the asset is loaded.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Format")]
    public MusicFormatType FormatType { get; set; }

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
    /// after reaching the end of the music.
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
    /// Gets or sets the playback position from which the music starts.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Start Offset")]
    public TimeSpan StartOffset { get; set; }

    /// <summary>
    /// Gets or sets the category associated with the music asset.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Category")]
    public MusicCategory Category { get; set; }
}
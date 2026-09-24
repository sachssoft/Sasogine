using Sachssoft.Engine.Collections;
using Sachssoft.Engine.Components.Definitions;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Provides a base implementation for asset definitions that describe
/// how an asset is identified, categorized, and loaded.
/// </summary>
public abstract class AssetDefinitionBase : IAssetDefinition
{
    /// <summary>
    /// Gets or sets the optional identifier of the asset.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the optional class used to categorize the asset.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Class")]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the file from which the asset is loaded.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("File")]
    public IAssetFile? File { get; set; }

    /// <summary>
    /// Gets the tags associated with the asset.
    /// </summary>
    /// <remarks>
    /// Tags can be used to categorize, search, and filter assets.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Tags")]
    public TrackableCollection<string> Tags { get; } = new();

    /// <summary>
    /// Gets or sets the optional description of the asset.
    /// </summary>
    /// <remarks>
    /// The description provides additional information about the purpose
    /// or intended usage of the asset.
    /// </remarks>
    [Category(Categories.Common)]
    [DisplayName("Description")]
    public string? Description { get; set; }
}
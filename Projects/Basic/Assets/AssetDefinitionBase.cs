using Sachssoft.Engine.Components.Models;
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
}
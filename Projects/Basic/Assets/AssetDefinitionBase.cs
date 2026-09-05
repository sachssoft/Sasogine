namespace Sachssoft.Sasogine.Assets;

/// <summary>
/// Provides a base implementation for asset definitions that describe
/// how an asset is identified and loaded.
/// </summary>
/// <typeparam name="T">
/// The asset type associated with this definition.
/// </typeparam>
public abstract class AssetDefinitionBase<T> : IAssetDefinition
    where T : class, IAsset
{
    /// <summary>
    /// Gets or sets the optional identifier of the asset.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the optional class used to categorize the asset.
    /// </summary>
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the file associated with the asset.
    /// </summary>
    public IAssetFile? File { get; set; }
}
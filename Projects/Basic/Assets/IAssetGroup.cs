using System.Collections.Generic;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Represents a section that provides assets for an asset store.
/// </summary>
public interface IAssetStoreSection
{
    /// <summary>
    /// Creates the assets provided by this section.
    /// </summary>
    /// <returns>
    /// The assets provided by this section.
    /// </returns>
    IEnumerable<IAsset> CreateAssets();
}
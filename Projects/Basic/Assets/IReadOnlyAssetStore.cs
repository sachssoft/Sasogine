using Sachssoft.Sasogine.Assets;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Provides read-only access to a collection of assets.
/// </summary>
public interface IReadOnlyAssetStore : IReadOnlyCollection<IAsset>
{
    /// <summary>
    /// Determines whether an asset with the specified identifier exists.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an asset with the specified identifier exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(string id);

    /// <summary>
    /// Gets the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// The asset associated with the specified identifier.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// No asset with the specified identifier exists.
    /// </exception>
    IAsset Get(string id);

    /// <summary>
    /// Gets the asset with the specified identifier and type.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The expected asset type.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <returns>
    /// The asset associated with the specified identifier.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// No asset with the specified identifier exists.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The asset is not of type <typeparamref name="TAsset"/>.
    /// </exception>
    TAsset Get<TAsset>(string id)
        where TAsset : class, IAsset;

    /// <summary>
    /// Attempts to get the asset with the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <param name="asset">
    /// When this method returns, contains the asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool TryGet(
        string id,
        out IAsset? asset);

    /// <summary>
    /// Attempts to get the asset with the specified identifier and type.
    /// </summary>
    /// <typeparam name="TAsset">
    /// The expected asset type.
    /// </typeparam>
    /// <param name="id">
    /// The identifier of the asset.
    /// </param>
    /// <param name="asset">
    /// When this method returns, contains the matching asset if found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching asset was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    bool TryGet<TAsset>(
        string id,
        out TAsset? asset)
        where TAsset : class, IAsset;
}

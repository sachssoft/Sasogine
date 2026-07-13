using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Provides functionality to detect asset formats,
    /// create asset files, and create asset definitions.
    /// </summary>
    public interface IAssetResolverProvider
    {
        /// <summary>
        /// Creates an asset file reference from the specified relative path and stream.
        /// </summary>
        /// <param name="relativePath">
        /// Relative path of the asset within the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <returns>
        /// The asset file reference if the asset format is recognized;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> or <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        IAssetFile? Resolve(
            string relativePath,
            Stream stream);


        /// <summary>
        /// Creates the asset definition for the specified asset file.
        /// </summary>
        /// <param name="file">
        /// Asset file reference.
        /// </param>
        /// <returns>
        /// The asset definition associated with the asset file.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="file"/> is <see langword="null"/>.
        /// </exception>
        IAssetDefinition? GetDefinition(
            IAssetFile file);
    }
}
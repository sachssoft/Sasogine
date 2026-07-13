using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Provides functionality to resolve asset files from streams.
    /// </summary>
    public interface IAssetResolverProvider
    {
        /// <summary>
        /// Registers a new asset resolver.
        /// </summary>
        /// <param name="match">
        /// Function that determines whether the specified stream matches an asset type.
        /// </param>
        /// <param name="factory">
        /// Function that creates the corresponding asset file for the specified relative path.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="match"/> or <paramref name="factory"/> is null.
        /// </exception>
        void Register(Func<Stream, bool> match, Func<string, IAssetFile> factory);

        /// <summary>
        /// Resolves a typed asset file from the specified relative path and stream.
        /// </summary>
        /// <param name="relativePath">
        /// Relative path of the asset within the package.
        /// </param>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <returns>
        /// A typed asset file if a matching resolver is found; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="relativePath"/> or <paramref name="stream"/> is null.
        /// </exception>
        IAssetFile? Resolve(string relativePath, Stream stream);
    }
}
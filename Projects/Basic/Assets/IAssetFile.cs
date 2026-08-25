using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents a reference to an asset file inside a package.
    /// </summary>
    public interface IAssetFile : ICloneable, IAssemblyContract
    {
        /// <summary>
        /// Gets the asset definition type associated with this file.
        /// </summary>
        Type AssetType { get; }

        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the relative directory path of the asset.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// Gets the complete relative file path inside the package.
        /// </summary>
        string FullRelativePath { get; }

        /// <summary>
        /// Gets a value indicating whether the asset type could not be resolved.
        /// </summary>
        bool IsUnknown { get; }

        /// <summary>
        /// Gets the error that occurred while processing the asset.
        /// </summary>
        Exception? Error { get; }

        /// <summary>
        /// Invalidates the cached asset definition.
        /// Allows the definition to be created again.
        /// </summary>
        void InvalidateDefinition();

        /// <summary>
        /// Creates a copy of this asset file reference.
        /// </summary>
        /// <returns>
        /// A cloned asset file.
        /// </returns>
        new IAssetFile Clone();

        /// <summary>
        /// Resolves this asset file reference into an asset definition
        /// using the specified stream and resolver.
        /// </summary>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <param name="resolver">
        /// Resolver used to detect the asset format and create the corresponding asset definition.
        ///</param>
        /// <returns>
        /// The resolved asset definition if the asset format is recognized and the asset
        /// definition can be created; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        ///</exception>
        IAssetDefinition? GetDefinition(Stream stream, IAssetResolverProvider? resolver);


        /// <summary>
        /// Resolves this asset file reference into an asset definition
        /// using the specified package path and resolver.
        /// </summary>
        /// <param name="packagePath">
        /// Absolute path to the package directory containing the asset file.
        ///</param>
        /// <param name="resolver">
        /// Resolver used to detect the asset format and create the corresponding asset definition.
        ///</param>
        /// <returns>
        /// The resolved asset definition if the asset file can be loaded and its format is recognized;
        /// otherwise, <see langword="null"/>.
        ///</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="packagePath"/> is null, empty, or consists only of whitespace.
        ///</exception>
        IAssetDefinition? GetDefinition(string packagePath, IAssetResolverProvider? resolver);

    }
}
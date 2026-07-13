using System;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents a reference to an asset file inside a package.
    /// </summary>
    public interface IAssetFile : ICloneable, IAssemblyContract
    {
        /// <summary>
        /// Gets the resolved asset type.
        /// </summary>
        Type? AssetType { get; }

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
        /// Gets a value indicating whether the asset type could not be detected.
        /// </summary>
        bool IsUnknown { get; }

        /// <summary>
        /// Gets the error that occurred while processing the asset.
        /// </summary>
        Exception? Error { get; }

        /// <summary>
        /// Creates a copy of this asset file reference.
        /// </summary>
        /// <returns>
        /// A cloned asset file.
        /// </returns>
        new IAssetFile Clone();
    }
}
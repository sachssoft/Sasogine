using System;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents a strongly typed asset file.
    /// Provides type information for a specific asset type.
    /// </summary>
    /// <typeparam name="T">
    /// The expected asset type.
    /// </typeparam>
    public sealed class TypedAssetFile<T> : IAssetFile
        where T : class, IAsset
    {
        private readonly AssetFile _assetFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedAssetFile{T}"/> class.
        /// </summary>
        /// <param name="relativeFilePath">
        /// Relative path of the asset inside the package.
        /// </param>
        public TypedAssetFile(string relativeFilePath)
        {
            _assetFile = new AssetFile(relativeFilePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedAssetFile{T}"/> class.
        /// </summary>
        /// <param name="assetFile">
        /// Existing asset file reference.
        /// </param>
        public TypedAssetFile(IAssetFile assetFile)
        {
            ArgumentNullException.ThrowIfNull(assetFile);

            _assetFile = new AssetFile(assetFile.FullRelativePath);
        }

        /// <summary>
        /// Gets the expected asset type.
        /// </summary>
        public Type AssetType => typeof(T);

        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        public string Name => _assetFile.Name;

        /// <summary>
        /// Gets the relative directory path of the asset.
        /// </summary>
        public string RelativePath => _assetFile.RelativePath;

        /// <summary>
        /// Gets the complete relative file path inside the package.
        /// </summary>
        public string FullRelativePath => _assetFile.FullRelativePath;

        /// <summary>
        /// Gets whether the asset type could not be detected.
        /// </summary>
        public bool IsUnknown => false;

        /// <summary>
        /// Gets the error that occurred while processing the asset.
        /// </summary>
        public Exception? Error => null;

        /// <summary>
        /// Creates a copy of this asset file reference.
        /// </summary>
        /// <returns>
        /// A cloned asset file.
        /// </returns>
        public IAssetFile Clone()
        {
            return new TypedAssetFile<T>(FullRelativePath);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is not IAssetFile other)
                return false;

            return string.Equals(
                FullRelativePath,
                other.FullRelativePath,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase
                .GetHashCode(FullRelativePath);
        }

        void IAssemblyContract.Initialize()
        {
            // Internal initialization only.
        }
    }
}
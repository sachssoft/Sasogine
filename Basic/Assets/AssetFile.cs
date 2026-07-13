using System;
using Sachssoft.Sasogine;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents an asset file reference inside a package.
    /// </summary>
    public class AssetFile : IAssetFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFile"/> class.
        /// </summary>
        /// <param name="relativeFilePath">
        /// Relative path of the asset inside the package.
        /// </param>
        public AssetFile(string relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                throw new ArgumentException("Invalid asset file path.", nameof(relativeFilePath));

            relativeFilePath = relativeFilePath.Replace('\\', '/');

            FullRelativePath = relativeFilePath;

            Name = System.IO.Path.GetFileNameWithoutExtension(relativeFilePath);

            var directory = System.IO.Path.GetDirectoryName(relativeFilePath);

            RelativePath = directory?.Replace('\\', '/') ?? string.Empty;
        }

        /// <summary>
        /// Gets the detected asset type.
        /// </summary>
        public Type? AssetType { get; internal set; }

        /// <summary>
        /// Gets the asset name without extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the directory path inside the package.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets the complete relative file path inside the package.
        /// </summary>
        public string FullRelativePath { get; }

        /// <summary>
        /// Gets a value indicating whether the asset type is unknown.
        /// </summary>
        public bool IsUnknown { get; internal set; }

        /// <summary>
        /// Gets the error that occurred during processing.
        /// </summary>
        public Exception? Error { get; internal set; }


        /// <summary>
        /// Creates a strongly typed asset file representation.
        /// </summary>
        public TypedAssetFile<T> As<T>()
            where T : class, IAsset
        {
            return new TypedAssetFile<T>(FullRelativePath);
        }


        /// <summary>
        /// Creates a copy of this asset file reference.
        /// </summary>
        public IAssetFile Clone()
        {
            return new AssetFile(FullRelativePath);
        }


        object ICloneable.Clone()
        {
            return Clone();
        }


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


        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FullRelativePath);
        }


        void IAssemblyContract.Initialize()
        {
            // Internal initialization only.
        }
    }
}
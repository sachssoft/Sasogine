using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Represents a strongly typed asset file reference inside a package.
    /// Provides type information and access to its asset definition.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The expected asset definition type.
    /// </typeparam>
    public sealed class AssetFile<TDefinition> : IAssetFile
        where TDefinition : class, IAssetDefinition
    {
        private TDefinition? _definition;
        private bool _hasDefinition;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssetFile{TDefinition}"/> class.
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
        /// Gets the asset definition type associated with this file.
        /// </summary>
        public Type AssetType => typeof(TDefinition);


        /// <summary>
        /// Gets the file name without extension.
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Gets the relative directory path of the asset.
        /// </summary>
        public string RelativePath { get; }


        /// <summary>
        /// Gets the complete relative file path inside the package.
        /// </summary>
        public string FullRelativePath { get; }


        /// <summary>
        /// Gets a value indicating whether the asset type is unknown.
        /// </summary>
        public bool IsUnknown => false;


        /// <summary>
        /// Gets the error that occurred while processing the asset.
        /// </summary>
        public Exception? Error => null;


        /// <summary>
        /// Gets a value indicating whether this asset file has a cached asset definition.
        /// </summary>
        public bool HasDefinition => _hasDefinition;


        /// <summary>
        /// Invalidates the cached asset definition.
        /// </summary>
        public void InvalidateDefinition()
        {
            _definition = null;
            _hasDefinition = false;
        }


        /// <summary>
        /// Gets the asset definition represented by this asset file.
        /// </summary>
        /// <param name="stream">
        /// Stream containing the asset data.
        /// </param>
        /// <param name="resolver">
        /// Resolver used to create the asset definition.
        /// </param>
        /// <returns>
        /// The asset definition if it could be created;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is <see langword="null"/>.
        /// </exception>
        public TDefinition? GetDefinition(
            Stream stream,
            IAssetResolverProvider? resolver = null)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (_hasDefinition)
                return _definition;

            resolver ??= new AssetResolver();

            _definition = resolver.GetDefinition(this) as TDefinition;

            _hasDefinition = true;

            return _definition;
        }


        /// <summary>
        /// Gets the asset definition represented by this asset file.
        /// </summary>
        /// <param name="packagePath">
        /// Absolute path to the package directory containing the asset file.
        /// </param>
        /// <param name="resolver">
        /// Resolver used to create the asset definition.
        /// </param>
        /// <returns>
        /// The asset definition if it could be created;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="packagePath"/> is null, empty,
        /// or consists only of whitespace.
        /// </exception>
        public TDefinition? GetDefinition(
            string packagePath,
            IAssetResolverProvider? resolver = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

            if (_hasDefinition)
                return _definition;

            resolver ??= new AssetResolver();

            var fullPath = System.IO.Path.Combine(packagePath, FullRelativePath);

            using var stream = File.OpenRead(fullPath);

            return GetDefinition(stream, resolver);
        }


        IAssetDefinition? IAssetFile.GetDefinition(
            Stream stream,
            IAssetResolverProvider? resolver)
        {
            return GetDefinition(stream, resolver);
        }


        IAssetDefinition? IAssetFile.GetDefinition(
            string packagePath,
            IAssetResolverProvider? resolver)
        {
            return GetDefinition(packagePath, resolver);
        }


        /// <summary>
        /// Creates a copy of this strongly typed asset file reference.
        /// </summary>
        /// <returns>
        /// A cloned asset file.
        /// </returns>
        public IAssetFile Clone()
        {
            return new AssetFile<TDefinition>(FullRelativePath);
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
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FullRelativePath);
        }


        void IAssemblyContract.Initialize()
        {
            // Internal initialization only.
        }
    }
}
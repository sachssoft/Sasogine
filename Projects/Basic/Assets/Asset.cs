using System;
using System.IO;

namespace Sachssoft.Engine.Assets
{
    /// <summary>
    /// Provides utility methods for working with assets.
    /// </summary>
    public static class Asset
    {
        /// <summary>
        /// Transforms an unknown asset definition into a specific
        /// asset definition.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// Type of the target asset definition.
        /// </typeparam>
        /// <param name="source">
        /// Unknown asset definition to transform.
        /// </param>
        /// <param name="definitionFactory">
        /// Factory used to create the target asset definition.
        /// </param>
        /// <returns>
        /// The created asset definition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when an argument is <see langword="null"/>.
        /// </exception>
        public static TDefinition TransformDefinition<TDefinition>(
            UnknownAssetDefinition source,
            Func<TDefinition> definitionFactory)
            where TDefinition : class, IAssetDefinition
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(definitionFactory);

            var definition = definitionFactory();

            definition.File = source.File;

            return definition;
        }

        /// <summary>
        /// Transforms an asset definition into an unknown asset definition.
        /// </summary>
        /// <param name="source">
        /// Asset definition to transform.
        /// </param>
        /// <returns>
        /// The created unknown asset definition.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        public static UnknownAssetDefinition TransformDefinition(
            IAssetDefinition source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new UnknownAssetDefinition
            {
                File = source.File
            };
        }

        /// <summary>
        /// Determines whether an asset definition references a file.
        /// </summary>
        /// <param name="definition">
        /// Asset definition to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the definition references a file;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasFile(IAssetDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            return definition.File is not null;
        }

        /// <summary>
        /// Determines whether an asset definition references the specified file.
        /// </summary>
        /// <param name="definition">
        /// Asset definition to check.
        /// </param>
        /// <param name="relativePath">
        /// Relative asset file path.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the definition references the specified
        /// file; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasFile(
            IAssetDefinition definition,
            string relativePath)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

            if (definition.File is null)
                return false;

            return string.Equals(
                NormalizePath(definition.File.FullRelativePath),
                NormalizePath(relativePath),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether two asset definitions reference the same file.
        /// </summary>
        /// <param name="first">
        /// First asset definition.
        /// </param>
        /// <param name="second">
        /// Second asset definition.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when both definitions reference the same file;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasSameFile(
            IAssetDefinition first,
            IAssetDefinition second)
        {
            ArgumentNullException.ThrowIfNull(first);
            ArgumentNullException.ThrowIfNull(second);

            if (first.File is null || second.File is null)
                return false;

            return string.Equals(
                NormalizePath(first.File.FullRelativePath),
                NormalizePath(second.File.FullRelativePath),
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normalizes a relative asset file path.
        /// </summary>
        /// <param name="relativePath">
        /// Relative asset file path.
        /// </param>
        /// <returns>
        /// The normalized asset file path.
        /// </returns>
        public static string NormalizePath(string relativePath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

            return relativePath.Replace('\\', '/');
        }

        /// <summary>
        /// Gets the file name of an asset definition.
        /// </summary>
        /// <param name="definition">
        /// Asset definition.
        /// </param>
        /// <returns>
        /// The asset file name, or <see langword="null"/> when the definition
        /// does not reference a file.
        /// </returns>
        public static string? GetFileName(IAssetDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            if (definition.File is null)
                return null;

            return Path.GetFileName(
                definition.File.FullRelativePath);
        }

        /// <summary>
        /// Gets the file extension of an asset definition.
        /// </summary>
        /// <param name="definition">
        /// Asset definition.
        /// </param>
        /// <returns>
        /// The asset file extension, or <see langword="null"/> when the
        /// definition does not reference a file.
        /// </returns>
        public static string? GetExtension(IAssetDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            if (definition.File is null)
                return null;

            return Path.GetExtension(
                definition.File.FullRelativePath);
        }
    }
}
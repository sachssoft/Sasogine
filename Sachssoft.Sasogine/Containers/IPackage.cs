using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Represents a game package containing assets, levels, and metadata.
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// Gets the manifest of the package, containing metadata and configuration.
        /// </summary>
        PackageManifest Manifest { get; }

        /// <summary>
        /// Gets or sets the document formatter used to read or write the manifest.
        /// </summary>
        IDocumentFormatter ManifestFormat { get; set; }

        /// <summary>
        /// Gets the icon representing this package.
        /// </summary>
        PackageIcon Icon { get; }

        /// <summary>
        /// Gets the collection of preview images for this package.
        /// </summary>
        PackagePreviews Previews { get; }

        /// <summary>
        /// Gets a value indicating whether this package is read-only.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether this package is currently open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets a read-only dictionary of assets contained in this package, keyed by their names.
        /// </summary>
        IReadOnlyDictionary<string, IAssetSource> Assets { get; }

        /// <summary>
        /// Gets a read-only collection of levels included in this package.
        /// </summary>
        IReadOnlyList<PackageLevelBase> Levels { get; }
    }
}

using Sachssoft.Sasogine.Resources.Wrappers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Provides access to preview images stored within a package.
    /// Previews are expected to be located in the "previews/" folder inside the package,
    /// with file names like "banner.png" or "screenshot1.png".
    /// </summary>
    public sealed class PackagePreviews
    {
        private const string FILE_PREFIX = "previews/";
        private const string FILE_EXTENSION = ".png";

        /// <summary>
        /// Format for single preview files (e.g. previews/banner.png).
        /// </summary>
        private const string FILE_FORMAT_SINGLE = FILE_PREFIX + "{0}" + FILE_EXTENSION;

        /// <summary>
        /// Format for indexed preview files (e.g. previews/screenshot1.png).
        /// </summary>
        private const string FILE_FORMAT_INDEXED = FILE_PREFIX + "{0}{1}" + FILE_EXTENSION;

        private readonly PackageBase _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackagePreviews"/> class for the specified package.
        /// </summary>
        /// <param name="package">The package containing preview images.</param>
        internal PackagePreviews(PackageBase package)
        {
            _package = package;
        }

        /// <summary>
        /// Gets the first preview image for the specified preview type.
        /// </summary>
        /// <param name="type">The type of preview image.</param>
        /// <returns>The first preview image or <c>null</c> if not found.</returns>
        public Texture2DWrapper? this[PackagePreviewType type] => Get(type, 1);

        /// <summary>
        /// Loads a single preview image for the specified type and index (starting at 1).
        /// </summary>
        /// <param name="type">The type of preview image.</param>
        /// <param name="index">The index of the preview image (1-based).</param>
        /// <returns>The loaded preview image or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is less than 1.</exception>
        public Texture2DWrapper? Get(PackagePreviewType type, int index = 1)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be >= 1.");

            string fileName = index == 1 && type != PackagePreviewType.Screenshot
                ? string.Format(FILE_FORMAT_SINGLE, type.ToString().ToLowerInvariant())
                : string.Format(FILE_FORMAT_INDEXED, type.ToString().ToLowerInvariant(), index);

            return new Texture2DWrapper(_package, (p) => p.OpenFile(fileName));
        }

        /// <summary>
        /// Checks whether a specific preview image exists.
        /// </summary>
        /// <param name="type">The type of preview image.</param>
        /// <param name="index">The index of the preview image (1-based).</param>
        /// <returns><c>true</c> if the preview exists; otherwise, <c>false</c>.</returns>
        public bool Contains(PackagePreviewType type, int index = 1)
        {
            string fileName = index == 1 && type != PackagePreviewType.Screenshot
                ? string.Format(FILE_FORMAT_SINGLE, type.ToString().ToLowerInvariant())
                : string.Format(FILE_FORMAT_INDEXED, type.ToString().ToLowerInvariant(), index);

            return _package.IsFileExists(fileName);
        }

        /// <summary>
        /// Finds and loads all preview images for the specified type.
        /// </summary>
        /// <param name="type">The type of preview images.</param>
        /// <returns>A read-only list of all matching preview images.</returns>
        public IReadOnlyList<Texture2DWrapper> FindAll(PackagePreviewType type)
        {
            var results = new List<Texture2DWrapper>();

            // Look through all entries in the package starting with the preview type prefix
            string prefix = FILE_PREFIX + type.ToString().ToLowerInvariant();

            foreach (var entry in _package.Source!.Entries
                .Where(e => e.FullName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)
                         && e.FullName.EndsWith(FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(e => e.FullName))
            {
                results.Add(new Texture2DWrapper(_package, (p) => entry.Open()));
            }

            return results;
        }
    }
}

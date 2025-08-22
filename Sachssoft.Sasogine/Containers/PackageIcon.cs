using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources.Wrappers;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Provides access to package icon images of different sizes.
    /// Icon files are expected to be named like "icon.png", "icon_tiny.png", "icon_small.png" etc.
    /// </summary>
    public sealed class PackageIcon
    {
        private const string FILE_NAME = "icon";
        private const string FILE_EXTENSION = ".png";

        /// <summary>
        /// Format string for icon file names, e.g. "icon.png" or "icon_tiny.png".
        /// </summary>
        private const string FILE_FORMAT = FILE_NAME + "{0}" + FILE_EXTENSION;

        private readonly PackageBase _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageIcon"/> class for the specified package.
        /// </summary>
        /// <param name="package">The package containing icon images.</param>
        internal PackageIcon(PackageBase package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Gets the icon image for the specified size.
        /// </summary>
        /// <param name="size">The desired icon size.</param>
        /// <returns>The icon image or <c>null</c> if not found.</returns>
        public Texture2DWrapper this[PackageIconSize size] => Get(size);

        /// <summary>
        /// Gets the default icon image (medium size).
        /// </summary>
        public Texture2DWrapper Default => Get(PackageIconSize.Medium);

        /// <summary>
        /// Gets the icon image for the specified size.
        /// </summary>
        /// <param name="size">The desired icon size.</param>
        /// <returns>The icon image or <c>null</c> if not found.</returns>
        public Texture2DWrapper Get(PackageIconSize size)
        {
            string sizeSuffix = size == PackageIconSize.Medium ? "" : "_" + size.ToString().ToLowerInvariant();
            string fileName = string.Format(FILE_FORMAT, sizeSuffix);

            return new Texture2DWrapper(_package, (p) => p.OpenFile(fileName));
        }

        /// <summary>
        /// Checks whether an icon image exists for the specified size.
        /// </summary>
        /// <param name="size">The icon size to check.</param>
        /// <returns><c>true</c> if the icon file exists; otherwise, <c>false</c>.</returns>
        public bool Contains(PackageIconSize size)
        {
            string sizeSuffix = size == PackageIconSize.Medium ? "" : "_" + size.ToString().ToLowerInvariant();
            string fileName = string.Format(FILE_FORMAT, sizeSuffix);

            return _package.IsFileExists(fileName);
        }

        /// <summary>
        /// Sets (updates) the icon image for the medium size inside the package.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> containing the medium size icon image.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the package is readonly or not opened,
        /// or if the <paramref name="texture"/> has invalid dimensions for the medium size.
        /// </exception>
        public void Set(Texture2D texture) => Set(texture, PackageIconSize.Medium);

        /// <summary>
        /// Sets (updates) the icon image for the specified size inside the package.
        /// If an icon file with the given size already exists, it will be replaced.
        /// </summary>
        /// <param name="texture">The <see cref="Texture2D"/> containing the icon image.</param>
        /// <param name="size">The icon size to set.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the package is readonly or not opened,
        /// or if the <paramref name="texture"/> has invalid dimensions for the specified size.
        /// </exception>
        public void Set(Texture2D texture, PackageIconSize size)
        {
            _package.ThrowIfNotOpened();

            if (_package.IsReadOnly)
                throw new InvalidOperationException("Package is readonly");

            CheckValidSize(texture, size);

            string sizeSuffix = size == PackageIconSize.Medium ? "" : "_" + size.ToString().ToLowerInvariant();
            string fileName = string.Format(FILE_FORMAT, sizeSuffix);

            // Texture2D in PNG stream konvertieren
            using (var ms = new MemoryStream())
            {
                texture.SaveAsPng(ms, texture.Width, texture.Height);
                ms.Position = 0;

                // Alte Datei löschen, falls vorhanden
                var existingEntry = _package.Source.GetEntry(fileName);
                existingEntry?.Delete();

                // Neue Datei anlegen und Stream schreiben
                var newEntry = _package.Source.CreateEntry(fileName);

                using (var entryStream = newEntry.Open())
                {
                    ms.CopyTo(entryStream);
                }
            }
        }

        /// <summary>
        /// Validates that the <see cref="Texture2D"/> dimensions match the expected size for the specified icon size.
        /// </summary>
        /// <param name="texture">The texture to validate.</param>
        /// <param name="size">The expected icon size.</param>
        /// <exception cref="InvalidOperationException">Thrown if the texture dimensions do not match the expected size.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the icon size is unknown.</exception>
        private void CheckValidSize(Texture2D texture, PackageIconSize size)
        {
            switch (size)
            {
                case PackageIconSize.Tiny:
                    if (texture.Width != 16 || texture.Height != 16)
                        throw new InvalidOperationException("Tiny icon must be exactly 16x16 pixels.");
                    break;

                case PackageIconSize.Small:
                    if (texture.Width != 32 || texture.Height != 32)
                        throw new InvalidOperationException("Small icon must be exactly 32x32 pixels.");
                    break;

                case PackageIconSize.Medium:
                    if (texture.Width != 64 || texture.Height != 64)
                        throw new InvalidOperationException("Medium icon must be exactly 64x64 pixels.");
                    break;

                case PackageIconSize.Large:
                    if (texture.Width != 128 || texture.Height != 128)
                        throw new InvalidOperationException("Large icon must be exactly 128x128 pixels.");
                    break;

                case PackageIconSize.Huge:
                    if (texture.Width != 256 || texture.Height != 256)
                        throw new InvalidOperationException("Huge icon must be exactly 256x256 pixels.");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(size), "Unknown icon size.");
            }
        }

    }
}

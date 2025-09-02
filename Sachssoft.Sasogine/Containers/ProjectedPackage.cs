using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Documents.Xml;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    public class ProjectedPackage : PackageBase, IPackage
    {
        private readonly ProjectedPackageLevelCollection _levels;
        private readonly ProjectedPackageAssetCollection _assets;
        private string _filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectedPackage"/> class.
        /// </summary>
        /// <param name="filePath">The path to the package file.</param>
        /// <param name="manifest">Optional manifest for the package.</param>
        public ProjectedPackage(string filePath, PackageManifest? manifest = null)
            : base(() => File.Open(filePath, FileMode.Open, FileAccess.ReadWrite), false, manifest)
        {
            _filePath = filePath;
            _assets = new ProjectedPackageAssetCollection(this);
            _levels = new ProjectedPackageLevelCollection(this);
        }

        /// <summary>
        /// Gets the file path of the package.
        /// </summary>
        public string FilePath => _filePath;

        /// <summary>
        /// Gets the asset collection of the package.
        /// </summary>
        public ProjectedPackageAssetCollection Assets => _assets;

        /// <summary>
        /// Gets the level collection of the package.
        /// </summary>
        public ProjectedPackageLevelCollection Levels => _levels;

        /// <summary>
        /// Gets or sets the index of the currently selected level.
        /// </summary>
        public int SelectedLevelIndex { get; set; }

        /// <summary>
        /// Gets the currently selected level in the package.
        /// </summary>
        /// <remarks>
        /// The selected level is determined by the <see cref="SelectedLevelIndex"/> property.
        /// If the index is out of range or the level collection is empty, this property returns <c>null</c>.
        /// </remarks>
        public PackageLevelBase? SelectedLevel
        {
            get
            {
                if (_levels == null || _levels.Entries.Count == 0)
                    return null;

                if (SelectedLevelIndex < 0 || SelectedLevelIndex >= _levels.Entries.Count)
                    throw new IndexOutOfRangeException($"{nameof(SelectedLevelIndex)} is out of the valid range.");

                return _levels[SelectedLevelIndex];
            }
        }

        IReadOnlyDictionary<string, IAssetSource> IPackage.Assets => (IReadOnlyDictionary<string, IAssetSource>)_manifest._assetEntries;

        IReadOnlyCollection<PackageLevelBase> IPackage.Levels => (IReadOnlyCollection<PackageLevelBase>)_manifest._levels;

        /// <summary>
        /// Creates a new package file.
        /// </summary>
        /// <param name="filePath">The path where the new package will be created.</param>
        /// <param name="manifest">Optional manifest to initialize the package.</param>
        /// <param name="formatter">Optional document formatter for the manifest.</param>
        /// <returns>The created <see cref="ProjectedPackage"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the file already exists.</exception>
        public static ProjectedPackage Create(string filePath, PackageManifest? manifest = null, IDocumentFormatter formatter = null)
        {
            if (File.Exists(filePath))
                throw new InvalidOperationException("File already exists");

            // 1. Create empty ZIP archive
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                // Initialize empty ZIP structure
            }

            // 2. Open package in update mode
            var package = Open(filePath, manifest);
            package.Manifest.Save();

            return package;
        }

        /// <summary>
        /// Creates a new package using a factory method for custom types.
        /// </summary>
        public static TPackage Create<TPackage, TManifest>(
            string filePath,
            Func<string, TManifest?, TPackage> createInstance,
            TManifest? manifest = null,
            IDocumentFormatter? formatter = null)
            where TPackage : ProjectedPackage
            where TManifest : PackageManifest
        {
            if (File.Exists(filePath))
                throw new InvalidOperationException("File already exists");

            // 1. Leere ZIP anlegen
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                // Struktur leer anlegen
            }

            // 2. Stream öffnen für Update
            //var fs2 = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

            // 3. Instanz durch Factory erzeugen
            var package = createInstance(filePath, manifest);
            if (package == null)
                throw new InvalidOperationException("createInstance returned null");

            // 4. Package initialisieren
            package.Open();
            package.ManifestFormat = formatter;
            package.Manifest.Save();

            return package;
        }

        /// <summary>
        /// Opens an existing package.
        /// </summary>
        /// <param name="filePath">The path to the package file.</param>
        /// <param name="manifest">Optional manifest to load with the package.</param>
        /// <returns>The opened <see cref="ProjectedPackage"/>.</returns>
        public static ProjectedPackage Open(string filePath, PackageManifest? manifest = null)
        {
            var package = new ProjectedPackage(filePath, manifest);
            package.Open();
            return package;
        }

        /// <summary>
        /// Opens an existing package using a factory method for custom types.
        /// </summary>
        public static TPackage Open<TPackage, TManifest>(
            string filePath,
            Func<string, TManifest?, TPackage> createInstance,
            TManifest? manifest = null)
            where TPackage : ProjectedPackage
            where TManifest : PackageManifest
        {
            var package = createInstance(filePath, manifest);
            if (package == null)
                throw new InvalidOperationException("createInstance returned null");
            package.Open();
            return package;
        }

        /// <summary>
        /// Creates a new package with the specified manifest document type.
        /// </summary>
        public static ProjectedPackage Create(string filePath, PackageManifest? manifest = null, PackageManifestDocumentType type = PackageManifestDocumentType.Json)
        {
            return Create(filePath, manifest, TypeToFormatter(type));
        }

        /// <summary>
        /// Creates a new package using a factory method and specified manifest document type.
        /// </summary>
        public static TPackage Create<TPackage, TManifest>(
           string filePath,
           Func<string, TManifest?, TPackage> createInstance,
           TManifest? manifest = null,
           PackageManifestDocumentType type = PackageManifestDocumentType.Json)
           where TPackage : ProjectedPackage
           where TManifest : PackageManifest
        {
            return Create<TPackage, TManifest>(filePath, createInstance, manifest, TypeToFormatter(type));
        }

        private static IDocumentFormatter TypeToFormatter(PackageManifestDocumentType type)
        {
            return type switch
            {
                PackageManifestDocumentType.Json => new JsonDocumentFormatter(),
                PackageManifestDocumentType.XML => new XmlDocumentFormatter(),
                _ => throw new NotSupportedException("Unknown document type is not supported")
            };
        }
    }
}
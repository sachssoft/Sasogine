using Sachssoft.Inspection;
using Sachssoft.Sasogine.Assets;
using System;
using System.IO;
using System.IO.Compression;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class PackageAssetSource : IAssetSource, IHasGuid
    {
        private readonly PackageBase _package;
        private AssetCategory _category = AssetCategory.Other;
        private string _categoryName = AssetCategory.Other.ToString();
        private string? _id;
        private IAsset? _asset;

        public event EventHandler? IDChanged;
        public event EventHandler? AssetChanged;

        internal PackageAssetSource(PackageBase package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public Guid Guid { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Eindeutige ID des Assets (kann vom Inspector oder Referenzen genutzt werden)
        /// </summary>
        public string? ID
        {
            get => _id;
            set
            {
                if (string.Equals(_id, value, StringComparison.Ordinal))
                    return;

                _id = value;
                IDChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Optionaler Type Key für TypedFactoryManager
        /// </summary>
        public string? TypeName { get; set; }

        public IAsset? Asset
        {
            get => _asset;
            internal set
            {
                if (_asset != value)
                {
                    _asset = value;
                    AssetChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Vollständiger Pfad im Package, z. B. "assets/textures/foo.png"
        /// </summary>
        public string FilePath => ProjectedPackageAssetCollection.FILE_PATH + FileName;

        /// <summary>
        /// Kategorie des Assets (enum)
        /// </summary>
        public AssetCategory Category
        {
            get => _category;
            set
            {
                if (_category == value)
                    return;

                _category = value;
                _categoryName = _category.ToString();
            }
        }

        /// <summary>
        /// Kategorie als String (synchronisiert mit <see cref="Category"/>)
        /// </summary>
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (string.Equals(_categoryName, value, StringComparison.OrdinalIgnoreCase))
                    return;

                _categoryName = value;

                if (!Enum.TryParse<AssetCategory>(_categoryName, true, out var cat))
                    cat = AssetCategory.Other;

                _category = cat;
            }
        }

        /// <summary>
        /// Größe des Assets in Bytes
        /// </summary>
        public long Size { get; internal set; }

        public string? Hash { get; set; }

        /// <summary>
        /// Öffnet einen Stream zum Asset im Package
        /// </summary>
        public Stream Open()
        {
            _package.ThrowIfNotOpened();
            return EnsureGetEntry().Open();
        }

        /// <summary>
        /// Löscht das Asset sowohl im Package als auch im Manifest
        /// </summary>
        public void Delete()
        {
            _package.ThrowIfNotOpened();

            var entry = EnsureGetEntry();
            entry.Delete();

            _package.Manifest._assetEntries.Remove(FileName);
            _package.Manifest.Save();
        }

        /// <summary>
        /// Ersetzt den Inhalt des Assets mit einem neuen Stream
        /// </summary>
        public void Replace(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            _package.ThrowIfNotOpened();

            var oldEntry = _package.Source.GetEntry(FilePath);
            oldEntry?.Delete();

            var newEntry = _package.Source.CreateEntry(FilePath);
            using var entryStream = newEntry.Open();

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            stream.CopyTo(entryStream);
            Size = entryStream.Length;

            _package.Manifest._assetEntries[FileName] = this;
            _package.Manifest.Save();
        }

        /// <summary>
        /// Stellt sicher, dass der Asset-Eintrag existiert und liefert ihn zurück
        /// </summary>
        internal ZipArchiveEntry EnsureGetEntry()
        {
            var entry = _package.Source?.GetEntry(FilePath)
                        ?? throw new FileNotFoundException($"Asset '{FileName}' not found in package.");

            return entry;
        }

        public override string ToString() => $"{FileName} ({CategoryName})";
    }
}

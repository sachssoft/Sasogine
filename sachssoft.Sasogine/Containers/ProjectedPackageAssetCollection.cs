using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class ProjectedPackageAssetCollection
    {
        internal const string FILE_PATH = "assets/";

        private readonly PackageBase _package;

        public PackageAssetEntry? this[string fileName] => FindEntry(fileName);

        internal ProjectedPackageAssetCollection(PackageBase package)
        {
            package.ThrowIfIsReadOnly();
            _package = package;
        }

        public void Add(byte[] data, string filePath, AssetCategory category)
        {
            using var ms = new MemoryStream(data);
            DoAdd(ms, filePath, category, saveManifest: true);
        }

        public void Add(string content, string filePath, AssetCategory category)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(content);
            Add(data, filePath, category);
        }

        public void Add(Stream stream, string filePath, AssetCategory category)
        {
            DoAdd(stream, filePath, category, saveManifest: true);
        }

        private void DoAdd(Stream stream, string filePath, AssetCategory category, bool saveManifest)
        {
            _package.ThrowIfNotOpened();

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File name must be specified", nameof(filePath));

            string path = FILE_PATH + filePath;

            if (_package.IsFileExists(path))
                throw new InvalidOperationException($"Asset '{filePath}' already exists.");

            var entry = _package.Source.CreateEntry(path);
            using var entryStream = entry.Open();

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            stream.CopyTo(entryStream);

            var newEntry = new PackageAssetEntry(_package)
            {
                FileName = filePath,
                Category = category,
                Size = entryStream.Length
            };

            _package.Manifest._assets[filePath] = newEntry;

            if (saveManifest)
                _package.Manifest.Save();
        }

        public void AddRange(
            IEnumerable<(Stream Stream, string FilePath, AssetCategory Category)> assets,
            bool overwrite = false)
        {
            // 1. Prüfen
            foreach (var asset in assets)
            {
                if (Contains(asset.FilePath) && !overwrite)
                    throw new InvalidOperationException($"Asset '{asset.FilePath}' already exists.");
            }

            // 2. Hinzufügen / Ersetzen
            foreach (var asset in assets)
                AddOrReplace(asset.Stream, asset.FilePath, asset.Category, overwrite);

            _package.Manifest.Save();
        }

        public void AddRange(
            IEnumerable<(string OriginalSourceFilePath, string FilePath, AssetCategory Category)> assets,
            bool overwrite = false)
        {
            // 1. Prüfen
            foreach (var asset in assets)
            {
                if (Contains(asset.FilePath) && !overwrite)
                    throw new InvalidOperationException($"Asset '{asset.FilePath}' already exists.");
            }

            // 2. Hinzufügen / Ersetzen
            foreach (var asset in assets)
            {
                using var fs = File.OpenRead(asset.OriginalSourceFilePath);
                AddOrReplace(fs, asset.FilePath, asset.Category, overwrite);
            }

            _package.Manifest.Save();
        }

        // private Hilfsmethode
        private void AddOrReplace(Stream stream, string filePath, AssetCategory category, bool overwrite)
        {
            if (Contains(filePath))
            {
                AddOrReplace(stream, filePath, category); // überschreiben
            }
            else
            {
                DoAdd(stream, filePath, category, saveManifest: false);
            }
        }

        public void RemoveRange(params string[] filePaths)
            => RemoveRange(false, filePaths);

        public void RemoveRange(bool ignoreMissing, params string[] filePaths)
        {
            if (!ignoreMissing)
            {
                // 1. Prüfen, ob alle existieren
                foreach (var filePath in filePaths)
                {
                    if (!Contains(filePath))
                        throw new FileNotFoundException($"Asset '{filePath}' does not exist.");
                }
            }

            // 2. Löschen
            foreach (var filePath in filePaths)
            {
                var entry = FindEntry(filePath);
                if (entry != null)
                    entry.Delete();
            }

            _package.Manifest.Save();
        }

        public void AddOrReplace(Stream stream, string assetFilePath, AssetCategory category)
        {
            _package.ThrowIfNotOpened();
            _package.ThrowIfIsReadOnly();


            // 1. Prüft, ob es in der Liste der Assets
            var assetEntry = FindEntry(assetFilePath);
            assetEntry?.Delete();

            // 2. Prüft, ob es in der Dateiliste
            var entry = _package.Source.GetEntry(FILE_PATH + assetFilePath);
            entry?.Delete();

            // Neu hinzufügen
            Add(stream, assetFilePath, category);
        }

        public void AddOrReplace(string originalFilePath, string assetFilePath, AssetCategory category)
        {
            using var fs = File.OpenRead(originalFilePath);
            AddOrReplace(fs, assetFilePath, category);
        }

        public void AddOrReplace(Stream stream, string assetFilePath, string category)
        {
            if (!Enum.TryParse<AssetCategory>(category, true, out var cat))
                cat = AssetCategory.Other;

            AddOrReplace(stream, assetFilePath, cat);
        }

        public bool Contains(string fileName)
        {
            return _package.IsFileExists(FILE_PATH + fileName);
        }

        // Prüft, ob ein Asset im Manifest steht
        public bool ContainsInManifest(string fileName)
        {
            return _package.Manifest._assets.ContainsKey(fileName);
        }

        // Synchronisiert zwischen Manifest und Assets
        // Fügt Assets hinzu, die im Package sind, aber nicht im Manifest
        // Entfernt Manifest-Einträge, deren Dateien nicht mehr existieren
        public void SynchronizeToManifest()
        {
            _package.ThrowIfNotOpened();

            // 1. Neue Dateien im Package, die noch nicht im Manifest sind, hinzufügen
            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string fileName = entry.FullName.Substring(FILE_PATH.Length);

                if (!_package.Manifest._assets.ContainsKey(fileName))
                {
                    var newEntry = new PackageAssetEntry(_package)
                    {
                        FileName = fileName,
                        Category = InferCategoryFromPath(fileName),
                        Size = entry.Length
                    };
                    _package.Manifest._assets[fileName] = newEntry;
                }
            }

            // 2. Manifest-Einträge entfernen, deren Dateien nicht mehr existieren
            var toRemove = new List<string>();
            foreach (var kvp in _package.Manifest._assets)
            {
                string path = FILE_PATH + kvp.Key;
                if (!_package.IsFileExists(path))
                    toRemove.Add(kvp.Key);
            }
            foreach (var key in toRemove)
                _package.Manifest._assets.Remove(key);

            UpdateManifest();
        }

        // Löscht alle Asset-Dateien im Package, die nicht im Manifest stehen
        public void DeleteAssetsIfNotInManifest()
        {
            _package.ThrowIfNotOpened();

            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string fileName = entry.FullName.Substring(FILE_PATH.Length);
                if (!_package.Manifest._assets.ContainsKey(fileName))
                {
                    entry.Delete();
                }
            }
            // Optional: Manifest aktualisieren, falls nötig
        }

        public PackageAssetEntry GetEntry(string fileName) =>
            _package.Manifest._assets.TryGetValue(fileName, out var entry)
                ? entry
                : throw new InvalidOperationException($"Asset '{fileName}' does not exist.");

        public PackageAssetEntry? FindEntry(string filePath) =>
            _package.Manifest._assets.TryGetValue(filePath, out var entry) ? entry : null;

        public IEnumerable<PackageAssetEntry> GetAll() => _package.Manifest._assets.Values;

        public IEnumerable<PackageAssetEntry> FindAll(AssetCategory category)
            => _package.Manifest._assets.Values
                .Where(x => x.Category == category)
                .ToList();

        public IEnumerable<PackageAssetEntry> FindAll(string categoryName)
            => _package.Manifest._assets.Values
                .Where(x => x.CategoryName == categoryName)
                .ToList();

        public IEnumerable<PackageAssetEntry> FindAllByExtension(string fileExtension)
        {
            if (!fileExtension.StartsWith("."))
                fileExtension = "." + fileExtension;

            return _package.Manifest._assets
                .Values
                .Where(x => x.FileName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IEnumerable<PackageAssetEntry> FindAllByFilename(string fileName)
        {
            return _package.Manifest._assets
                .Values
                .Where(x => string.Equals(x.FileName, fileName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public void UpdateManifest()
        {
            _package.Manifest.Save();
        }

        // Erstelle ein veröffentliches Package
        public PublishedPackage Publish(PublishOptions? options = null)
        {
            options ??= new PublishOptions(); // Standardoptionen

            //_package.ThrowIfNotOpened();

            //// Temporärer Stream für das Paket
            //using var ms = new MemoryStream();

            //// 1. Optional: Assets komprimieren
            //Stream targetStream = ms;
            //if (options.Compress)
            //{
            //    targetStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true);
            //}

            //// 2. Alle Assets ins Stream schreiben
            //foreach (var asset in GetAll())
            //{
            //    using var assetStream = _package.Source.GetEntry(ProjectedPackageAssetCollection.FILE_PATH + asset.FileName)?.Open();
            //    if (assetStream == null) continue;

            //    // Metadaten + Inhalt schreiben
            //    WriteAssetToStream(asset, assetStream, targetStream);
            //}

            //// 3. Verschlüsselung optional
            //if (options.Encrypt)
            //{
            //    if (string.IsNullOrEmpty(options.Password))
            //        throw new InvalidOperationException("Password must be provided for encryption.");

            //    ms.Position = 0;
            //    var encryptedStream = new MemoryStream();
            //    EncryptStream(ms, encryptedStream, options.Password);
            //    ms.SetLength(0);
            //    encryptedStream.Position = 0;
            //    encryptedStream.CopyTo(ms);
            //}

            //// 4. In Datei schreiben
            //using var fs = File.OpenWrite(options.OutputFilePath);
            //ms.Position = 0;
            //ms.CopyTo(fs);

            return new PublishedPackage(options.OutputFilePath);
        }


        private AssetCategory InferCategoryFromPath(string fileName)
        {
            if (fileName.StartsWith("texture/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Texture;
            if (fileName.StartsWith("models/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Model;
            if (fileName.StartsWith("audio/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Audio;
            if (fileName.StartsWith("shaders/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Shader;
            if (fileName.StartsWith("fonts/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Font;
            if (fileName.StartsWith("videos/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Video;
            if (fileName.StartsWith("scripts/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Script;
            if (fileName.StartsWith("config/", StringComparison.InvariantCultureIgnoreCase))
                return AssetCategory.Config;

            // Standard-Kategorie falls nicht erkannt
            return AssetCategory.Other;
        }
    }
}

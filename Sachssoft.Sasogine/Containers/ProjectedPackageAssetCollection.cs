using Sachssoft.Inspection;
using Sachssoft.Sasogine.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class ProjectedPackageAssetCollection : IAssetSourceCollection
    {
        internal const string FILE_PATH = "assets/";

        private readonly PackageBase _package;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public PackageAssetSource? this[string fileName] => FindEntry(fileName);

        internal ProjectedPackageAssetCollection(PackageBase package)
        {
            package.ThrowIfIsReadOnly();
            _package = package;
        }

        public int Count => _package.Manifest._assetEntries.Count;

        // --- Add Methoden ---
        public void Add(Stream stream, string filePath, AssetCategory category = AssetCategory.Other)
        {
            var entry = DoAdd(stream, filePath, category, overwrite: false);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entry));
            OnPropertyChanged(nameof(Count));
        }

        public void AddOrReplace(Stream stream, string filePath, AssetCategory category = AssetCategory.Other)
        {
            var existing = FindEntry(filePath);
            if (existing != null)
            {
                Remove(filePath); // feuert Events intern
            }

            Add(stream, filePath, category);
        }

        public void AddFromFile(string sourceFilePath, string filePath, AssetCategory category = AssetCategory.Other)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("EntrySource file does not exist.", sourceFilePath);

            using var fs = File.OpenRead(sourceFilePath);
            Add(fs, filePath, category);
        }

        public void Add(byte[] data, string filePath, AssetCategory category = AssetCategory.Other)
        {
            using var ms = new MemoryStream(data);
            Add(ms, filePath, category);
        }

        public void Add(string content, string filePath, AssetCategory category = AssetCategory.Other)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(content);
            Add(data, filePath, category);
        }

        public void AddRange(IEnumerable<(Stream Stream, string FilePath, AssetCategory Category)> assets, bool overwrite = false)
        {
            foreach (var asset in assets)
            {
                if (Contains(asset.FilePath) && !overwrite)
                    throw new InvalidOperationException($"Asset '{asset.FilePath}' already exists.");

                if (overwrite)
                    AddOrReplace(asset.Stream, asset.FilePath, asset.Category);
                else
                    Add(asset.Stream, asset.FilePath, asset.Category);
            }
        }

        // --- Remove Methoden ---
        public void Remove(string filePath)
        {
            var entry = FindEntry(filePath);
            if (entry == null) return;

            entry.Delete();
            _package.Manifest._assetEntries.Remove(filePath);
            _package.Manifest.Save();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entry));
            OnPropertyChanged(nameof(Count));
        }

        public void RemoveRange(params string[] filePaths)
        {
            foreach (var file in filePaths)
            {
                Remove(file); // feuert Events intern
            }
        }

        // --- Synchronize ---
        public void Synchronize(IEnumerable<string> fileNames)
        {
            _package.ThrowIfNotOpened();

            foreach (var fileName in fileNames)
            {
                string path = FILE_PATH + fileName;
                if (!_package.IsFileExists(path)) continue;

                if (!ContainsInManifest(fileName))
                {
                    var entry = _package.Source.GetEntry(path);
                    var newEntry = new PackageAssetSource(_package)
                    {
                        FileName = fileName,
                        Category = InferCategoryFromPath(fileName),
                        Size = entry?.Length ?? 0
                    };
                    _package.Manifest._assetEntries[fileName] = newEntry;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newEntry));
                    OnPropertyChanged(nameof(Count));
                }
            }

            UpdateManifest();
        }

        public void SynchronizeAll()
        {
            _package.ThrowIfNotOpened();

            // Neue Dateien hinzufügen
            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string fileName = entry.FullName.Substring(FILE_PATH.Length);

                if (!_package.Manifest._assetEntries.ContainsKey(fileName))
                {
                    var newEntry = new PackageAssetSource(_package)
                    {
                        FileName = fileName,
                        Category = InferCategoryFromPath(fileName),
                        Size = entry.Length
                    };
                    _package.Manifest._assetEntries[fileName] = newEntry;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newEntry));
                    OnPropertyChanged(nameof(Count));
                }
            }

            // Entfernte Manifest-Einträge löschen
            var toRemove = _package.Manifest._assetEntries.Keys
                .Where(k => !_package.IsFileExists(FILE_PATH + k))
                .ToList();

            foreach (var key in toRemove)
            {
                var entry = _package.Manifest._assetEntries[key];
                _package.Manifest._assetEntries.Remove(key);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entry));
                OnPropertyChanged(nameof(Count));
            }

            UpdateManifest();
        }

        public IEnumerable<string> GetUnregisteredAssets()
        {
            _package.ThrowIfNotOpened();

            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // Nur echte Dateien (kein Verzeichnis)
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                string assetName = entry.FullName.Substring(FILE_PATH.Length);

                // Prüfen, ob Asset bereits im Manifest registriert ist
                if (!_package.Manifest._assetEntries.ContainsKey(assetName))
                    yield return assetName;
            }
        }

        // --- Hilfsmethoden ---
        public bool Contains(string fileName) => _package.IsFileExists(FILE_PATH + fileName);
        public bool ContainsInManifest(string fileName) => _package.Manifest._assetEntries.ContainsKey(fileName);

        public PackageAssetSource? FindEntry(string fileName)
        {
            return _package.Manifest._assetEntries.TryGetValue(fileName, out var entry)
                ? (PackageAssetSource)entry
                : null;
        }

        public IEnumerable<PackageAssetSource> GetAll() => _package.Manifest._assetEntries.Values.Cast<PackageAssetSource>();

        public IAssetCollection FindAssets<TAssetType>() where TAssetType : IAsset
        {
            return new AssetSourceView(this, typeof(TAssetType));
        }

        public IAssetCollection FindAssets(Type? type = null)
        {
            return new AssetSourceView(this, type);
        }

        private PackageAssetSource DoAdd(Stream stream, string filePath, AssetCategory category, bool overwrite)
        {
            _package.ThrowIfNotOpened();

            string path = FILE_PATH + filePath;
            if (_package.IsFileExists(path) && !overwrite)
                throw new InvalidOperationException($"Asset '{filePath}' already exists.");

            var entry = _package.Source.CreateEntry(path);
            using var entryStream = entry.Open();
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(entryStream);

            var newEntry = new PackageAssetSource(_package)
            {
                FileName = filePath,
                Category = category,
                Size = entryStream.Length
            };

            _package.Manifest._assetEntries[filePath] = newEntry;
            _package.Manifest.Save();

            return newEntry;
        }

        private AssetCategory InferCategoryFromPath(string fileName)
        {
            if (fileName.StartsWith("texture/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Texture;
            if (fileName.StartsWith("models/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Model;
            if (fileName.StartsWith("audio/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Audio;
            if (fileName.StartsWith("shaders/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Shader;
            if (fileName.StartsWith("fonts/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Font;
            if (fileName.StartsWith("videos/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Video;
            if (fileName.StartsWith("scripts/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Script;
            if (fileName.StartsWith("config/", StringComparison.InvariantCultureIgnoreCase)) return AssetCategory.Config;
            return AssetCategory.Other;
        }

        private void UpdateManifest() => _package.Manifest.Save();

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // --- IEnumerable Implementation ---
        public IEnumerator<IAssetSource> GetEnumerator() => _package.Manifest._assetEntries.Values.Cast<IAssetSource>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

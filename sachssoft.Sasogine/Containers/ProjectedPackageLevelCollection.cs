using Sachssoft.Sasogine.Elements;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class ProjectedPackageLevelCollection
    {
        internal const string FILE_PATH = "levels/";

        private readonly PackageBase _package;
        private readonly ReadOnlyObservableCollection<PackageLevelBase> _readOnlyEntries;

        public ReadOnlyObservableCollection<PackageLevelBase> Entries => _readOnlyEntries;

        public PackageLevelBase this[int index] => _readOnlyEntries[index];

        internal ProjectedPackageLevelCollection(PackageBase package)
        {
            package.ThrowIfIsReadOnly();
            _package = package;
            _readOnlyEntries = new ReadOnlyObservableCollection<PackageLevelBase>(_package.Manifest._levels);
        }

        public void Add(PackageLevelBase level)
        {
            AddOrReplace(level, overwrite: false);
        }

        public void AddOrReplace(PackageLevelBase level, bool overwrite)
        {
            _package.ThrowIfNotOpened();

            string filePath = level.FilePath ?? throw new ArgumentException("FilePath must be set.", nameof(level));
            string fullPath = FILE_PATH + filePath;

            if (_package.IsFileExists(fullPath) && !overwrite)
                throw new InvalidOperationException($"Level '{filePath}' already exists.");

            using var sourceStream = level.Open();
            using var entryStream = _package.Source.CreateEntry(fullPath).Open();
            sourceStream.CopyTo(entryStream);

            // Existierendes Level entfernen
            var existing = _package.Manifest._levels.FirstOrDefault(l => l.FilePath == filePath);
            if (existing != null)
                _package.Manifest._levels.Remove(existing);

            _package.Manifest._levels.Add(level);

            UpdateManifest();
        }

        public void AddRange(params PackageLevelBase[] levels)
        {
            foreach (var level in levels)
            {
                if (level.FilePath == null)
                    throw new ArgumentException("FilePath must be set.", nameof(level));

                if (Contains(level.FilePath))
                    throw new InvalidOperationException($"Level '{level.FilePath}' already exists.");
            }

            foreach (var level in levels)
                Add(level);

            UpdateManifest();
        }

        public bool Contains(string fileName) =>
            _package.Manifest._levels.Any(l => l.FilePath == fileName);

        public PackageLevelBase GetEntry(string fileName) =>
            _package.Manifest._levels.FirstOrDefault(l => l.FilePath == fileName)
                ?? throw new InvalidOperationException($"Level '{fileName}' does not exist.");

        public PackageLevelBase? FindEntry(string fileName) =>
            _package.Manifest._levels.FirstOrDefault(l => l.FilePath == fileName);

        public void SynchronizeToManifest(Func<PackageBase, PackageLevelBase> createInstanceFunc)
        {
            _package.ThrowIfNotOpened();

            // Neue Level im Package hinzufügen
            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string fileName = entry.FullName.Substring(FILE_PATH.Length);

                if (!Contains(fileName))
                {
                    var newLevel = createInstanceFunc(_package);
                    newLevel.FilePath = fileName;

                    using var stream = entry.Open();
                    using var tempStream = new MemoryStream();
                    stream.CopyTo(tempStream);
                    tempStream.Position = 0;

                    newLevel.Save(tempStream); // Stream temporär speichern

                    _package.Manifest._levels.Add(newLevel);
                }
            }

            // Lösche Level aus Manifest, die nicht mehr existieren
            var toRemove = _package.Manifest._levels
                .Where(l => !_package.IsFileExists(FILE_PATH + l.FilePath))
                .ToList();

            foreach (var level in toRemove)
                _package.Manifest._levels.Remove(level);

            UpdateManifest();
        }

        public void DeleteLevelsIfNotInManifest()
        {
            _package.ThrowIfNotOpened();

            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_PATH, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                string fileName = entry.FullName.Substring(FILE_PATH.Length);
                if (!Contains(fileName))
                    entry.Delete();
            }
        }

        private void UpdateManifest()
        {
            _package.Manifest.Save();
        }
    }
}

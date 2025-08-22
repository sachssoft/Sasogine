using Sachssoft.Sasogine.Elements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sachssoft.Sasogine.Containers
{
    public sealed class ProjectedPackageLevelCollection : IEnumerable<PackageLevelBase>
    {
        internal const string FILE_PATH = "levels/";

        private readonly PackageBase _package;
        private readonly ReadOnlyObservableCollection<PackageLevelBase> _readOnlyEntries;

        public ReadOnlyObservableCollection<PackageLevelBase> Entries => _readOnlyEntries;

        public IEnumerable<PackageLevelBase> SortedEntries => _readOnlyEntries.OrderBy(x => x.Index);

        public PackageLevelBase this[int index] => _readOnlyEntries[index];

        internal ProjectedPackageLevelCollection(PackageBase package)
        {
            package.ThrowIfIsReadOnly();
            _package = package;
            _readOnlyEntries = new ReadOnlyObservableCollection<PackageLevelBase>(_package.Manifest._levels);
        }

        public void Add(PackageLevelBase level, bool create = false)
        {
            AddOrReplace(level, create ? PackageEntryFlags.CreateIfNotExists : PackageEntryFlags.None);
        }

        public void AddOrReplace(PackageLevelBase level, PackageEntryFlags flags = PackageEntryFlags.None)
        {
            if (level == null)
                throw new ArgumentNullException(nameof(level));

            _package.ThrowIfNotOpened();

            if (string.IsNullOrWhiteSpace(level.RelativeFilePath))
                throw new ArgumentException("RelativeFilePath must be set.", nameof(level));

            string fullPath = Path.Combine(FILE_PATH, level.RelativeFilePath);

            bool overwrite = (flags & PackageEntryFlags.OverwriteExisting) != 0;
            bool create = (flags & PackageEntryFlags.CreateIfNotExists) != 0;

            if (_package.IsFileExists(fullPath) && !overwrite)
                throw new InvalidOperationException($"Level '{level.RelativeFilePath}' already exists.");

            if (create)
                level.Create();

            // Es kann auch passieren, dass trotz Erstellen keine Datei angelegt wurde.
            if (!level.IsExists && ((flags & PackageEntryFlags.CreateEmptyEntryIfMissing) != 0))
            {
                // Lege eine leere Datei
                level.Write(new MemoryStream());
            }

            // Existierendes Level entfernen
            var existing = _package.Manifest._levels.FirstOrDefault(l => l.RelativeFilePath == level.RelativeFilePath);
            if (existing != null)
                _package.Manifest._levels.Remove(existing);

            level.Index = _package.Manifest._levels.Count;
            _package.Manifest._levels.Add(level);

            level.Save();
            UpdateManifest();
        }

        public void AddRange(params PackageLevelBase[] levels)
        {
            foreach (var level in levels)
            {
                if (level.RelativeFilePath == null)
                    throw new ArgumentException("RelativeFilePath must be set.", nameof(level));

                if (Contains(level.RelativeFilePath))
                    throw new InvalidOperationException($"Level '{level.RelativeFilePath}' already exists.");
            }

            foreach (var level in levels)
                Add(level);

            UpdateManifest();
        }

        public bool Contains(string fileName) =>
            _package.Manifest._levels.Any(l => l.RelativeFilePath == fileName);

        public void MoveTo(string oldFileName, string newFileName)
        {
            string oldFullFilePath = FILE_PATH + oldFileName;
            string newFullFilePath = FILE_PATH + newFileName;
            _package.MoveFileTo(oldFullFilePath, newFullFilePath);

            var level = GetEntry(newFileName);
            level.RelativeFilePath = newFileName;
        }

        public PackageLevelBase GetEntry(string fileName) =>
            _package.Manifest._levels.FirstOrDefault(l => l.RelativeFilePath == fileName)
                ?? throw new InvalidOperationException($"Level '{fileName}' does not exist.");

        public PackageLevelBase? FindEntry(string fileName) =>
            _package.Manifest._levels.FirstOrDefault(l => l.RelativeFilePath == fileName);

        public bool IsEntryExists(string fileName) =>
            _package.Manifest._levels.Where(x => x.RelativeFilePath.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).Any();

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
                    newLevel.RelativeFilePath = fileName;

                    //using var stream = entry.Open();
                    //using var tempStream = new MemoryStream();
                    //stream.CopyTo(tempStream);
                    //tempStream.Position = 0;

                    //newLevel.Save(tempStream); // Stream temporär speichern

                    _package.Manifest._levels.Add(newLevel);
                }
            }

            // Lösche Level aus Manifest, die nicht mehr existieren
            var toRemove = _package.Manifest._levels
                .Where(l => !_package.IsFileExists(FILE_PATH + l.RelativeFilePath))
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
            var sortedLevelList = _package.Manifest._levels.OrderBy(l => l.Index)
                                                           .ToArray();

            for (int i = 0; i < sortedLevelList.Length; i++)
            {
                var level = sortedLevelList[i];
                level.Index = i;
            }

            _package.Manifest.Save();
        }

        IEnumerator<PackageLevelBase> IEnumerable<PackageLevelBase>.GetEnumerator()
            => _readOnlyEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _readOnlyEntries.GetEnumerator();
    }
}

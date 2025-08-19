using Sachssoft.Sasogine.Elements;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Containers
{
    public class PackageLevelBase : GameObject
    {
        private readonly PackageBase _package;

        public PackageLevelBase(PackageBase package, string filePath)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public int Index { get; internal set; }

        public string FilePath { get; internal set; }

        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Stream für das Level öffnen.
        /// </summary>
        public Stream Open()
        {
            string fullPath = ProjectedPackageLevelCollection.FILE_PATH + FilePath;
            if (!_package.IsFileExists(fullPath))
                throw new FileNotFoundException($"Level '{FilePath}' existiert nicht.", fullPath);

            var entry = _package.Source.GetEntry(fullPath);
            return entry.Open();
        }

        /// <summary>
        /// Level speichern.
        /// </summary>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            string fullPath = ProjectedPackageLevelCollection.FILE_PATH + FilePath;
            var entry = _package.Source.CreateEntry(fullPath);
            using var entryStream = entry.Open();
            if (stream.CanSeek)
                stream.Position = 0;
            stream.CopyTo(entryStream);
        }

        /// <summary>
        /// Level löschen.
        /// </summary>
        public void Delete()
        {
            string fullPath = ProjectedPackageLevelCollection.FILE_PATH + FilePath;
            if (_package.IsFileExists(fullPath))
            {
                var entry = _package.Source.GetEntry(fullPath);
                entry.Delete();
            }
        }

        /// <summary>
        /// Pfad ändern (nur innerhalb des Packages erlaubt).
        /// </summary>
        public void ChangeFilePath(string newFilePath)
        {
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentException("Pfad darf nicht leer sein.", nameof(newFilePath));

            FilePath = newFilePath;
        }
    }
}

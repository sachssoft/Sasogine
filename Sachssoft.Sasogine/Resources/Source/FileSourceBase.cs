using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Source
{
    /// <summary>
    /// Base class for sources that store data in the local file system.
    /// Implements synchronous and asynchronous loading and saving.
    /// </summary>
    // Basisklasse für Quellen, die Daten im lokalen Dateisystem speichern.
    // Unterstützt synchrones und asynchrones Laden und Speichern.
    public abstract class FileSourceBase : SourceBase
    {
        /// <summary>
        /// Full file path in the local file system.
        /// </summary>
        // Vollständiger Dateipfad im lokalen Dateisystem.
        public string FilePath { get; }

        protected FileSourceBase(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        // ---- LoaderBase Implementation ----

        protected override Stream OpenStream()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("File not found", FilePath);

            return File.OpenRead(FilePath);
        }

        protected override async Task<Stream> OpenStreamAsync()
        {
            if (!File.Exists(FilePath))
                throw new FileNotFoundException("File not found", FilePath);

            var ms = new MemoryStream();
            using (var fs = File.OpenRead(FilePath))
            {
                await fs.CopyToAsync(ms).ConfigureAwait(false);
            }
            ms.Position = 0;
            return ms;
        }

        // ---- SourceBase Implementation ----

        protected override void SaveStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var fs = File.Create(FilePath);
            stream.CopyTo(fs);
        }

        protected override async Task SaveStreamAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var fs = File.Create(FilePath);
            await stream.CopyToAsync(fs).ConfigureAwait(false);
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Source
{
    /// <summary>
    /// Source that stores data entirely in memory.
    /// Supports synchronous and asynchronous reading and writing.
    /// </summary>
    // Quelle, die ihre Daten vollständig im Arbeitsspeicher hält.
    // Unterstützt synchrones und asynchrones Lesen und Schreiben.
    public class MemorySource : SourceBase
    {
        private MemoryStream _memoryStream;

        /// <summary>
        /// Creates a MemorySource initialized with optional data.
        /// </summary>
        // Erstellt eine MemorySource, optional mit initialen Daten.
        public MemorySource(byte[]? initialData = null)
        {
            _memoryStream = initialData != null
                ? new MemoryStream(initialData, writable: true)
                : new MemoryStream();
        }

        // ---- LoaderBase Implementation ----

        protected override Stream OpenStream()
        {
            // Liefert eine Kopie des internen MemoryStreams
            return new MemoryStream(_memoryStream.ToArray(), writable: true);
        }

        protected override Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult<Stream>(new MemoryStream(_memoryStream.ToArray(), writable: true));
        }

        // ---- SourceBase Implementation ----

        protected override void SaveStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // Alte Daten überschreiben
            _memoryStream = new MemoryStream();
            stream.CopyTo(_memoryStream);
            _memoryStream.Position = 0;
        }

        protected override async Task SaveStreamAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            _memoryStream = new MemoryStream();
            await stream.CopyToAsync(_memoryStream).ConfigureAwait(false);
            _memoryStream.Position = 0;
        }
    }
}
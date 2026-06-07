using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    /// <summary>
    /// Loader für Daten, die im Speicher liegen (byte[], MemoryStream, AudioStream etc.).
    /// </summary>
    public sealed class MemorySource : ResourceSourceBase
    {
        /// <summary>
        /// Speicherinhalt des Loaders. Muss vor dem Zugriff gesetzt werden.
        /// </summary>
        public byte[]? Data { get; set; }

        public MemorySource() { }

        public MemorySource(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        protected override Stream OpenStream()
        {
            return new MemoryStream(Data!, writable: false);
        }

        protected override Task<Stream> OpenStreamAsync()
        {
            // Bereits im Speicher – Task.FromResult reicht
            return Task.FromResult<Stream>(new MemoryStream(Data!, writable: false));
        }
    }
}

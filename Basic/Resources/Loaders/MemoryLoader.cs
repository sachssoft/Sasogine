using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    /// <summary>
    /// Loader für Daten, die im Speicher liegen (byte[], MemoryStream, AudioStream etc.).
    /// </summary>
    public sealed class MemoryLoader : LoaderBase
    {
        /// <summary>
        /// Speicherinhalt des Loaders. Muss vor dem Zugriff gesetzt werden.
        /// </summary>
        public byte[]? Data { get; set; }

        public MemoryLoader() { }

        public MemoryLoader(byte[] data)
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

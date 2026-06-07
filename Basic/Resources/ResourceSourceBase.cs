using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Base class for resources that can be loaded.
    /// Provides synchronous and asynchronous access to streams and raw byte arrays.
    /// </summary>
    // Basisklasse für Ressourcen, die geladen werden können.
    // Stellt synchronen und asynchronen Zugriff auf Streams und Rohdaten (byte[]) bereit.
    public abstract class ResourceSourceBase
    {
        /// <summary>
        /// Opens a stream synchronously for reading the resource.
        /// </summary>
        // Öffnet einen Stream synchron zum Lesen der Ressource.
        protected abstract Stream OpenStream();

        /// <summary>
        /// Opens a stream asynchronously for reading the resource.
        /// </summary>
        // Öffnet einen Stream asynchron zum Lesen der Ressource.
        protected abstract Task<Stream> OpenStreamAsync();

        /// <summary>
        /// Returns a stream synchronously or throws if not available.
        /// </summary>
        // Liefert einen Stream synchron oder wirft, wenn er nicht verfügbar ist.
        public Stream GetStream()
        {
            return OpenStream();
        }

        /// <summary>
        /// Returns a stream asynchronously or throws if not available.
        /// </summary>
        // Liefert einen Stream asynchron oder wirft, wenn er nicht verfügbar ist.
        public async Task<Stream> GetStreamAsync()
        {
            return await OpenStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the raw bytes of the resource synchronously.
        /// </summary>
        // Liest die Rohdaten der Ressource als byte[] synchron.
        public byte[] LoadRaw()
        {
            using var s = GetStream();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Loads the raw bytes of the resource asynchronously.
        /// </summary>
        // Liest die Rohdaten der Ressource als byte[] asynchron.
        public async Task<byte[]> LoadRawAsync()
        {
            using var s = await GetStreamAsync().ConfigureAwait(false);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }
    }
}
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Provides a base implementation for accessing resource data.
    /// </summary>
    public abstract class ResourceSourceBase
    {
        /// <summary>
        /// Opens a stream for reading the resource.
        /// </summary>
        /// <returns>
        /// A stream containing the resource data.
        /// </returns>
        protected abstract Stream OpenStream();

        /// <summary>
        /// Opens a stream asynchronously for reading the resource.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a stream
        /// with the resource data.
        /// </returns>
        protected abstract Task<Stream> OpenStreamAsync();

        /// <summary>
        /// Gets a stream for reading the resource.
        /// </summary>
        /// <returns>
        /// A stream containing the resource data.
        /// </returns>
        public Stream GetStream()
        {
            return OpenStream();
        }

        /// <summary>
        /// Gets a stream asynchronously for reading the resource.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a stream
        /// with the resource data.
        /// </returns>
        public async Task<Stream> GetStreamAsync()
        {
            return await OpenStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the resource data as a byte array.
        /// </summary>
        /// <returns>
        /// A byte array containing the resource data.
        /// </returns>
        public byte[] LoadRaw()
        {
            using var s = GetStream();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Loads the resource data asynchronously as a byte array.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, containing a byte
        /// array with the resource data.
        /// </returns>
        public async Task<byte[]> LoadRawAsync()
        {
            using var s = await GetStreamAsync().ConfigureAwait(false);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    public abstract class LoaderBase
    {
        /// <summary>
        /// Liefert einen Stream synchron.
        /// </summary>
        protected abstract Stream OpenStream();

        /// <summary>
        /// Liefert einen Stream asynchron.
        /// </summary>
        protected abstract Task<Stream> OpenStreamAsync();

        /// <summary>
        /// Öffnet den Stream synchron oder wirft, wenn nicht verfügbar.
        /// </summary>
        public Stream GetStream()
        {
            return OpenStream();
        }

        /// <summary>
        /// Öffnet den Stream asynchron oder wirft, wenn nicht verfügbar.
        /// </summary>
        public async Task<Stream> GetStreamAsync()
        {
            return await OpenStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Rohdaten als byte[] synchron.
        /// </summary>
        public byte[] LoadRaw()
        {
            using var s = GetStream();
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Rohdaten als byte[] asynchron.
        /// </summary>
        public async Task<byte[]> LoadRawAsync()
        {
            using var s = await GetStreamAsync().ConfigureAwait(false);
            using var ms = new MemoryStream();
            await s.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }
    }
}

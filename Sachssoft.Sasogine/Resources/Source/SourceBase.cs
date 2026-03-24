using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Source
{
    /// <summary>
    /// Base class for sources that support both loading and saving.
    /// Provides convenience methods for working with raw byte arrays.
    /// </summary>
    // Basisklasse für Quellen, die Laden und Speichern unterstützen.
    // Stellt Komfortmethoden für byte[] bereit.
    public abstract class SourceBase : LoaderBase
    {
        /// <summary>
        /// Saves the provided stream synchronously to the underlying source.
        /// </summary>
        // Speichert den übergebenen Stream synchron in die Quelle.
        protected abstract void SaveStream(Stream stream);

        /// <summary>
        /// Saves the provided stream asynchronously to the underlying source.
        /// </summary>
        // Speichert den übergebenen Stream asynchron in die Quelle.
        protected abstract Task SaveStreamAsync(Stream stream);

        /// <summary>
        /// Saves the provided stream synchronously.
        /// </summary>
        // Speichert den Stream synchron.
        public void PutStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            SaveStream(stream);
        }

        /// <summary>
        /// Saves the provided stream asynchronously.
        /// </summary>
        // Speichert den Stream asynchron.
        public async Task PutStreamAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            await SaveStreamAsync(stream).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves a byte array synchronously.
        /// Internally uses a MemoryStream to pass bytes to <see cref="SaveStream"/>.
        /// </summary>
        // Speichert ein byte[] synchron.
        // Intern wird ein MemoryStream verwendet, um die Bytes an SaveStream weiterzugeben.
        public void SaveRaw(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using var ms = new MemoryStream(bytes);
            SaveStream(ms);
        }

        /// <summary>
        /// Saves a byte array asynchronously.
        /// Internally uses a MemoryStream to pass bytes to <see cref="SaveStreamAsync"/>.
        /// </summary>
        // Speichert ein byte[] asynchron.
        // Intern wird ein MemoryStream verwendet, um die Bytes an SaveStreamAsync weiterzugeben.
        public async Task SaveRawAsync(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            using var ms = new MemoryStream(bytes);
            await SaveStreamAsync(ms).ConfigureAwait(false);
        }
    }
}
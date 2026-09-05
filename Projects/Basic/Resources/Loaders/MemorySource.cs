using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Provides access to resource data stored in memory.
    /// </summary>
    public sealed class MemorySource : ResourceSourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySource"/> class.
        /// </summary>
        public MemorySource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySource"/> class
        /// with the specified resource data.
        /// </summary>
        /// <param name="data">
        /// The resource data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="data"/> is <see langword="null"/>.
        /// </exception>
        public MemorySource(byte[] data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Gets or sets the resource data.
        /// </summary>
        public byte[]? Data { get; set; }

        /// <inheritdoc/>
        protected override Stream OpenStream()
        {
            if (Data == null)
                throw new InvalidOperationException("Data is not set.");

            return new MemoryStream(Data, writable: false);
        }

        /// <inheritdoc/>
        protected override Task<Stream> OpenStreamAsync()
        {
            return Task.FromResult(OpenStream());
        }
    }
}
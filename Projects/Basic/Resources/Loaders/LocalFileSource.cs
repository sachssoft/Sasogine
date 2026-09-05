using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Provides access to resources stored in the local file system.
    /// </summary>
    public sealed class LocalFileSource : ResourceSourceBase, IFileSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileSource"/> class.
        /// </summary>
        public LocalFileSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileSource"/> class
        /// with the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The path of the resource file.
        /// </param>
        public LocalFileSource(string? filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Gets or sets the path of the resource file.
        /// </summary>
        public string? FilePath { get; set; }

        /// <inheritdoc/>
        protected override Stream OpenStream()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                throw new InvalidOperationException("FilePath is not set.");

            try
            {
                return new FileStream(
                    FilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: false
                );
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to open file: {FilePath}", ex);
            }
        }

        /// <inheritdoc/>
        protected override Task<Stream> OpenStreamAsync()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                throw new InvalidOperationException("FilePath is not set.");

            try
            {
                Stream stream = new FileStream(
                    FilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true
                );

                return Task.FromResult(stream);
            }
            catch (Exception ex)
            {
                throw new IOException(
                    $"Failed to open file asynchronously: {FilePath}",
                    ex);
            }
        }
    }
}
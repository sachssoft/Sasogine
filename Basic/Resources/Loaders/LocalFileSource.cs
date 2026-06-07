using System;
using System.IO;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    /// <summary>
    /// Loads a file from the local file system.
    /// Provides existence checking and optional case-sensitive path validation.
    /// Supports synchronous and asynchronous access.
    /// </summary>
    public sealed class LocalFileSource : ResourceSourceBase, IFileSource
    {

        public LocalFileSource()
        {
        }

        public LocalFileSource(string? filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Vollständiger Pfad oder Name der Ressource. Optional.
        /// </summary>
        public string? FilePath { get; set; }

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
                throw new IOException($"Failed to open file asynchronously: {FilePath}", ex);
            }
        }
    }
}

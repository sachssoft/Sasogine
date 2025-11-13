using System;
using System.IO;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Loads a file from the local file system.
    /// Provides existence checking and optional case-sensitive path validation.
    /// </summary>
    public sealed class LocalFileLoader : LoaderBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="LocalFileLoader"/> for the specified file path.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        public LocalFileLoader(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        /// Opens a readable <see cref="Stream"/> to the local file.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/> of the file.</returns>
        /// <exception cref="IOException">Thrown if the file cannot be opened.</exception>
        protected override Stream StreamOpen()
        {
            try
            {
                return new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to open file: {FilePath}", ex);
            }
        }

        /// <summary>
        /// Checks whether the file exists on the local file system.
        /// Performs optional case-sensitive validation on case-insensitive platforms.
        /// </summary>
        public override bool IsFileExist
        {
            get
            {
                if (!File.Exists(FilePath))
                    return false;

                // Perform optional case-sensitive check (important for Linux/macOS)
                var directory = Path.GetDirectoryName(FilePath);
                if (directory is null)
                    return false;

                var fileName = Path.GetFileName(FilePath);
                try
                {
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        if (string.Equals(fileName, Path.GetFileName(file), StringComparison.Ordinal))
                            return true;
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }
        }
    }
}

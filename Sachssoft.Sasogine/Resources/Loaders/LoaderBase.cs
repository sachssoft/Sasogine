using System;
using System.IO;

namespace Sachssoft.Sasogine.Resources
{
    /// <summary>
    /// Abstract base class for file loaders. Provides basic validation, optional case-sensitive
    /// file path checking, and defines the interface for file existence and stream access.
    /// </summary>
    public abstract class LoaderBase
    {
        private readonly string _filePath;
        private readonly string _fileName;

        /// <summary>
        /// Initializes a new instance of <see cref="LoaderBase"/> with the specified file path.
        /// Performs null/empty checks and initializes file metadata. 
        /// </summary>
        /// <param name="filePath">The full path to the file to load.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is invalid (cannot determine directory or file name).</exception>
        /// <remarks>
        /// Derived classes may implement additional checks for case-sensitive file systems
        /// to ensure the file exists exactly as specified.
        /// </remarks>
        protected LoaderBase(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty.");

            // Ensure path is valid
            var directory = Path.GetDirectoryName(filePath);
            if (directory is null)
                throw new ArgumentException("Invalid file path. Could not determine directory.", nameof(filePath));

            _filePath = filePath;
            _fileName = Path.GetFileName(filePath);
        }

        /// <summary>
        /// Gets the full path of the file.
        /// </summary>
        public string FilePath => _filePath;

        /// <summary>
        /// Gets the name of the file, without directory information.
        /// </summary>
        public string FileName => _fileName;

        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// Derived classes must implement the actual existence check.
        /// </summary>
        public abstract bool IsFileExist { get; }

        /// <summary>
        /// Opens a readable <see cref="Stream"/> to the file.
        /// Derived classes must implement the actual stream access logic.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/> for the file.</returns>
        protected abstract Stream StreamOpen();

        /// <summary>
        /// Returns a readable <see cref="Stream"/> for the file.
        /// Throws <see cref="FileNotFoundException"/> if the file does not exist.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/> of the file.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        public Stream GetStream()
        {
            if (!IsFileExist)
                throw new FileNotFoundException($"File does not exist: {_filePath}", _filePath);

            return StreamOpen();
        }
    }
}

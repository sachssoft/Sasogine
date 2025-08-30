using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Containers;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Represents a base implementation of a package containing assets, levels, and metadata.
    /// Provides functionality to open, close, save, and manage package files.
    /// </summary>
    public abstract class PackageBase : IDisposable, IPackage
    {
        protected private ZipArchive? _source;
        protected private Stream? _stream;
        private bool _isClosed = true;
        protected private bool _isReadOnly;
        protected private PackageManifest _manifest;
        private bool _isDisposed;
        private PackageLicense _license;
        private PackageIcon _icon;
        private PackagePreviews _previews;
        private IDocumentFormatter _manifestFormat = new JsonDocumentFormatter();
        private readonly Func<Stream> _openFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageBase"/> class.
        /// </summary>
        /// <param name="streamFactory">A factory function that provides the package stream.</param>
        /// <param name="isReadOnly">Indicates whether the package should be opened in read-only mode.</param>
        /// <param name="manifest">Optional manifest to initialize the package with.</param>
        internal PackageBase(Func<Stream> streamFactory, bool isReadOnly, PackageManifest? manifest = null)
        {
            _openFunc = streamFactory ?? throw new ArgumentNullException(nameof(streamFactory));
            _isReadOnly = isReadOnly;

            _manifest = manifest ?? new PackageManifest();
            _manifest._package = this;

            _license = new PackageLicense(this);
            _icon = new PackageIcon(this);
            _previews = new PackagePreviews(this);
        }

        /// <summary>
        /// Gets the underlying ZIP archive. May be null if the package is not opened.
        /// </summary>
        internal ZipArchive? Source => _source;

        /// <summary>
        /// Gets a value indicating whether the package is currently open.
        /// </summary>
        public bool IsOpen => _source != null;

        /// <summary>
        /// Gets a value indicating whether the package is read-only.
        /// </summary>
        public bool IsReadOnly => _isReadOnly;

        /// <summary>
        /// Gets the package manifest, containing metadata and configuration.
        /// </summary>
        public PackageManifest Manifest => _manifest;

        /// <summary>
        /// Gets or sets the document formatter used for reading and writing the manifest.
        /// </summary>
        [AllowNull]
        public IDocumentFormatter ManifestFormat
        {
            get => _manifestFormat;
            set => _manifestFormat = value ?? new JsonDocumentFormatter();
        }

        /// <summary>
        /// Gets the package license information.
        /// </summary>
        public PackageLicense License => _license;

        /// <summary>
        /// Gets the package icon handler.
        /// </summary>
        public PackageIcon Icon => _icon;

        /// <summary>
        /// Gets the package previews handler.
        /// </summary>
        public PackagePreviews Previews => _previews;

        IReadOnlyDictionary<string, IPackageAsset> IPackage.Assets => new Dictionary<string, IPackageAsset>();

        IReadOnlyCollection<PackageLevelBase> IPackage.Levels => Array.Empty<PackageLevelBase>();

        /// <summary>
        /// Opens the package safely, loading the manifest and ZIP archive.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the package is already open or the archive is invalid.</exception>
        /// <exception cref="IOException">Thrown if the package stream cannot be accessed.</exception>
        public void Open()
        {
            ThrowIfDisposed();
            if (_source != null)
                throw new InvalidOperationException("Package is already open.");

            if (_isClosed)
            {
                _isClosed = false;

                try
                {
                    _stream = _openFunc() ?? throw new IOException("Stream factory returned null.");
                    _stream.Position = 0;

                    if (!_isReadOnly && (!_stream.CanRead || !_stream.CanWrite || !_stream.CanSeek))
                        throw new PackageException("Cannot open package in edit mode.");

                    _source = new ZipArchive(_stream, _isReadOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update);
                    _manifest.Load();
                }
                catch (InvalidDataException ex)
                {
                    throw new InvalidOperationException("Package format is invalid or corrupted.", ex);
                }
                catch (IOException ex)
                {
                    throw new IOException("Failed to open package stream.", ex);
                }
            }
        }

        /// <summary>
        /// Closes the package, saves changes if necessary, and releases all resources.
        /// </summary>
        public void Close()
        {
            ThrowIfDisposed();

            try
            {
                if (_source != null && !IsReadOnly)
                    _manifest.Save();

                foreach (var level in _manifest._levels)
                    level.Close();

                _source?.Dispose();
                _stream?.Dispose();
            }
            finally
            {
                _source = null;
                _stream = null;
                _isClosed = true;
            }
        }

        /// <summary>
        /// Saves the package by closing and reopening it.
        /// </summary>
        public void Save()
        {
            Close();
            Open();
        }

        /// <summary>
        /// Disposes the package and releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            foreach (var level in _manifest._levels)
                level.Dispose();

            _source?.Dispose();
            _stream?.Dispose();

            Disposing();

            _source = null;
            _stream = null;
            _isClosed = true;
            _isDisposed = true;
        }

        /// <summary>
        /// Called when the instance is being disposed.
        /// Override this method in derived classes
        /// to implement custom cleanup logic for managed resources.
        /// </summary>
        protected virtual void Disposing()
        {
        }

        /// <summary>
        /// Checks whether a file exists in the package by its path.
        /// </summary>
        /// <param name="filePath">The file path inside the package.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        internal bool IsFileExists(string filePath)
        {
            return Source?.Entries.Any(e =>
                e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Opens a file from the package as a stream.
        /// </summary>
        /// <param name="filePath">The file path inside the package.</param>
        /// <returns>A stream of the file, or null if not found.</returns>
        internal Stream? OpenFile(string filePath)
        {
            return Source?.Entries
                .FirstOrDefault(e => e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
                ?.Open();
        }

        /// <summary>
        /// Moves a file within the package to a new path.
        /// </summary>
        /// <param name="oldFilePath">The current file path.</param>
        /// <param name="newFilePath">The target file path.</param>
        /// <exception cref="ArgumentException">If the target path is invalid.</exception>
        /// <exception cref="PackageException">If the move fails.</exception>
        internal void MoveFileTo(string oldFilePath, string newFilePath)
        {
            ThrowIfDisposed();
            ThrowIfNotOpened();
            ThrowIfIsReadOnly();

            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentException("Target path cannot be empty.", nameof(newFilePath));

            if (!IsFileExists(oldFilePath))
                throw new PackageException($"Source file '{oldFilePath}' does not exist in the package.");

            if (IsFileExists(newFilePath))
                throw new PackageException($"Target file '{newFilePath}' already exists in the package.");

            var oldEntry = _source.GetEntry(oldFilePath);
            if (oldEntry == null)
                throw new PackageException($"Source entry '{oldFilePath}' not found in the ZIP archive.");

            try
            {
                var newEntry = _source.CreateEntry(newFilePath);

                using var oldStream = oldEntry.Open();
                using var newStream = newEntry.Open();
                oldStream.CopyTo(newStream);

                oldEntry.Delete();
            }
            catch (Exception ex)
            {
                throw new PackageException($"Failed to move file '{oldFilePath}' to '{newFilePath}'.", ex);
            }
        }

        /// <summary>
        /// Throws if the package has been disposed.
        /// </summary>
        internal void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PackageBase));
        }

        /// <summary>
        /// Throws if the package is not opened.
        /// </summary>
        [MemberNotNull(nameof(Source))]
        [MemberNotNull(nameof(_source))]
        internal void ThrowIfNotOpened()
        {
            if (Source == null && !_isDisposed)
                throw new PackageException("Package is not opened.");
        }

        /// <summary>
        /// Throws if the package is read-only.
        /// </summary>
        internal void ThrowIfIsReadOnly()
        {
            if (IsReadOnly)
                throw new PackageException("Package is read-only.");
        }
    }
}

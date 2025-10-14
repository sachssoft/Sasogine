using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Observables;
using Sachssoft.Sasogine.Assets;
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
        private readonly object _syncRoot = new();


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

        internal ZipArchive? Source => _source;
        public bool IsOpen => _source != null;
        public bool IsReadOnly => _isReadOnly;
        public PackageManifest Manifest => _manifest;

        [AllowNull]
        public IDocumentFormatter ManifestFormat
        {
            get => _manifestFormat;
            set => _manifestFormat = value ?? new JsonDocumentFormatter();
        }

        public PackageLicense License => _license;
        public PackageIcon Icon => _icon;
        public PackagePreviews Previews => _previews;

        IReadOnlyDictionary<string, IAssetSource> IPackage.Assets => new Dictionary<string, IAssetSource>();
        IReadOnlyList<PackageLevelBase> IPackage.Levels => Array.Empty<PackageLevelBase>();

        public void Open()
        {
            ThrowIfDisposed();
            if (IsOpen) return;

            if (_source != null)
                throw new InvalidOperationException("Package is already open.");

            if (_isClosed)
            {
                lock (_syncRoot)
                {
                    _isClosed = false;

                    try
                    {
                        _stream = _openFunc() ?? throw new IOException("Stream factory returned null.");
                        _stream.Position = 0;

                        if (!_isReadOnly && (!_stream.CanRead || !_stream.CanWrite || !_stream.CanSeek))
                            throw new PackageException("Cannot open package in edit mode.");

                        // 🟡 leaveOpen = true sorgt dafür, dass _stream NICHT geschlossen wird!
                        _source = new ZipArchive(
                            _stream,
                            _isReadOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update,
                            leaveOpen: true
                        );

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
        }

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

        public void Save()
        {
            ThrowIfDisposed();
            ThrowIfNotOpened();
            ThrowIfIsReadOnly();

            // 1. Manifest & Level speichern
            _manifest.Save();
            foreach (var level in _manifest._levels)
            {
                if (level.IsOpen && level.IsDirty)
                    level.Save();
            }

            // 2. Archiv schließen — schreibt alle Änderungen in den Stream
            _source?.Dispose();
            _source = null;

            // 3. Stream flushen
            if (_stream != null)
            {
                _stream.Flush();
                _stream.Position = 0;
            }

            // 4. Archiv wieder öffnen, damit das Paket weiter verwendbar ist
            _source = new ZipArchive(
                _stream!,
                _isReadOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update,
                leaveOpen: true
            );
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            try
            {
                foreach (var level in _manifest._levels)
                    level.Dispose();

                _source?.Dispose();
                _stream?.Dispose();

                Disposing();
            }
            finally
            {
                _source = null;
                _stream = null;
                _isClosed = true;
                _isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }

        protected virtual void Disposing()
        {
        }

        internal bool IsFileExists(string filePath)
        {
            return Source?.Entries.Any(e =>
                e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }

        internal Stream? OpenFile(string filePath)
        {
            return Source?.Entries
                .FirstOrDefault(e => e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
                ?.Open();
        }

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

        internal void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PackageBase));
        }

        [MemberNotNull(nameof(Source))]
        [MemberNotNull(nameof(_source))]
        internal void ThrowIfNotOpened()
        {
            if (Source == null && !_isDisposed)
                throw new PackageException("Package is not opened.");
        }

        internal void ThrowIfIsReadOnly()
        {
            if (IsReadOnly)
                throw new PackageException("Package is read-only.");
        }
    }
}

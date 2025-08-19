using Sachssoft.Documents;
using Sachssoft.Documents.Json;
using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Sachssoft.Sasogine.Containers
{
    public abstract class PackageBase : IDisposable
    {
        protected private ZipArchive? _source;
        protected private Stream _stream;
        protected private bool _isReadOnly;
        private bool _isDisposed;
        private PackageManifest _manifest;
        private PackageLicense _license;
        private PackageIcon _icon;
        private PackagePreviews _previews;
        private List<PackageBase> _levels;
        private IDocumentFormatter _manifestFormat = new JsonDocumentFormatter();

        internal PackageBase(Stream stream, bool isReadOnly, PackageManifest? manifest = null)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _isReadOnly = isReadOnly;

            _manifest = manifest ?? new PackageManifest();
            _manifest._package = this;

            _license = new PackageLicense(this);
            _icon = new PackageIcon(this);
            _previews = new PackagePreviews(this);
            _levels = new List<PackageBase>();
        }

        internal ZipArchive? Source => _source;

        internal List<PackageBase> InnerLevels => _levels;

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

        public abstract IPackageAsset? GetAsset(string filePath);
        public abstract IReadOnlyCollection<PackageLevelBase> Levels { get; }

        internal bool IsFileExists(string filePath)
        {
            return Source?.Entries.Any(e => e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase)) ?? false;
        }

        internal Stream? OpenFile(string filePath)
        {
            return Source?.Entries.FirstOrDefault(e => e.FullName.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))?.Open();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(PackageBase));
        }

        internal void ThrowIfNotOpened()
        {
            if (Source == null && !_isDisposed)
                throw new InvalidOperationException("Package Not Opened");
        }

        internal void ThrowIfIsReadOnly()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Package is read-only");
        }

        public void Open()
        {
            ThrowIfDisposed();
            if (_source != null)
                throw new InvalidOperationException("Package ist bereits geöffnet.");

            _stream.Position = 0;

            if (!_isReadOnly && (!_stream.CanRead || !_stream.CanWrite || !_stream.CanSeek))
                throw new InvalidOperationException("Stream kann für Update nicht genutzt werden");

            _source = new ZipArchive(_stream, _isReadOnly ? ZipArchiveMode.Read : ZipArchiveMode.Update);
            _manifest.Load();
        }

        public void Close()
        {
            ThrowIfDisposed();
            if (_source == null)
                throw new InvalidOperationException("Package ist nicht geöffnet.");

            _source.Dispose();
            _source = null;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _source?.Dispose();
            _stream?.Dispose();
            _source = null;
            _stream = null!;
            _isDisposed = true;
        }
    }

}

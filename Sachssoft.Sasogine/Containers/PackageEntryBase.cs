using Sachssoft.Sasogine.Elements;
using Sachssoft.Sasogine.Features;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Represents a single entry within a package.
    /// Provides methods to create, duplicate, read, write, and delete the entry.
    /// </summary>
    public abstract class PackageEntryBase : GameObject, IDisposable
    {
        private bool _isDeleted;
        private bool _isOpen;
        private bool _isDisposed;
        private readonly PackageBase _package;
        private ZipArchiveEntry? _packageEntry;
        private Stream? _stream;
        private bool _isDirty;
        private string _relativeFilePath;

        /// <summary>
        /// Occurs when the dirty state changes.
        /// </summary>
        public event EventHandler? IsDirtyChanged;

        /// <summary>
        /// Initializes a new instance of <see cref="PackageEntryBase"/>.
        /// </summary>
        /// <param name="package">The parent package.</param>
        /// <param name="filePath">The relative file path of the entry.</param>
        /// <exception cref="ArgumentNullException"></exception>
        protected PackageEntryBase(PackageBase package, string filePath)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            RelativeFilePath = filePath?.Trim() ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Gets the parent package.
        /// </summary>
        protected PackageBase Package => _package;

        /// <summary>
        /// Gets the relative file path of the entry within the package.
        /// </summary>
        public string RelativeFilePath
        {
            get => _relativeFilePath;
            internal set
            {
                if (_relativeFilePath != value)
                {
                    var oldFilePath = _relativeFilePath;
                    _relativeFilePath = value;
                    OnRelativeFilePathChanged(oldFilePath, value);
                }
            }
        }

        /// <summary>
        /// Gets the root path for this entry.
        /// </summary>
        protected virtual string RootPath => string.Empty;

        /// <summary>
        /// Gets the absolute file path in the package.
        /// </summary>
        public string AbsoluteFilePath => RootPath + RelativeFilePath;

        /// <summary>
        /// Gets a value indicating whether the entry is currently open.
        /// </summary>
        public bool IsOpen => _isOpen;

        /// <summary>
        /// Gets a value indicating whether the entry has been deleted.
        /// </summary>
        public bool IsDeleted => _isDeleted;

        /// <summary>
        /// Gets or sets a value indicating whether the entry has unsaved changes.
        /// </summary>
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    IsDirtyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the entry exists in the package.
        /// </summary>
        public bool IsExists
        {
            get
            {
                EnsurePackage(writable: false);
                return _package.IsFileExists(RelativeFilePath);
            }
        }

        /// <summary>
        /// Ensures the package is valid and optionally writable.
        /// </summary>
        /// <param name="writable">If true, throws if the package is read-only.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected void EnsurePackage(bool writable = true)
        {
            _package.ThrowIfDisposed();
            _package.ThrowIfNotOpened();
            if (writable)
                _package.ThrowIfIsReadOnly();
        }

        /// <summary>
        /// Creates the entry in the package.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the entry already exists.</exception>
        public void Create()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            if (_package.IsFileExists(RelativeFilePath))
                throw new InvalidOperationException($"File '{RelativeFilePath}' already exists.");

            _packageEntry = _package.Source.CreateEntry(AbsoluteFilePath);
        }

        /// <summary>
        /// Duplicates the current entry under a new file path in the same package.
        /// </summary>
        /// <param name="newFilePath">The new relative file path.</param>
        /// <exception cref="InvalidOperationException">If the new file already exists.</exception>
        public void Duplicate(string newFilePath)
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            var rootPath = RootPath.TrimEnd('/');
            var absoluteFilePath = rootPath + "/" + newFilePath;

            if (_package.IsFileExists(absoluteFilePath))
                throw new InvalidOperationException($"File '{newFilePath}' already exists.");

            var copyEntry = _package.Source.CreateEntry(absoluteFilePath);

            //using (var originalStream = _packageEntry.Open())
            using (var copyStream = copyEntry.Open())
            {
                byte[] buffer = new byte[81920]; // 80 KB buffer
                int bytesRead;
                while ((bytesRead = _stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    copyStream.Write(buffer, 0, bytesRead);
                }
            }

            OnDuplicated(newFilePath);
        }

        /// <summary>
        /// Opens the entry for reading.
        /// </summary>
        /// <exception cref="FileNotFoundException">If the entry does not exist.</exception>
        public void Open()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: false);

            var entry = _package.Source.GetEntry(AbsoluteFilePath)
                        ?? throw new FileNotFoundException($"File '{RelativeFilePath}' does not exist in the package.", AbsoluteFilePath);

            _packageEntry = entry;
            _stream?.Dispose();
            _stream = _packageEntry.Open();
            _isOpen = true;

            OnLoad();
            IsDirty = false;
        }

        /// <summary>
        /// Closes the entry stream.
        /// </summary>
        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
            _isOpen = false;
            OnUnload();
        }

        /// <summary>
        /// Deletes the entry from the package.
        /// </summary>
        public void Delete()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            var entry = _package.Source.GetEntry(AbsoluteFilePath);
            if (entry != null)
            {
                entry.Delete();
                _isDeleted = true;
                OnDeleted();
            }
        }

        /// <summary>
        /// Changes the relative file path of the entry.
        /// </summary>
        /// <param name="newFilePath">The new relative file path.</param>
        /// <exception cref="ArgumentException">If the new path is empty.</exception>
        /// <exception cref="InvalidOperationException">If the package is read-only or not projected.</exception>
        public void ChangeFilePath(string newFilePath)
        {
            ThrowsIfDeleted();
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentException("New file path cannot be empty.", nameof(newFilePath));

            EnsurePackage(writable: true);

            if (_package is not ProjectedPackage projected || _package.IsReadOnly)
                throw new InvalidOperationException("Package is read-only or not a ProjectedPackage.");

            projected.Levels.MoveTo(RelativeFilePath, newFilePath.Trim());
            RelativeFilePath = newFilePath.Trim();
        }

        /// <summary>
        /// Called after the entry is opened. Override to implement custom loading logic.
        /// </summary>
        protected virtual void OnLoad()
        {
        }

        /// <summary>
        /// Called after the entry has been closed.
        /// Override this method to implement custom cleanup or resource release logic.
        /// </summary>
        protected virtual void OnUnload()
        {
        }

        /// <summary>
        /// Saves the entry.
        /// </summary>
        public void Save()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            OnSave();
            IsDirty = false;
        }

        /// <summary>
        /// Called when saving. Override to implement custom saving logic.
        /// </summary>
        protected virtual void OnSave()
        {
        }

        /// <summary>
        /// Called after the entry has been deleted.
        /// Override this method to implement custom deletion logic or cleanup.
        /// </summary>
        protected virtual void OnDeleted()
        {
        }

        /// <summary>
        /// Called after the entry has been duplicated to a new file path.
        /// Override this method to implement custom logic after duplication.
        /// </summary>
        /// <param name="newFilePath">The relative file path of the duplicated entry.</param>
        protected virtual void OnDuplicated(string newFilePath)
        {
        }

        /// <summary>
        /// Called after the entry's file path has been changed.
        /// Override this method to implement custom logic when the file path changes.
        /// </summary>
        /// <param name="oldFilePath">The previous relative file path of the entry.</param>
        /// <param name="newFilePath">The new relative file path of the entry.</param>
        protected virtual void OnRelativeFilePathChanged(string oldFilePath, string newFilePath)
        {
        }

        internal protected Stream Read(PackageEntryAccessOptions? options = null)
        {
            options ??= new PackageEntryAccessOptions();

            ThrowsIfDeleted();
            _package.ThrowIfDisposed();

            var memory = new MemoryStream();
            CopyStreamWithProgress(_stream, memory, options);
            memory.Position = 0;
            return memory;
        }

        internal protected async Task<Stream> ReadAsync(PackageEntryAccessOptions? options = null)
        {
            options ??= new PackageEntryAccessOptions();

            ThrowsIfDeleted();
            _package.ThrowIfDisposed();

            var memory = new MemoryStream();
            await CopyStreamAsync(_stream, memory, options);
            memory.Position = 0;
            return memory;
        }

        private static void CopyStreamWithProgress(Stream source, Stream target, PackageEntryAccessOptions options)
        {
            byte[] buffer = new byte[options.BufferSize];
            long totalBytes = 0;
            long length = source.CanSeek ? source.Length : -1;
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                target.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
                if (options.Progress != null && length > 0)
                    options.Progress.Report((double)totalBytes / length);
            }
        }

        internal protected void Write(Stream source, PackageEntryAccessOptions? options = null)
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.CanSeek)
                source.Position = 0;

            string fullPath = AbsoluteFilePath;
            options ??= new PackageEntryAccessOptions();

            _stream?.Close();
            _packageEntry?.Delete();
            _packageEntry = _package.Source.CreateEntry(fullPath);

            Stream tempStream = options.Mode switch
            {
                PackageEntryAccessMode.Direct => source,
                PackageEntryAccessMode.TemporaryFile => new FileStream(
                    Path.GetTempFileName(),
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    options.BufferSize,
                    FileOptions.DeleteOnClose),
                PackageEntryAccessMode.MemoryOnly => new MemoryStream(),
                _ => throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, null)
            };

            using (tempStream)
            {
                using var entryStream = _packageEntry.Open();
                CopyStream(source, entryStream, options.BufferSize);
            }
        }

        protected async Task WriteAsync(Stream source, PackageEntryAccessOptions? options = null)
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.CanSeek)
                source.Position = 0;

            string fullPath = AbsoluteFilePath;
            options ??= new PackageEntryAccessOptions();

            _package.Source.GetEntry(fullPath)?.Delete();
            _packageEntry = _package.Source.CreateEntry(fullPath);

            Stream tempStream = options.Mode switch
            {
                PackageEntryAccessMode.Direct => source,
                PackageEntryAccessMode.TemporaryFile => new FileStream(
                    Path.GetTempFileName(),
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    options.BufferSize,
                    FileOptions.DeleteOnClose),
                PackageEntryAccessMode.MemoryOnly => new MemoryStream(),
                _ => throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, null)
            };

            using (tempStream)
            {
                using var entryStream = _packageEntry.Open();
                await CopyStreamAsync(source, entryStream, options);
            }
        }

        private static void CopyStream(Stream source, Stream target, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            do
            {
                bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                    target.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);
        }

        private static async Task CopyStreamAsync(Stream source, Stream target, PackageEntryAccessOptions options)
        {
            byte[] buffer = new byte[options.BufferSize];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, options.CancellationToken)) > 0)
            {
                await target.WriteAsync(buffer, 0, bytesRead, options.CancellationToken);
                if (options.Progress != null && source.CanSeek)
                {
                    options.Progress.Report((double)source.Position / source.Length);
                }
            }
        }

        private void ThrowsIfDeleted()
        {
            if (_isDeleted)
                throw new InvalidOperationException($"File '{RelativeFilePath}' has been deleted.");
        }

        /// <summary>
        /// Disposes the entry and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                Close();
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}

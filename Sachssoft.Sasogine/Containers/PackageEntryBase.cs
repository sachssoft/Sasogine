using Sachssoft.Sasogine.Elements;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Containers
{
    public abstract class PackageEntryBase : GameObject, IDisposable
    {
        private bool _isDeleted;
        private bool _isOpen;
        private bool _isDisposed;
        private readonly PackageBase _package;
        private ZipArchiveEntry? _packageEntry;
        private Stream? _stream;
        private bool _isDirty;

        public event EventHandler? IsDirtyChanged;

        protected PackageEntryBase(PackageBase package, string filePath)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            RelativeFilePath = filePath?.Trim() ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string RelativeFilePath { get; internal set; }

        protected virtual string RootPath => string.Empty;

        public string AbsoluteFilePath => RootPath + RelativeFilePath;

        public bool IsOpen => _isOpen;

        public bool IsDeleted => _isDeleted;

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

        public bool IsExists
        {
            get
            {
                EnsurePackage(writable: false);
                return _package.IsFileExists(RelativeFilePath);
            }
        }

        protected void EnsurePackage(bool writable = true)
        {
            _package.ThrowIfDisposed();
            _package.ThrowIfNotOpened();
            if (writable)
                _package.ThrowIfIsReadOnly();
        }

        public void Create()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            if (_package.IsFileExists(RelativeFilePath))
                throw new InvalidOperationException($"Datei '{RelativeFilePath}' existiert bereits.");

            _packageEntry = _package.Source.CreateEntry(AbsoluteFilePath);
        }

        public void Open()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: false);

            var entry = _package.Source.GetEntry(AbsoluteFilePath)
                        ?? throw new FileNotFoundException($"Level '{RelativeFilePath}' existiert nicht im Archiv.", AbsoluteFilePath);

            _packageEntry = entry;
            _stream?.Dispose();
            _stream = _packageEntry.Open();
            _isOpen = true;

            OnLoad();
            IsDirty = false;
        }

        public void Close()
        {
            _stream?.Dispose();
            _stream = null;
            _isOpen = false;
        }

        public void Delete()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            var entry = _package.Source.GetEntry(AbsoluteFilePath);
            if (entry != null)
            {
                entry.Delete();
                _isDeleted = true;
            }
        }

        public void ChangeFilePath(string newFilePath)
        {
            ThrowsIfDeleted();
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentException("Neuer Dateipfad darf nicht leer sein.", nameof(newFilePath));

            EnsurePackage(writable: true);

            if (_package is not ProjectedPackage projected || _package.IsReadOnly)
                throw new InvalidOperationException("Package ist schreibgeschützt oder kein ProjectedPackage.");

            projected.Levels.MoveTo(RelativeFilePath, newFilePath.Trim());
            RelativeFilePath = newFilePath.Trim();
        }

        protected virtual void OnLoad()
        {
            // Level-spezifisches Laden implementieren
        }

        public void Save()
        {
            ThrowsIfDeleted();
            EnsurePackage(writable: true);

            OnSave();
            IsDirty = false;
        }

        protected virtual void OnSave()
        {
            // Level-spezifisches Speichern implementieren
        }

        internal protected Stream Read(PackageEntryAccessOptions? options = null)
        {
            options ??= new PackageEntryAccessOptions();

            ThrowsIfDeleted();
            _package.ThrowIfDisposed();

            // In MemoryStream kopieren, damit mehrfaches Lesen möglich ist
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

        // Hilfsmethode mit optionalem Progress
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

            // Alten Eintrag löschen
            _stream?.Close(); // Stream beenden bevor löschen
            _packageEntry?.Delete();
            //_package.Source.GetEntry(fullPath)?.Delete();
            // Neuen Eintrag erstellen
            _packageEntry = _package.Source.CreateEntry(fullPath);

            Stream tempStream;

            // Stream je nach Mode auswählen
            switch (options.Mode)
            {
                case PackageEntryAccessMode.Direct:
                    tempStream = source; // direkt aus Quelle in ZIP
                    break;
                case PackageEntryAccessMode.TemporaryFile:
                    tempStream = new FileStream(
                        Path.GetTempFileName(),
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        options.BufferSize,
                        FileOptions.DeleteOnClose);
                    // Daten aus Quelle in temporären Stream kopieren
                    CopyStream(source, tempStream, options.BufferSize);
                    tempStream.Position = 0;
                    break;
                case PackageEntryAccessMode.MemoryOnly:
                    tempStream = new MemoryStream();
                    CopyStream(source, tempStream, options.BufferSize);
                    tempStream.Position = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, null);
            }

            using (tempStream)
            {
                using var entryStream = _packageEntry.Open();
                CopyStream(tempStream, entryStream, options.BufferSize);
            }
        }

        // Hilfsmethode für synchrones Kopieren mit Puffer
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

            // Alten Eintrag löschen
            _package.Source.GetEntry(fullPath)?.Delete();
            // Neuen Eintrag erstellen
            _packageEntry = _package.Source.CreateEntry(fullPath);

            Stream tempStream;

            switch (options.Mode)
            {
                case PackageEntryAccessMode.Direct:
                    tempStream = source; // direkt schreiben
                    break;
                case PackageEntryAccessMode.TemporaryFile:
                    tempStream = new FileStream(
                        Path.GetTempFileName(),
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        options.BufferSize,
                        FileOptions.DeleteOnClose);
                    await CopyStreamAsync(source, tempStream, options);
                    tempStream.Position = 0;
                    break;
                case PackageEntryAccessMode.MemoryOnly:
                    tempStream = new MemoryStream();
                    await CopyStreamAsync(source, tempStream, options);
                    tempStream.Position = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, null);
            }

            using (tempStream)
            {
                using var entryStream = _packageEntry.Open();
                await CopyStreamAsync(tempStream, entryStream, options);
            }
        }

        private static async Task CopyStreamAsync(Stream source, Stream target, PackageEntryAccessOptions options)
        {
            byte[] buffer = new byte[options.BufferSize];
            long totalBytes = 0;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, options.CancellationToken)) > 0)
            {
                await target.WriteAsync(buffer, 0, bytesRead, options.CancellationToken);
                totalBytes += bytesRead;
                if (options.Progress != null && source.CanSeek)
                {
                    options.Progress.Report((double)source.Position / source.Length);
                }
            }
        }

        private void ThrowsIfDeleted()
        {
            if (_isDeleted)
                throw new InvalidOperationException($"Level '{RelativeFilePath}' wurde gelöscht.");
        }

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

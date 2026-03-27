using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Source
{
    /// <summary>
    /// Base class for sources that store data in isolated storage.
    /// Provides scoped, sandboxed file access.
    /// </summary>
    // Basisklasse für Quellen, die Daten im Isolated Storage speichern.
    // Bietet scoped, sandboxed Zugriff auf Dateien.
    public abstract class StorageSourceBase : SourceBase
    {
        /// <summary>
        /// The isolated storage scope to use (User/Domain/Assembly).
        /// </summary>
        // Der IsolatedStorageScope, der verwendet wird (User/Domain/Assembly).
        protected IsolatedStorageScope Scope { get; }

        /// <summary>
        /// Relative path within the isolated storage.
        /// </summary>
        // Relativer Pfad innerhalb des Isolated Storage.
        public string RelativePath { get; }

        protected StorageSourceBase(string relativePath, IsolatedStorageScope scope = IsolatedStorageScope.User | IsolatedStorageScope.Assembly)
        {
            RelativePath = relativePath ?? throw new ArgumentNullException(nameof(relativePath));
            Scope = scope;
        }

        private IsolatedStorageFile GetStorage()
        {
            return IsolatedStorageFile.GetStore(Scope, null, null);
        }

        // ---- LoaderBase Implementation ----

        protected override Stream OpenStream()
        {
            var storage = GetStorage();
            if (!storage.FileExists(RelativePath))
                throw new FileNotFoundException("File not found in isolated storage", RelativePath);

            return storage.OpenFile(RelativePath, FileMode.Open, FileAccess.Read);
        }

        protected override async Task<Stream> OpenStreamAsync()
        {
            var storage = GetStorage();
            if (!storage.FileExists(RelativePath))
                throw new FileNotFoundException("File not found in isolated storage", RelativePath);

            var ms = new MemoryStream();
            using (var fs = storage.OpenFile(RelativePath, FileMode.Open, FileAccess.Read))
            {
                await fs.CopyToAsync(ms).ConfigureAwait(false);
            }
            ms.Position = 0;
            return ms;
        }

        // ---- SourceBase Implementation ----

        protected override void SaveStream(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var storage = GetStorage();
            using var fs = storage.OpenFile(RelativePath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
        }

        protected override async Task SaveStreamAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            using var storage = GetStorage();
            using var fs = storage.OpenFile(RelativePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fs).ConfigureAwait(false);
        }
    }
}
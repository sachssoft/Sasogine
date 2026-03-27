using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Loaders
{
    /// <summary>
    /// Loader für Daten von einer URL oder Remote-Quelle.
    /// Lädt bei Zugriff (Lazy) und cached die Daten.
    /// </summary>
    public sealed class RemoteLoader : LoaderBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private byte[]? _cachedData;
        private readonly object _lock = new object();

        /// <summary>
        /// URL oder Remote-Pfad. Muss gesetzt werden.
        /// </summary>
        public string? Url { get; set; }

        public RemoteLoader() { }

        public RemoteLoader(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        protected override Stream OpenStream()
        {
            EnsureDataLoaded();

            return new MemoryStream(_cachedData!, writable: false);
        }

        protected override async Task<Stream> OpenStreamAsync()
        {
            if (_cachedData == null)
            {
                _cachedData = await _httpClient.GetByteArrayAsync(Url!).ConfigureAwait(false);
            }

            return new MemoryStream(_cachedData, writable: false);
        }

        private void EnsureDataLoaded()
        {
            if (_cachedData == null)
            {
                lock (_lock)
                {
                    if (_cachedData == null)
                    {
                        try
                        {
                            _cachedData = _httpClient.GetByteArrayAsync(Url!).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            throw new IOException($"Failed to download data from URL: {Url}", ex);
                        }
                    }
                }
            }
        }
    }
}

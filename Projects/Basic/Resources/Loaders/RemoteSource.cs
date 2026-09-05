using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Resources.Sources
{
    /// <summary>
    /// Provides access to resource data from a remote location.
    /// </summary>
    public sealed class RemoteSource : ResourceSourceBase
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);
        private byte[]? _cachedData;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteSource"/> class.
        /// </summary>
        public RemoteSource()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteSource"/> class
        /// with the specified URL.
        /// </summary>
        /// <param name="url">
        /// The URL of the remote resource.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="url"/> is <see langword="null"/>.
        /// </exception>
        public RemoteSource(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        /// <summary>
        /// Gets or sets the URL of the remote resource.
        /// </summary>
        public string? Url { get; set; }

        /// <inheritdoc/>
        protected override Stream OpenStream()
        {
            EnsureDataLoaded();

            return new MemoryStream(_cachedData!, writable: false);
        }

        /// <inheritdoc/>
        protected override async Task<Stream> OpenStreamAsync()
        {
            await EnsureDataLoadedAsync().ConfigureAwait(false);

            return new MemoryStream(_cachedData!, writable: false);
        }

        private void EnsureDataLoaded()
        {
            if (_cachedData != null)
                return;

            _loadLock.Wait();

            try
            {
                if (_cachedData != null)
                    return;

                ValidateUrl();

                try
                {
                    _cachedData = _httpClient
                        .GetByteArrayAsync(Url!)
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception ex)
                {
                    throw new IOException(
                        $"Failed to download data from URL: {Url}",
                        ex);
                }
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private async Task EnsureDataLoadedAsync()
        {
            if (_cachedData != null)
                return;

            await _loadLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_cachedData != null)
                    return;

                ValidateUrl();

                try
                {
                    _cachedData = await _httpClient
                        .GetByteArrayAsync(Url!)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new IOException(
                        $"Failed to download data from URL: {Url}",
                        ex);
                }
            }
            finally
            {
                _loadLock.Release();
            }
        }

        private void ValidateUrl()
        {
            if (string.IsNullOrWhiteSpace(Url))
                throw new InvalidOperationException("Url is not set.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Network
{
    public delegate bool ApiTryParseCallback<T>(string? content, out T? result);

    public abstract class ApiQuery
    {
        private static HttpClient? _client;
        private readonly string _hostingUrl; // z.B. https://www.sachssoft.com/api
        private readonly Dictionary<string, RequestTaskItem> _requestTasks = new();
        private readonly Dictionary<string, object?> _requestResults = new();

        private record RequestTaskItem(Task Task, string? UrlSource);

        public ApiQuery(string hostingUrl)
        {
            if (string.IsNullOrWhiteSpace(hostingUrl))
                throw new ArgumentException("Hosting URL cannot be null or empty.", nameof(hostingUrl));

            // Ensure valid absolute URL
            if (!Uri.TryCreate(hostingUrl.TrimEnd('/'), UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Hosting URL is not a valid HTTP or HTTPS URL.", nameof(hostingUrl));
            }

            if (_client == null)
            {
                _client = CreateClient();
                _client.DefaultRequestHeaders.UserAgent.ParseAdd(CreateUserAgent());
            }

            _hostingUrl = uriResult.ToString(); // normalized
        }

        public Action<string?>? ErrorHandler { get; set; }

        public string HostingUrl => _hostingUrl;

        protected virtual HttpClient CreateClient()
        {
            return new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        protected virtual string CreateUserAgent()
        {
            string gameName = GameAppMetadata.Product;
            string gameVersion = GameAppMetadata.InformationalVersion;

            string osPlatform;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) osPlatform = "Windows";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) osPlatform = "Linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) osPlatform = "macOS";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))) osPlatform = "Android";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))) osPlatform = "iOS";
            else osPlatform = "Unknown";

            bool isMobile = osPlatform is "Android" or "iOS";
            string deviceType = isMobile ? "Mobile" : "Desktop";

            return $"{gameName}/{gameVersion} ({osPlatform}; {deviceType})";
        }

        protected virtual string GetQuery(string key, object? args) => "";

        /// <summary>
        /// Startet eine asynchrone GET-Anfrage und speichert sie unter dem angegebenen Key.
        /// </summary>
        protected void AddRequest<T>(string key, object?[] args, ApiTryParseCallback<T> tryParseCallback, CancellationToken ct = default)
        {
            if (_client == null)
                throw new InvalidOperationException("HttpClient wurde noch nicht initialisiert.");

            string query = GetQuery(key, args);
            string url = $"{_hostingUrl}/{query}";
            Debug.WriteLine($"[ApiQuery] => AddRequest: {key} => {url}");

            var task = Task.Run(async () =>
            {
                var result = await GetApiResultAsync(url, tryParseCallback, ct).ConfigureAwait(false);
                lock (_requestResults)
                {
                    _requestResults[key] = result;
                }
            }, ct);

            lock (_requestTasks)
            {
                _requestTasks[key] = new(task, url);
            }
        }

        protected async Task<T?> GetResultAsync<T>(string key, CancellationToken ct = default)
        {
            var result = await GetRequestAsync<T>(key);

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else
            {
                ErrorHandler?.Invoke(result.ErrorMessage);
                return default(T);
            }
        }

        /// <summary>
        /// Holt das Ergebnis einer früher gestarteten Anfrage (blockierend bis abgeschlossen).
        /// </summary>
        protected async Task<ApiClientResult<T>> GetRequestAsync<T>(string key, CancellationToken ct = default)
        {
            if (!_requestTasks.TryGetValue(key, out var taskItem))
                return ApiClientResult<T>.Failure(taskItem.UrlSource, $"No taskItem found for key '{key}'", ApiErrorCode.NotFound);

            try
            {
                await taskItem.Task.WaitAsync(ct).ConfigureAwait(false);

                lock (_requestResults)
                {
                    if (_requestResults.TryGetValue(key, out var resultObj) && resultObj is ApiClientResult<T> result)
                        return result;
                }

                return ApiClientResult<T>.Failure(taskItem.UrlSource, $"Invalid result type for key '{key}'", ApiErrorCode.UnexpectedError);
            }
            catch (OperationCanceledException)
            {
                return ApiClientResult<T>.Failure(taskItem.UrlSource, "Request cancelled", ApiErrorCode.Cancelled);
            }
        }

        /// <summary>
        /// Führt den HTTP-GET-Request aus und gibt das ApiClientResult zurück.
        /// </summary>
        private async Task<ApiClientResult<T>> GetApiResultAsync<T>(
            string url,
            ApiTryParseCallback<T> tryParseCallback,
            CancellationToken ct = default)
        {
            Debug.WriteLine($"[ApiClient] => START Request to: {url}");

            try
            {
                using var response = await _client!.GetAsync(url, ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return ApiClientResult<T>.Failure(
                        url,
                        $"HTTP error: {response.StatusCode}",
                        ApiErrorCode.InternalServerError);
                }

                var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                if (tryParseCallback.Invoke(content, out var result))
                {
                    return ApiClientResult<T>.Success(url, result);
                }

                return ApiClientResult<T>.Failure(url, $"Parse error", ApiErrorCode.ParseError);
            }
            catch (TaskCanceledException)
            {
                return ApiClientResult<T>.Failure(url, "Request timeout", ApiErrorCode.Timeout);
            }
            catch (HttpRequestException e)
            {
                return ApiClientResult<T>.Failure(url, $"Network error: {e.Message}", ApiErrorCode.NetworkError);
            }
            catch (JsonException e)
            {
                return ApiClientResult<T>.Failure(url, $"JSON parse error: {e.Message}", ApiErrorCode.ParseError);
            }
            catch (Exception e)
            {
                return ApiClientResult<T>.Failure(url, $"Unexpected error: {e.Message}", ApiErrorCode.UnexpectedError);
            }
            finally
            {
                Debug.WriteLine($"[ApiClient] => END Request to: {url}");
            }
        }
    }
}

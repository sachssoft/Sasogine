using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Services.Networking;

/// <summary>
/// Provides asynchronous HTTP communication.
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// Sends an HTTP request and returns its response.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// The HTTP response, which must be disposed by the caller.
    /// </returns>
    Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default);
}
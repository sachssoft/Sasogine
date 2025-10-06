namespace Sachssoft.Sasogine.Network;

/// <summary>
/// Defines error codes that can occur during API calls.
/// </summary>
public enum ApiErrorCode
{
    /// <summary>
    /// No error, the call was successful.
    /// </summary>
    None = 0,

    /// <summary>
    /// Invalid request data or parameters (HTTP 400).
    /// </summary>
    BadRequest = 400,

    /// <summary>
    /// Unauthorized access (HTTP 401).
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Forbidden access (HTTP 403).
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Resource not found (HTTP 404).
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Internal server error (HTTP 500).
    /// </summary>
    InternalServerError = 500,

    /// <summary>
    /// An unknown error has occurred.
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// Domain-specific error, e.g. business logic or application-level error.
    /// </summary>
    DomainError = -2,

    /// <summary>
    /// Network communication error, such as connection failure or timeout.
    /// </summary>
    NetworkError = -3,

    /// <summary>
    /// Error parsing response.
    /// </summary>
    ParseError = -4,

    /// <summary>
    /// Unexpected error occurred, e.g. unhandled exceptions.
    /// </summary>
    UnexpectedError = -5,

    /// <summary>
    /// Request was canceled.
    /// </summary>
    RequestCanceled = -6,

    /// <summary>
    /// API rate limit exceeded.
    /// </summary>
    RateLimitExceeded = -7,
    Timeout = 501,
    Cancelled = 502
}

using System;

namespace Sachssoft.Sasogine.Network
{
    /// <summary>
    /// Represents the result of an API call, including the value of type <typeparamref name="T"/>,
    /// error information, and the source URL of the request.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the API call.</typeparam>
    public class ApiClientResult<T>
    {
        /// <summary>
        /// Gets the URL that the API call was made to.
        /// </summary>
        public string? UrlSource { get; }

        /// <summary>
        /// Gets a value indicating whether the API call was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the value returned by the API call if successful; otherwise, <c>null</c>.
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// Gets the error message if the API call failed; otherwise, <c>null</c>.
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Gets the standardized error code describing the failure.
        /// </summary>
        public ApiErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets a domain-specific error code providing additional details
        /// beyond the standard <see cref="ErrorCode"/> values.
        /// Returns zero if no domain-specific error applies.
        /// </summary>
        public int DomainErrorCode { get; }

        /// <summary>
        /// Gets the exception thrown during the API call, if any; otherwise, <c>null</c>.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClientResult{T}"/> class.
        /// </summary>
        /// <param name="urlSource">The URL the API call was made to.</param>
        /// <param name="isSuccess">Indicates whether the call was successful.</param>
        /// <param name="value">The value returned by the call, or <c>null</c> if unsuccessful.</param>
        /// <param name="errorMessage">The error message if the call failed.</param>
        /// <param name="errorCode">The standardized error code.</param>
        /// <param name="domainErrorCode">Optional domain-specific error code.</param>
        /// <param name="exception">Optional exception thrown during the call.</param>
        private ApiClientResult(
            string? urlSource,
            bool isSuccess,
            T? value,
            string? errorMessage,
            ApiErrorCode errorCode,
            int domainErrorCode = 0,
            Exception? exception = null)
        {
            UrlSource = urlSource;
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            DomainErrorCode = domainErrorCode;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful API result with the specified value.
        /// </summary>
        /// <param name="urlSource">The URL the API call was made to.</param>
        /// <param name="value">The value returned by the API call.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing a successful API call.</returns>
        public static ApiClientResult<T> Success(string? urlSource, T? value)
            => new ApiClientResult<T>(urlSource, true, value, null, ApiErrorCode.None);

        /// <summary>
        /// Creates a failure result with a standardized error code.
        /// </summary>
        /// <param name="urlSource">The URL the API call was made to.</param>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <param name="errorCode">The standardized error code.</param>
        /// <param name="ex">Optional exception thrown during the API call.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing a failed API call.</returns>
        public static ApiClientResult<T> Failure(string? urlSource, string errorMessage, ApiErrorCode errorCode, Exception? ex = null)
            => new ApiClientResult<T>(urlSource, false, default, errorMessage, errorCode, 0, ex);

        /// <summary>
        /// Creates a failure result with a domain-specific error code.
        /// </summary>
        /// <param name="urlSource">The URL the API call was made to.</param>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <param name="domainErrorCode">The domain-specific error code.</param>
        /// <param name="ex">Optional exception thrown during the API call.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing a failed API call.</returns>
        public static ApiClientResult<T> Failure(string? urlSource, string errorMessage, int domainErrorCode, Exception? ex = null)
            => new ApiClientResult<T>(urlSource, false, default, errorMessage, ApiErrorCode.DomainError, domainErrorCode, ex);

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the API call failed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IsSuccess"/> is <c>false</c>.</exception>
        public void ThrowIfFailure()
        {
            if (!IsSuccess)
                throw new InvalidOperationException(ErrorMessage, Exception);
        }
    }
}

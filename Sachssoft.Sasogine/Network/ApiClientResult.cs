using System;

namespace Sachssoft.Sasogine.Network
{
    /// <summary>
    /// Represents the result of an API call with a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the returned value.</typeparam>
    public class ApiClientResult<T>
    {
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
        /// Gets the error code describing the failure.
        /// </summary>
        public ApiErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets a domain-specific error code providing additional details
        /// beyond the standard <see cref="ApiErrorCode"/> values.
        /// Use this to represent application-specific or business logic errors.
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
        /// <param name="isSuccess">Indicates whether the call was successful.</param>
        /// <param name="value">The value returned by the call.</param>
        /// <param name="errorMessage">The error message if any.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="domainErrorCode">The domain-specific error code.</param>
        /// <param name="exception">The exception if any.</param>
        private ApiClientResult(
            bool isSuccess,
            T? value,
            string? errorMessage,
            ApiErrorCode errorCode,
            int domainErrorCode = 0,
            Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
            DomainErrorCode = domainErrorCode;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The successful return value.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing success.</returns>
        public static ApiClientResult<T> Success(T value)
            => new ApiClientResult<T>(true, value, null, ApiErrorCode.None);

        /// <summary>
        /// Creates a failure result with the specified error message and code.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="errorCode">The error code.</param>
        /// <param name="ex">The exception if any.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing failure.</returns>
        public static ApiClientResult<T> Failure(string errorMessage, ApiErrorCode errorCode, Exception? ex = null)
            => new ApiClientResult<T>(false, default, errorMessage, errorCode, 0, ex);

        /// <summary>
        /// Creates a failure result with a domain-specific error code and message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="domainErrorCode">The domain-specific error code.</param>
        /// <param name="ex">The exception if any.</param>
        /// <returns>An <see cref="ApiClientResult{T}"/> representing failure.</returns>
        public static ApiClientResult<T> Failure(string errorMessage, int domainErrorCode, Exception? ex = null)
            => new ApiClientResult<T>(false, default, errorMessage, ApiErrorCode.DomainError, domainErrorCode, ex);

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the result indicates failure.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IsSuccess"/> is <c>false</c>.</exception>
        public void ThrowIfFailure()
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException(ErrorMessage, Exception);
            }
        }
    }
}
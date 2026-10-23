
using System;

namespace Sachssoft.Engine.Input;

/// <summary>
/// Provides request and response communication between independent components
/// without requiring direct references between the sender and receiver.
/// </summary>
public sealed class Request
{
    private RequestHandler? _handler;

    /// <summary>
    /// Gets whether a request handler is currently registered.
    /// </summary>
    public bool CanSend => _handler is not null;

    /// <summary>
    /// Registers a handler responsible for processing requests.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another handler is already registered.
    /// </exception>
    public void Register(RequestHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (_handler is not null)
            throw new InvalidOperationException("A request handler is already registered.");

        _handler = handler;
    }

    /// <summary>
    /// Unregisters the specified handler if it is currently registered.
    /// </summary>
    /// <param name="handler">The handler to unregister.</param>
    public void Unregister(RequestHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (_handler == handler)
            _handler = null;
    }

    /// <summary>
    /// Sends a request without input data or requiring a response.
    /// </summary>
    /// <returns>True if the request was dispatched; otherwise, false.</returns>
    public bool Send() => Send(null, static _ => { });

    /// <summary>
    /// Sends a request without requiring a response.
    /// </summary>
    /// <param name="input">The request input.</param>
    /// <returns>True if the request was dispatched; otherwise, false.</returns>
    public bool Send(object? input) => Send(input, static _ => { });

    /// <summary>
    /// Sends a request without input data and receives its response.
    /// </summary>
    /// <param name="output">The callback receiving the response.</param>
    /// <returns>True if the request was dispatched; otherwise, false.</returns>
    public bool Send(Action<object?> output) => Send(null, output);

    /// <summary>
    /// Sends a request to the registered handler and provides a callback
    /// for returning its response.
    /// </summary>
    /// <param name="input">The data associated with the request.</param>
    /// <param name="output">
    /// The callback invoked when the request has been completed.
    /// The response may be delivered after this method returns.
    /// </param>
    /// <returns>
    /// True if the request was dispatched; otherwise, false.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="output"/> is null.
    /// </exception>
    public bool Send(object? input, Action<object?> output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var handler = _handler;
        if (handler is null)
            return false;

        bool completed = false;

        handler(input, response =>
        {
            if (completed)
                throw new InvalidOperationException("The request has already been completed.");

            completed = true;
            output(response);
        });

        return true;
    }
}

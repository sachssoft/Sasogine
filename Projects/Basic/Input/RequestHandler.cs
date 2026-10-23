using System;

namespace Sachssoft.Engine.Input;

/// <summary>
/// Represents a handler that processes a request and returns a response
/// through a callback.
/// </summary>
/// <param name="input">The data provided with the request.</param>
/// <param name="output">
/// The callback used to deliver the response, including responses
/// produced after the handler returns.
/// </param>
public delegate void RequestHandler(object? input, Action<object?> output);
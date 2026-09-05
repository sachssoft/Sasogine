using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Represents an exception raised by the Sasogine game framework.
/// </summary>
public class GameException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameException"/> class
    /// with the specified error message.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </param>
    public GameException(string message)
        : base(message)
    {
    }
}
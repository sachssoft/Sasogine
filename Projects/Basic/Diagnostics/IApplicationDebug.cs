namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Defines diagnostic output provided by a game application.
/// </summary>
public interface IApplicationDebug
{
    /// <summary>
    /// Flushes pending diagnostic output.
    /// </summary>
    void Flush();

    /// <summary>
    /// Clears the diagnostic output.
    /// </summary>
    void Clear();

    /// <summary>
    /// Writes diagnostic text.
    /// </summary>
    /// <param name="message">The diagnostic message.</param>
    void Write(string? message);

    /// <summary>
    /// Writes diagnostic text followed by a line terminator.
    /// </summary>
    /// <param name="message">The diagnostic message.</param>
    void WriteLine(string? message);
}
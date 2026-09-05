namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Defines a display for presenting runtime diagnostic information.
/// </summary>
public interface IDebugDisplay
{
    /// <summary>
    /// Sends diagnostic text to the display.
    /// </summary>
    /// <param name="sender">
    /// The source of the diagnostic message, or <see langword="null"/>.
    /// </param>
    /// <param name="text">
    /// The diagnostic text to display, or <see langword="null"/>.
    /// </param>
    void SendDebugText(object? sender, string? text);

    /// <summary>
    /// Updates the debug display.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current game update.
    /// </param>
    void Update(GameContext context);
}
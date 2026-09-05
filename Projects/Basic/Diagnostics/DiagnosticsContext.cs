namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Provides configuration and services used for runtime diagnostics.
/// </summary>
public sealed class DiagnosticsContext
{
    /// <summary>
    /// Gets or sets the debug display used to present diagnostic information.
    /// </summary>
    public IDebugDisplay? DebugDisplay { get; set; }

    /// <summary>
    /// Gets or sets the diagnostic information to display.
    /// </summary>
    public DiagnosticsDisplayFlags Flags { get; set; } =
        DiagnosticsDisplayFlags.FPS;
}
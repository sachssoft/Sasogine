using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Resources.Localization;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides configuration for a game application.
/// </summary>
public class GameConfiguration
{
    /// <summary>
    /// Gets the service manager used to configure application services.
    /// </summary>
    public GameServiceManager Services { get; } =
        new GameServiceManager();

    /// <summary>
    /// Gets or sets the diagnostic output used by the application.
    /// </summary>
    public IApplicationDebug? Debug { get; set; }

    /// <summary>
    /// Gets or sets the initial language used by the application.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="null"/> indicates that no initial
    /// language is explicitly configured.
    /// </remarks>
    public Language? Language { get; set; }
}
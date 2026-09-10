using Sachssoft.Sasogine.Diagnostics;

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
}
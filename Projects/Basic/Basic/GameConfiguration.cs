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
}
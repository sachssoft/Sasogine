using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Services.Platform;

/// <summary>
/// Provides initialization of a platform service after the game window becomes available.
/// </summary>
public interface IPlatformInitializable
{
    /// <summary>
    /// Initializes the service using the active game instance.
    /// </summary>
    /// <param name="game">The initialized game instance.</param>
    void Initialize(Game game);
}
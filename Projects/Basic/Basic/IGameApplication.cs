using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Defines the core contract for a Sasogine game application.
/// </summary>
public interface IGameApplication
{
    /// <summary>
    /// Gets the currently active game application instance.
    /// </summary>
    /// <remarks>
    /// This global access point is obsolete and will be removed in a future version.
    /// Application dependencies should be provided explicitly instead.
    /// </remarks>
    [Obsolete(
        "Global application access is obsolete and will be removed in a future version.")]
    public static IGameApplication Current { get; internal set; } = null!;

    /// <summary>
    /// Gets the configuration associated with the game application.
    /// </summary>
    GameConfiguration Configuration { get; }

    /// <summary>
    /// Gets the graphics device used by the application.
    /// </summary>
    GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets the localization manager used by the application.
    /// </summary>
    LocalizationManager Localization { get; }

    /// <summary>
    /// Gets the scene manager responsible for managing scenes.
    /// </summary>
    ISceneManager Scenes { get; }

    /// <summary>
    /// Gets the asset store used by the application.
    /// </summary>
    AssetStore Assets { get; }

    /// <summary>
    /// Gets the registry containing shared engine objects.
    /// </summary>
    GameRegistry Registry { get; }

    /// <summary>
    /// Gets the application settings.
    /// </summary>
    IGameSettings Settings { get; }

    /// <summary>
    /// Gets the service container associated with the application.
    /// </summary>
    GameServiceContainer Services { get; }

    /// <summary>
    /// Requests termination of the game application.
    /// </summary>
    void Exit();
}
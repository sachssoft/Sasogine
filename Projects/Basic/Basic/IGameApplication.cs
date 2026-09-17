using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Experimental;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine;

/// <summary>
/// Defines the core contract for a Sasogine game application.
/// </summary>
public interface IGameApplication
{
    /// <summary>
    /// Gets the application diagnostic output.
    /// </summary>
    IApplicationDebug Debug { get; }

    /// <summary>
    /// Gets the graphics device used by the application.
    /// </summary>
    GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets the localization manager used by the application.
    /// </summary>
    LocalizationManager Localization { get; }

    /// <summary>
    /// Gets the game activator used to create engine objects
    /// from registered definitions.
    /// </summary>
    IGameActivator Activator { get; }

    /// <summary>
    /// Gets the scene manager used by the application.
    /// </summary>
    ISceneManager Scenes { get; }

    /// <summary>
    /// Gets the asset store used by the application.
    /// </summary>
    AssetStore Assets { get; }

    /// <summary>
    /// Gets the application settings, or <see langword="null"/> when
    /// the application does not provide persistent settings.
    /// </summary>
    IGameSettings? Settings { get; }

    /// <summary>
    /// Gets the service container used by the application.
    /// </summary>
    GameServiceContainer Services { get; }

    /// <summary>
    /// Requests termination of the application.
    /// </summary>
    void Exit();
}
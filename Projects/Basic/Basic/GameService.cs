using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides helper methods for resolving services from a game service container.
/// </summary>
public static class GameService
{
    /// <summary>
    /// Attempts to retrieve a service of the specified type from the given game.
    /// </summary>
    public static T? TryGet<T>(
        Game game)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(game);

        return game.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Attempts to retrieve a service of the specified type from the given
    /// game application.
    /// </summary>
    public static T? TryGet<T>(
        IGameApplication application)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(application);

        return application.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Retrieves a required service of the specified type from the given game.
    /// </summary>
    public static T GetRequired<T>(
        Game game)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(game);

        return game.Services.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException(
                $"Service of type {typeof(T).Name} not found.");
    }

    /// <summary>
    /// Retrieves a required service of the specified type from the given
    /// game application.
    /// </summary>
    public static T GetRequired<T>(
        IGameApplication application)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(application);

        return application.Services.GetService(typeof(T)) as T
            ?? throw new InvalidOperationException(
                $"Service of type {typeof(T).Name} not found.");
    }
}
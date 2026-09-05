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
    /// <typeparam name="T">
    /// The service type to retrieve.
    /// </typeparam>
    /// <param name="game">
    /// The game instance whose service container is queried.
    /// </param>
    /// <returns>
    /// The registered service, or <see langword="null"/> if no matching
    /// service is registered.
    /// </returns>
    public static T? TryGet<T>(Game game)
        where T : class =>
        game.Services.GetService(typeof(T)) as T;

    /// <summary>
    /// Attempts to retrieve a service of the specified type from the
    /// current game application.
    /// </summary>
    /// <typeparam name="T">
    /// The service type to retrieve.
    /// </typeparam>
    /// <returns>
    /// The registered service, or <see langword="null"/> if no matching
    /// service is registered.
    /// </returns>
    public static T? TryGet<T>()
        where T : class =>
        IGameApplication.Current.Services.GetService(typeof(T)) as T;

    /// <summary>
    /// Retrieves a required service of the specified type from the given game.
    /// </summary>
    /// <typeparam name="T">
    /// The required service type.
    /// </typeparam>
    /// <param name="game">
    /// The game instance whose service container is queried.
    /// </param>
    /// <returns>
    /// The registered service.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no service of the specified type is registered.
    /// </exception>
    public static T GetRequired<T>(Game game)
        where T : class =>
        game.Services.GetService(typeof(T)) as T
        ?? throw new InvalidOperationException(
            $"Service of type {typeof(T).Name} not found.");

    /// <summary>
    /// Retrieves a required service of the specified type from the
    /// current game application.
    /// </summary>
    /// <typeparam name="T">
    /// The required service type.
    /// </typeparam>
    /// <returns>
    /// The registered service.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no service of the specified type is registered.
    /// </exception>
    public static T GetRequired<T>()
        where T : class =>
        IGameApplication.Current.Services.GetService(typeof(T)) as T
        ?? throw new InvalidOperationException(
            $"Service of type {typeof(T).Name} not found.");
}
using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine;

public static class GameService
{
    public static T? TryGet<T>(Game game) where T : class =>
        game.Services.GetService(typeof(T)) as T;

    public static T? TryGet<T>() where T : class =>
        IGameApplication.Current.Services.GetService(typeof(T)) as T;

    public static T GetRequired<T>(Game game) where T : class =>
        game.Services.GetService(typeof(T)) as T
        ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} not found.");

    public static T GetRequired<T>() where T : class =>
        IGameApplication.Current.Services.GetService(typeof(T)) as T
        ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} not found.");

}

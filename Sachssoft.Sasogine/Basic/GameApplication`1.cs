using Sachssoft.Sasogine.Basic;
using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine;

public abstract class GameApplication<TResourceManager> : GameApplication
    where TResourceManager : GameResourceManager
{
    private readonly TResourceManager _resourcesTyped;

    protected GameApplication(GameConfiguration? configuration = null, params string[] args)
        : base(configuration, args)
    {
        _resourcesTyped = (TResourceManager)_resources;
    }

    /// <summary>
    /// Typed version of Resources, guaranteed to be TResourceManager
    /// </summary>
    public new TResourceManager Resources => _resourcesTyped;

    /// <summary>
    /// Type-safe Current property
    /// </summary>
    public static new GameApplication<TResourceManager> Current =>
        IGameApplication.Current as GameApplication<TResourceManager>
        ?? throw new InvalidOperationException($"Current game is not of type {typeof(GameApplication<TResourceManager>).Name}");
}
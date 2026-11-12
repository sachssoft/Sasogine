using Sachssoft.Engine.Assets;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Localization;
using Sachssoft.Engine.Scenes;
using System;

namespace Sachssoft.Engine;

/// <summary>Describes the components created after the game constructor finishes.</summary>
/// <remarks>Disposable components supplied here are application-owned. Registered services retain their caller-defined ownership.</remarks>
public sealed class ApplicationConfiguration
{
    internal ApplicationConfiguration(GameServiceManager services) => Services = services;

    /// <summary>Gets the service manager used to register services before runtime initialization.</summary>
    public GameServiceManager Services { get; }
    /// <summary>Gets or sets a factory for a custom application context.</summary>
    public Func<GameApplicationBase, IApplicationContext>? Context { get; set; }
    /// <summary>Gets or sets a factory for a custom localization manager.</summary>
    public Func<GameApplicationBase, LocalizationManager>? Localization { get; set; }
    /// <summary>Gets or sets a callback that configures the localization manager.</summary>
    public Action<LocalizationManager>? ConfigureLocalization { get; set; }
    /// <summary>Gets or sets a custom object activator.</summary>
    public IGameActivator? Activator { get; set; }
    /// <summary>Gets or sets the registry used to create the default object activator.</summary>
    public IGameRegistry? Registry { get; set; }
    /// <summary>Gets or sets a factory for the integrated asset store.</summary>
    public Func<GameApplicationBase, AssetStore>? Assets { get; set; }
    /// <summary>Gets or sets a factory for optional persistent settings.</summary>
    public Func<GameApplicationBase, IGameSettings>? Settings { get; set; }
    /// <summary>Gets or sets a factory for the initial scene of the default scene manager.</summary>
    public Func<GameApplicationBase, IScene>? StartScene { get; set; }
    /// <summary>Gets or sets a custom scene manager factory. Specify either this or <see cref="StartScene"/>.</summary>
    public Func<GameApplicationBase, ISceneManager>? Scenes { get; set; }
}

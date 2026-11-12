using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets;
using Sachssoft.Engine.Diagnostics;
using Sachssoft.Engine.Diagnostics.Internal;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Localization;
using Sachssoft.Engine.Scenes;
using Sachssoft.Engine.Services.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine;

/// <summary>
/// Provides the base implementation for a Sasogine game application.
/// </summary>
/// <remarks>
/// Manages the core application services, graphics device, object activation,
/// assets, localization, settings, scenes, and application lifecycle.
/// </remarks>
public abstract class GameApplicationBase : Game, IServiceProvider
{
    private readonly string[] _commandArgs;
    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    private readonly IApplicationDebug _applicationDebug;
    private readonly List<IDisposable> _ownedComponents = new();
    private IApplicationContext? _context;
    private LocalizationManager? _localization;
    private IGameActivator? _activator;
    private AssetStore? _integratedAssets;
    private ISceneManager? _scenes;
    private IGameSettings? _settings;
    private bool _initializationStarted;
    private bool _settingsLoaded;
    private bool _assetsNeedUnload;
    private bool _scenesNeedUnload;
    private bool _shutdown;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameApplicationBase"/> class
    /// using the default configuration.
    /// </summary>
    public GameApplicationBase()
        : this(null, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameApplicationBase"/> class
    /// with the specified command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public GameApplicationBase(params string[] args)
        : this(null, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameApplicationBase"/> class.
    /// </summary>
    /// <param name="configuration">The optional application configuration.</param>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public GameApplicationBase(GameConfiguration? configuration, params string[] args)
    {
        Configuration = configuration ?? new GameConfiguration();
        _commandArgs = args ?? [];

        _applicationDebug = Configuration.Debug
            ?? CreateDefaultApplicationDebug();

        Content.RootDirectory = Configuration.ContentRootDirectory;
        _graphicsDeviceManager = new GraphicsDeviceManager(this);
        ApplyGraphicsConfiguration(Configuration.Graphics);
        Services.AddService(typeof(GraphicsDeviceManager), _graphicsDeviceManager);
    }

    /// <summary>
    /// Gets the configuration used by the application.
    /// </summary>
    protected GameConfiguration Configuration { get; }

    /// <summary>
    /// Gets the application context.
    /// </summary>
    /// <exception cref="InvalidOperationException">The context has not been created yet.</exception>
    public IApplicationContext Context => _context ?? throw ComponentUnavailable(nameof(Context));

    /// <summary>
    /// Gets the application diagnostic output.
    /// </summary>
    public IApplicationDebug Debug => _applicationDebug;

    /// <summary>
    /// Gets the localization manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">The localization manager has not been created yet.</exception>
    public LocalizationManager Localization => _localization ?? throw ComponentUnavailable(nameof(Localization));

    /// <summary>
    /// Gets the game activator used to create engine objects from registered definitions.
    /// </summary>
    /// <exception cref="InvalidOperationException">The activator has not been created yet.</exception>
    public IGameActivator Activator => _activator ?? throw ComponentUnavailable(nameof(Activator));

    /// <summary>
    /// Gets the scene manager.
    /// </summary>
    /// <exception cref="InvalidOperationException">The scene manager has not been created yet.</exception>
    public ISceneManager Scenes => _scenes ?? throw ComponentUnavailable(nameof(Scenes));

    /// <summary>
    /// Gets the integrated asset store provided by the application.
    /// </summary>
    /// <exception cref="InvalidOperationException">The asset store has not been created yet.</exception>
    public AssetStore IntegratedAssets => _integratedAssets ?? throw ComponentUnavailable(nameof(IntegratedAssets));

    /// <summary>
    /// Gets the application settings.
    /// </summary>
    public IGameSettings? Settings => _settings;

    /// <summary>
    /// Gets the base directory of the running application.
    /// </summary>
    public string CurrentDirectory => AppContext.BaseDirectory;

    /// <summary>
    /// Gets or sets a value indicating whether the application is running in debug mode.
    /// </summary>
    public bool IsDebugMode { get; set; } = true;

    /// <summary>
    /// Gets the command-line arguments supplied to the application.
    /// </summary>
    public string[] CommandArgs => _commandArgs;

    /// <summary>
    /// Gets the assembly associated with the application.
    /// </summary>
    public virtual Assembly Assembly =>
        Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    /// <summary>
    /// Initializes the application and its core services.
    /// </summary>
    protected override void Initialize()
    {
        if (_initializationStarted || _shutdown)
            throw new InvalidOperationException("Application initialization can only start once.");

        _initializationStarted = true;
        var configuration = new ApplicationConfiguration(Configuration.Services);
        try
        {
            IsFixedTimeStep = false;
            IsMouseVisible = true;

            ConfigureApplication(configuration);
            if (configuration.Scenes is not null && configuration.StartScene is not null)
                throw new GameException("Configure either Scenes or StartScene, not both.");
            if (configuration.Activator is not null && configuration.Registry is not null)
                throw new GameException("Configure either Activator or Registry, not both.");

            Configuration.Services.Lock();
            _context = Own(configuration.Context is { } context
                ? CreateComponent(context, nameof(configuration.Context))
                : new ApplicationContext(this));
            LocalizationManager localizationManager = Own(configuration.Localization is { } localization
                ? CreateComponent(localization, nameof(configuration.Localization))
                : new LocalizationManager());
            _localization = localizationManager;
            localizationManager.CurrentLanguage = Configuration.Language ?? localizationManager.FallbackLanguage;
            configuration.ConfigureLocalization?.Invoke(localizationManager);
            _activator = Own(configuration.Activator ?? (configuration.Registry is { } registry
                ? (IGameActivator)new GameActivator(Own(registry))
                : EmptyGameActivator.Instance));

            _integratedAssets = Own(configuration.Assets is { } assets
                ? CreateComponent(assets, nameof(configuration.Assets))
                : new AssetStore(this));
            _assetsNeedUnload = true;
            if (configuration.Settings is { } settings)
                _settings = Own(CreateComponent(settings, nameof(configuration.Settings)));

            Configuration.Services.ForEachOnce<IPlatformInitializable>(service => service.Initialize(this));
            IntegratedAssets.Initialize(Context);
            localizationManager.Close();
            _scenes = Own(configuration.Scenes is { } scenes
                ? CreateComponent(scenes, nameof(configuration.Scenes))
                : new SceneManager(this, configuration.StartScene is { } start
                    ? Own(CreateComponent(start, nameof(configuration.StartScene)))
                    : new EmptyApplicationScene()));

            if (Window is not null)
                RegisterWindowEvents();
            base.Initialize();
        }
        catch (Exception error)
        {
            try
            {
                if (configuration.Activator is { } activator)
                    Own(activator);
                if (configuration.Registry is { } registry)
                    Own(registry);
                Shutdown(saveSettings: false);
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Application startup and cleanup failed.", error, cleanupError);
            }
            throw;
        }
    }

    /// <summary>
    /// Configures runtime components once, after the derived application constructor has completed.
    /// </summary>
    /// <param name="configuration">The runtime component and service configuration.</param>
    protected virtual void ConfigureApplication(ApplicationConfiguration configuration)
    {
    }

    private void ApplyGraphicsConfiguration(GraphicsConfiguration graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        if (graphics.Width is <= 0 || graphics.Height is <= 0)
            throw new ArgumentOutOfRangeException(nameof(graphics), "Back-buffer dimensions must be positive.");

        var manager = _graphicsDeviceManager;
        int width = manager.PreferredBackBufferWidth;
        int height = manager.PreferredBackBufferHeight;
        if (graphics.UseDisplayResolution && (graphics.Width is null || graphics.Height is null))
        {
            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            width = display.Width;
            height = display.Height;
            const DisplayOrientation landscape = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            if (OperatingSystem.IsAndroid() && graphics.SupportedOrientations is { } orientation &&
                (orientation & landscape) != 0 && (orientation & ~landscape) == 0 && height > width)
                (width, height) = (height, width);
        }

        manager.PreferredBackBufferWidth = graphics.Width ?? width;
        manager.PreferredBackBufferHeight = graphics.Height ?? height;
        manager.IsFullScreen = graphics.FullScreen ?? manager.IsFullScreen;
        manager.HardwareModeSwitch = graphics.HardwareModeSwitch ?? manager.HardwareModeSwitch;
        manager.PreferMultiSampling = graphics.MultiSampling ?? manager.PreferMultiSampling;
        manager.GraphicsProfile = graphics.Profile ?? manager.GraphicsProfile;
        manager.PreferredDepthStencilFormat = graphics.DepthStencilFormat ?? manager.PreferredDepthStencilFormat;
        manager.SynchronizeWithVerticalRetrace = graphics.SynchronizeWithVerticalRetrace ?? manager.SynchronizeWithVerticalRetrace;
        manager.SupportedOrientations = graphics.SupportedOrientations ?? manager.SupportedOrientations;
        manager.PreferHalfPixelOffset = graphics.PreferHalfPixelOffset ?? manager.PreferHalfPixelOffset;
    }

    private T CreateComponent<T>(Func<GameApplicationBase, T> factory, string name) where T : class =>
        factory(this) ?? throw new GameException($"The {name} factory returned null.");

    private T Own<T>(T component) where T : class
    {
        if (component is IDisposable disposable && !_ownedComponents.Any(existing => ReferenceEquals(existing, disposable)))
            _ownedComponents.Add(disposable);
        return component;
    }

    private static InvalidOperationException ComponentUnavailable(string name) =>
        new($"Application component '{name}' is unavailable at this stage of initialization.");

    /// <summary>
    /// Changes the preferred application resolution.
    /// </summary>
    /// <param name="width">The preferred back-buffer width.</param>
    /// <param name="height">The preferred back-buffer height.</param>
    /// <param name="fullscreen">Whether fullscreen mode should be enabled.</param>
    public void ChangeResolution(int width, int height, bool fullscreen = false)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (_graphicsDeviceManager.PreferredBackBufferWidth == width &&
            _graphicsDeviceManager.PreferredBackBufferHeight == height &&
            _graphicsDeviceManager.IsFullScreen == fullscreen)
        {
            return;
        }

        _graphicsDeviceManager.PreferredBackBufferWidth = width;
        _graphicsDeviceManager.PreferredBackBufferHeight = height;
        _graphicsDeviceManager.IsFullScreen = fullscreen;
        if (_graphicsDeviceManager.GraphicsDevice is not null)
            _graphicsDeviceManager.ApplyChanges();
    }

    /// <summary>
    /// Resolves a service registered with the application.
    /// </summary>
    /// <param name="serviceType">The type of service to resolve.</param>
    /// <returns>The resolved service, or <see langword="null"/> if unavailable.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="serviceType"/> is <see langword="null"/>.
    /// </exception>
    public object? GetService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        return Configuration.Services.GetService(serviceType)
            ?? Services.GetService(serviceType);
    }

    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <returns>The resolved service, or <see langword="null"/> if unavailable.</returns>
    public T? GetService<T>() where T : class
    {
        return GetService(typeof(T)) as T;
    }

    /// <summary>
    /// Attempts to resolve a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of service to resolve.</typeparam>
    /// <param name="service">
    /// Receives the resolved service, or <see langword="null"/> if unavailable.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the service was resolved successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetService<T>(out T? service) where T : class
    {
        service = GetService<T>();
        return service is not null;
    }

    /// <summary>
    /// Loads application settings, assets, and scenes.
    /// </summary>
    protected override void LoadContent()
    {
        _settings?.Load();
        _settingsLoaded = _settings is not null;
        _assetsNeedUnload = true;
        IntegratedAssets.LoadAll();
        _scenesNeedUnload = true;
        Scenes.Load();
    }

    /// <summary>
    /// Unloads application assets.
    /// </summary>
    protected override void UnloadContent()
    {
        try
        {
            ReleaseContent(saveSettings: true);
        }
        finally
        {
            base.UnloadContent();
        }
    }

    /// <summary>
    /// Updates the current scene.
    /// </summary>
    /// <param name="gameTime">Provides timing information for the current update.</param>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_scenes is { IsLoaded: true } scenes)
            scenes.Update(gameTime);
    }

    /// <summary>
    /// Draws the current scene.
    /// </summary>
    /// <param name="gameTime">Provides timing information for the current frame.</param>
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (_scenes is { IsLoaded: true } scenes)
            scenes.Draw(gameTime);
    }

    /// <summary>
    /// Handles application activation.
    /// </summary>
    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);

        if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IClientActivator activator)
            activator.OnClientActivate();
    }

    /// <summary>
    /// Handles application deactivation.
    /// </summary>
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IClientActivator activator)
            activator.OnClientDeactivate();
    }

    /// <summary>
    /// Handles application exit.
    /// </summary>
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        base.OnExiting(sender, args);
        if (args.Cancel)
            return;

        try
        {
            if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IApplicationExitAware exitAware)
                exitAware.OnApplicationExited();
        }
        finally
        {
            ReleaseContent(saveSettings: true);
        }
    }

    /// <summary>Releases Sasogine components before MonoGame disposes the graphics device.</summary>
    /// <param name="disposing">Whether managed resources can be released.</param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
                Shutdown(saveSettings: true);
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    private void Shutdown(bool saveSettings)
    {
        if (_shutdown)
            return;

        _shutdown = true;
        List<Exception>? failures = null;
        AttemptCleanup(() => ReleaseContent(saveSettings), ref failures);
        for (int i = _ownedComponents.Count - 1; i >= 0; i--)
            AttemptCleanup(_ownedComponents[i].Dispose, ref failures);
        _ownedComponents.Clear();
        if (failures is not null)
            throw new AggregateException("Application cleanup failed.", failures);
    }

    private void ReleaseContent(bool saveSettings)
    {
        List<Exception>? failures = null;
        if (_scenesNeedUnload)
        {
            _scenesNeedUnload = false;
            AttemptCleanup(Scenes.Unload, ref failures);
        }
        if (_settingsLoaded)
        {
            _settingsLoaded = false;
            if (saveSettings && _settings is { } settings)
                AttemptCleanup(settings.Save, ref failures);
        }
        if (_assetsNeedUnload)
        {
            _assetsNeedUnload = false;
            AttemptCleanup(IntegratedAssets.UnloadAll, ref failures);
        }
        if (failures is not null)
            throw new AggregateException("Application content could not be fully released.", failures);
    }

    private static void AttemptCleanup(Action cleanup, ref List<Exception>? failures)
    {
        try
        {
            cleanup();
        }
        catch (Exception error)
        {
            (failures ??= new()).Add(error);
        }
    }

    private void RegisterWindowEvents()
    {
        Window.FileDrop += Window_FileDrop;
        Window.ClientSizeChanged += Window_ClientSizeChanged;
        Window.OrientationChanged += Window_OrientationChanged;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            RegisterDesktopWindowInput();
    }

    private void Window_FileDrop(object? sender, FileDropEventArgs e)
    {
        if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IClientFileDropReceiver receiver)
            receiver.OnFileDrop(e.Files);
    }

    private void Window_ClientSizeChanged(object? sender, EventArgs e)
    {
        if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnClientSizeChanged();
    }

    private void Window_OrientationChanged(object? sender, EventArgs e)
    {
        if (_scenes is { IsLoaded: true } scenes && scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnOrientationChanged();
    }

    /// <summary>Registers desktop-only input without resolving its backend-specific argument types on mobile.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RegisterDesktopWindowInput()
    {
        new DesktopWindowInput(this).Attach();
    }

    private static IApplicationDebug CreateDefaultApplicationDebug()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new DesktopApplicationDebug();
        }

        return new NullApplicationDebug();
    }
}

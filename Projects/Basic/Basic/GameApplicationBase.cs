using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Diagnostics;
using Sachssoft.Engine.Diagnostics.Internals;
using Sachssoft.Engine.Resources;
using Sachssoft.Engine.Resources.Localization;
using Sachssoft.Engine.Scenes;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine;

/// <summary>
/// Provides the base implementation for a Sasogine game application.
/// </summary>
/// <remarks>
/// Manages the core application services, graphics device, object activation,
/// assets, localization, settings, scenes, and application lifecycle.
/// </remarks>
public abstract class GameApplicationBase : Game/*, IGameApplication*/
{
    private readonly string[] _commandArgs;
    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    private protected readonly IApplicationContext _context;
    private protected readonly IApplicationDebug _applicationDebug;
    private protected readonly LocalizationManager _localization;
    private protected readonly IGameActivator _activator;
    private protected readonly AssetStore _assets;
    private protected readonly ISceneManager _scenes;
    private protected readonly IGameSettings? _settings;

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
    /// <exception cref="GameException">
    /// Thrown when a required engine service cannot be created.
    /// </exception>
    public GameApplicationBase(GameConfiguration? configuration, params string[] args)
    {
        Configuration = configuration ?? new GameConfiguration();
        _commandArgs = args ?? [];

        _context = CreateContext()
            ?? throw new GameException("Application context creation failed.");

        _applicationDebug = Configuration.Debug
            ?? CreateDefaultApplicationDebug();

        _localization = CreateLocalization()
            ?? throw new GameException("Localization manager creation failed.");

        _localization.CurrentLanguage = Configuration.Language;
        ConfigureLocalization(_localization, Configuration);

        IGameRegistry? registry = CreateRegistry(Configuration);
        _activator = registry is not null
            ? new GameActivator(registry)
            : EmptyGameActivator.Instance;

        _assets = CreateAssets(Configuration) ?? new AssetStore(this);
        _settings = CreateSettings(Configuration);

        _scenes = CreateScenes(Configuration)
            ?? throw new GameException("Scene manager creation failed.");

        _graphicsDeviceManager = ConfigureGraphicsDevice();

        Services.AddService(typeof(GraphicsDeviceManager), _graphicsDeviceManager);
        _graphicsDeviceManager.ApplyChanges();
    }

    /// <summary>
    /// Gets the configuration used by the application.
    /// </summary>
    protected GameConfiguration Configuration { get; }

    /// <summary>
    /// Gets the application context.
    /// </summary>
    public IApplicationContext Context => _context;

    /// <summary>
    /// Gets the application diagnostic output.
    /// </summary>
    public IApplicationDebug Debug => _applicationDebug;

    /// <summary>
    /// Gets the localization manager.
    /// </summary>
    public LocalizationManager Localization => _localization;

    /// <summary>
    /// Gets the game activator used to create engine objects from registered definitions.
    /// </summary>
    public IGameActivator Activator => _activator;

    /// <summary>
    /// Gets the scene manager.
    /// </summary>
    public ISceneManager Scenes => _scenes;

    /// <summary>
    /// Gets the asset store.
    /// </summary>
    public AssetStore Assets => _assets;

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
        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        if (Window is not null)
            RegisterWindowEvents();

        _assets.Initialize(_context);
        _localization.Close();

        base.Initialize();
    }

    /// <summary>
    /// Creates and configures the graphics device manager.
    /// </summary>
    /// <returns>The configured graphics device manager.</returns>
    protected virtual GraphicsDeviceManager ConfigureGraphicsDevice()
    {
        return new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
            HardwareModeSwitch = true,
            PreferMultiSampling = true,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            SupportedOrientations =
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
    }

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
        _graphicsDeviceManager.ApplyChanges();
    }

    /// <summary>
    /// Loads application settings, assets, and scenes.
    /// </summary>
    protected override void LoadContent()
    {
        _settings?.Load();
        _assets.LoadAll();
        _scenes.Load();
    }

    /// <summary>
    /// Unloads application assets.
    /// </summary>
    protected override void UnloadContent()
    {
        _assets.UnloadAll();
        base.UnloadContent();
    }

    /// <summary>
    /// Updates the current scene.
    /// </summary>
    /// <param name="gameTime">Provides timing information for the current update.</param>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Update(gameTime);
    }

    /// <summary>
    /// Draws the current scene.
    /// </summary>
    /// <param name="gameTime">Provides timing information for the current frame.</param>
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Draw(gameTime);
    }

    /// <summary>
    /// Handles application activation.
    /// </summary>
    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);

        if (_scenes.IsLoaded && _scenes.CurrentScene is IClientActivator activator)
            activator.OnClientActivate();
    }

    /// <summary>
    /// Handles application deactivation.
    /// </summary>
    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (_scenes.IsLoaded && _scenes.CurrentScene is IClientActivator activator)
            activator.OnClientDeactivate();
    }

    /// <summary>
    /// Handles application exit.
    /// </summary>
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        if (_scenes.IsLoaded)
        {
            if (_scenes.CurrentScene is IApplicationExitAware exitAware)
                exitAware.OnApplicationExited();

            foreach (var scene in _scenes.ActiveScenes.ToArray())
                scene.Unload();
        }

        _settings?.Save();

        base.OnExiting(sender, args);
    }

    /// <summary>
    /// Creates the application context used by engine components.
    /// </summary>
    /// <returns>The application context associated with this application.</returns>
    protected virtual IApplicationContext CreateContext()
    {
        return new ApplicationContext(this);
    }

    /// <summary>
    /// Creates the localization manager used by the application.
    /// </summary>
    /// <returns>The localization manager.</returns>
    protected virtual LocalizationManager CreateLocalization()
    {
        return new LocalizationManager();
    }

    /// <summary>
    /// Configures localization for the application.
    /// </summary>
    /// <param name="localization">The localization manager to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    protected virtual void ConfigureLocalization(
        LocalizationManager localization,
        GameConfiguration configuration)
    {
    }

    /// <summary>
    /// Creates the game registry used by the application.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The game registry, or <see langword="null"/> when none is provided.</returns>
    protected virtual IGameRegistry? CreateRegistry(GameConfiguration configuration) =>
        null;

    /// <summary>
    /// Creates the asset store.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The asset store, or <see langword="null"/> to use the default.</returns>
    protected virtual AssetStore? CreateAssets(GameConfiguration configuration) =>
        null;

    /// <summary>
    /// Creates the scene manager.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The scene manager.</returns>
    protected abstract ISceneManager CreateScenes(GameConfiguration configuration);

    /// <summary>
    /// Creates the application settings.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>
    /// The settings implementation, or <see langword="null"/> when none is provided.
    /// </returns>
    protected virtual IGameSettings? CreateSettings(GameConfiguration configuration) =>
        null;

    private void RegisterWindowEvents()
    {
        Window.FileDrop += Window_FileDrop;
        Window.ClientSizeChanged += Window_ClientSizeChanged;
        Window.OrientationChanged += Window_OrientationChanged;
        Window.KeyDown += Window_KeyDown;
        Window.KeyUp += Window_KeyUp;
        Window.TextInput += Window_TextInput;
    }

    private void Window_FileDrop(object? sender, FileDropEventArgs e)
    {
        if (_scenes.CurrentScene is IClientFileDropReceiver receiver)
            receiver.OnFileDrop(e.Files);
    }

    private void Window_ClientSizeChanged(object? sender, EventArgs e)
    {
        if (_scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnClientSizeChanged();
    }

    private void Window_OrientationChanged(object? sender, EventArgs e)
    {
        if (_scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnOrientationChanged();
    }

    private void Window_KeyUp(object? sender, InputKeyEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyUp(e.Key);
    }

    private void Window_KeyDown(object? sender, InputKeyEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyDown(e.Key);
    }

    private void Window_TextInput(object? sender, TextInputEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnTextInput(e.Character);
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
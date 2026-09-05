using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Reflection;
using System.Linq;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides the base implementation for a Sasogine game application.
/// </summary>
/// <remarks>
/// Manages the core application services, graphics device,
/// assets, localization, settings, scenes, and application lifecycle.
/// </remarks>
public abstract class GameApplicationBase : Game, IGameApplication
{
    private readonly string[] _commandArgs;
    private readonly GameServiceManager _services;

    protected private readonly LocalizationManager _localization;
    protected private readonly GameRegistry _registry;
    protected private readonly AssetStore _assets;
    protected private readonly ISceneManager _scenes;
    protected private readonly IGameSettings? _settings;

    private readonly GraphicsDeviceManager _graphicsDeviceManager;

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
    /// <param name="args">
    /// The command-line arguments passed to the application.
    /// </param>
    public GameApplicationBase(params string[] args)
        : this(null, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GameApplicationBase"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The optional application configuration.
    /// </param>
    /// <param name="args">
    /// The command-line arguments passed to the application.
    /// </param>
    /// <exception cref="GameException">
    /// Thrown when another game application has already been initialized,
    /// or when a required engine service cannot be created.
    /// </exception>
    public GameApplicationBase(
        GameConfiguration? configuration,
        params string[] args)
    {
        if (IGameApplication.Current != null)
            throw new GameException("Game already was started.");

        Configuration =
            configuration ?? new GameConfiguration();

        _commandArgs = args ?? [];
        _localization = new LocalizationManager(this);

        _registry =
            CreateRegistry(Configuration) ??
            throw new GameException("Registry creation failed.");

        _assets =
            CreateAssets(Configuration) ??
            new AssetStore(this);

        _settings = CreateSettings(Configuration);

        _scenes =
            CreateScenes(Configuration) ??
            throw new GameException("Scene manager creation failed.");

        _graphicsDeviceManager = ConfigureGraphicsDevice();

        Services.AddService(
            typeof(GraphicsDeviceManager),
            _graphicsDeviceManager);

        _graphicsDeviceManager.ApplyChanges();

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        IGameApplication.Current = this;
    }

    /// <summary>
    /// Gets the configuration used by the application.
    /// </summary>
    public GameConfiguration Configuration { get; }

    /// <summary>
    /// Gets the localization manager.
    /// </summary>
    public LocalizationManager Localization =>
        _localization;

    /// <summary>
    /// Gets the game registry.
    /// </summary>
    public GameRegistry Registry =>
        _registry;

    /// <summary>
    /// Gets the scene manager.
    /// </summary>
    public ISceneManager Scenes =>
        _scenes;

    /// <summary>
    /// Gets the asset store.
    /// </summary>
    public AssetStore Assets =>
        _assets;

    /// <summary>
    /// Gets the application settings.
    /// </summary>
    public IGameSettings? Settings =>
        _settings;

    /// <summary>
    /// Gets the base directory of the running application.
    /// </summary>
    public string CurrentDirectory =>
        AppContext.BaseDirectory;

    /// <summary>
    /// Gets or sets a value indicating whether the application
    /// is running in debug mode.
    /// </summary>
    public bool IsDebugMode { get; set; } = true;

    /// <summary>
    /// Gets the command-line arguments supplied to the application.
    /// </summary>
    public string[] CommandArgs =>
        _commandArgs;

    /// <summary>
    /// Gets the current <see cref="GameApplicationBase"/> instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current application has not been initialized
    /// as a <see cref="GameApplicationBase"/>.
    /// </exception>
    public static GameApplicationBase Current =>
        IGameApplication.Current as GameApplicationBase
        ?? throw new InvalidOperationException(
            "GameApplicationBase not initialized.");

    /// <summary>
    /// Gets the assembly associated with the application.
    /// </summary>
    public virtual Assembly Assembly =>
        Assembly.GetEntryAssembly() ??
        Assembly.GetExecutingAssembly();

    /// <summary>
    /// Initializes the application and its core services.
    /// </summary>
    protected override void Initialize()
    {
        if (Window != null)
            RegisterWindowEvents();

        _assets.Initialize();

        base.Initialize();
    }

    /// <summary>
    /// Registers handlers for application window events.
    /// </summary>
    private void RegisterWindowEvents()
    {
        Window.FileDrop += Window_FileDrop;
        Window.ClientSizeChanged += Window_ClientSizeChanged;
        Window.OrientationChanged += Window_OrientationChanged;
        Window.KeyDown += Window_KeyDown;
        Window.KeyUp += Window_KeyUp;
        Window.TextInput += Window_TextInput;
    }

    /// <summary>
    /// Creates and configures the graphics device manager.
    /// </summary>
    /// <returns>
    /// The configured graphics device manager.
    /// </returns>
    protected virtual GraphicsDeviceManager ConfigureGraphicsDevice()
    {
        return new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,

            PreferredBackBufferHeight =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,

            HardwareModeSwitch = true,
            PreferMultiSampling = true,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferredDepthStencilFormat =
                DepthFormat.Depth24Stencil8,

            SupportedOrientations =
                DisplayOrientation.LandscapeLeft |
                DisplayOrientation.LandscapeRight
        };
    }

    /// <summary>
    /// Changes the preferred application resolution.
    /// </summary>
    /// <param name="width">
    /// The preferred back-buffer width.
    /// </param>
    /// <param name="height">
    /// The preferred back-buffer height.
    /// </param>
    /// <param name="fullscreen">
    /// A value indicating whether fullscreen mode should be enabled.
    /// </param>
    public void ChangeResolution(
        int width,
        int height,
        bool fullscreen = false)
    {
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
        _assets.Load();

        _scenes.Load();
    }

    /// <summary>
    /// Unloads application assets.
    /// </summary>
    protected override void UnloadContent()
    {
        _assets.Unload();

        base.UnloadContent();
    }

    /// <summary>
    /// Updates the current scene.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current update.
    /// </param>
    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Update(gameTime);
    }

    /// <summary>
    /// Draws the current scene.
    /// </summary>
    /// <param name="gameTime">
    /// Provides timing information for the current frame.
    /// </param>
    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Draw(gameTime);
    }

    /// <summary>
    /// Handles application activation.
    /// </summary>
    protected override void OnActivated(
        object sender,
        EventArgs args)
    {
        base.OnActivated(sender, args);

        if (_scenes.IsLoaded &&
            _scenes.CurrentScene is IClientActivator activator)
        {
            activator.OnClientActivate();
        }
    }

    /// <summary>
    /// Handles application deactivation.
    /// </summary>
    protected override void OnDeactivated(
        object sender,
        EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (_scenes.IsLoaded &&
            _scenes.CurrentScene is IClientActivator activator)
        {
            activator.OnClientDeactivate();
        }
    }

    /// <summary>
    /// Handles application exit.
    /// </summary>
    protected override void OnExiting(
        object sender,
        ExitingEventArgs args)
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
    /// Creates the game registry.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <returns>
    /// The created game registry.
    /// </returns>
    protected virtual GameRegistry CreateRegistry(
        GameConfiguration configuration) =>
        new GameRegistry();

    /// <summary>
    /// Creates the asset store.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <returns>
    /// The created asset store, or <see langword="null"/> to use
    /// the default implementation.
    /// </returns>
    protected virtual AssetStore? CreateAssets(
        GameConfiguration configuration) =>
        null;

    /// <summary>
    /// Creates the scene manager.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <returns>
    /// The scene manager used by the application.
    /// </returns>
    protected abstract ISceneManager CreateScenes(
        GameConfiguration configuration);

    /// <summary>
    /// Creates the application settings.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration.
    /// </param>
    /// <returns>
    /// The settings implementation, or <see langword="null"/> when
    /// the application does not provide persistent settings.
    /// </returns>
    protected virtual IGameSettings? CreateSettings(
        GameConfiguration configuration) =>
        null;

    private void Window_FileDrop(
        object? sender,
        FileDropEventArgs e)
    {
        if (_scenes.CurrentScene is IClientFileDropReceiver receiver)
            receiver.OnFileDrop(e.Files);
    }

    private void Window_ClientSizeChanged(
        object? sender,
        EventArgs e)
    {
        if (_scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnClientSizeChanged();
    }

    private void Window_OrientationChanged(
        object? sender,
        EventArgs e)
    {
        if (_scenes.CurrentScene is IClientResizeAware resizeAware)
            resizeAware.OnOrientationChanged();
    }

    private void Window_KeyUp(
        object? sender,
        InputKeyEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyUp(e.Key);
    }

    private void Window_KeyDown(
        object? sender,
        InputKeyEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyDown(e.Key);
    }

    private void Window_TextInput(
        object? sender,
        TextInputEventArgs e)
    {
        if (_scenes.CurrentScene is IClientKeyboardInput input)
            input.OnTextInput(e.Character);
    }
}
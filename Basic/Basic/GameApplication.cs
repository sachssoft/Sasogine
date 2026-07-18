using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Sasogine;

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

    public GameApplicationBase() : this(null, [])
    {
    }

    public GameApplicationBase(params string[] args) : this(null, args)
    {
    }

    public GameApplicationBase(GameConfiguration? configuration, params string[] args)
    {
        if (IGameApplication.Current != null)
            throw new GameException("Game already was started.");

        // Optional Configuration: default erstellen, falls null
        Configuration = configuration ?? new GameConfiguration();

        _commandArgs = args ?? new string[0];
        _localization = new LocalizationManager(this);
        _registry = CreateRegistry(Configuration) ?? throw new GameException("Registry creation failed.");
        _assets = CreateAssets(Configuration) ?? new AssetStore(this);
        _settings = CreateSettings(Configuration);
        _scenes = CreateScenes(Configuration) ?? throw new GameException("Scene manager creation failed.");

        _graphicsDeviceManager = ConfigureGraphicsDevice();
        // Monogame Services (ab Version 2 wird Monogame losgelöst)
        Services.AddService(typeof(GraphicsDeviceManager), _graphicsDeviceManager);
        _graphicsDeviceManager.ApplyChanges();

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        IGameApplication.Current = this;
    }

    public GameConfiguration Configuration { get; }

    public LocalizationManager Localization => _localization;

    public GameRegistry Registry => _registry;

    public ISceneManager Scenes => _scenes;

    public AssetStore Assets => _assets;

    public IGameSettings? Settings => _settings;

    public string CurrentDirectory => AppContext.BaseDirectory;

    public bool IsDebugMode { get; set; } = true;

    public string[] CommandArgs => _commandArgs;

    public static GameApplicationBase Current =>
        IGameApplication.Current as GameApplicationBase
        ?? throw new InvalidOperationException("GameApplicationBase not initialized.");

    public virtual Assembly Assembly =>
        Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    protected override void Initialize()
    {
        if (Window != null)
            RegisterWindowEvents();

        _assets.Initialize();

        base.Initialize();
    }

    private void RegisterWindowEvents()
    {
        Window.FileDrop += Window_FileDrop;
        Window.ClientSizeChanged += Window_ClientSizeChanged;
        Window.OrientationChanged += Window_OrientationChanged;
        Window.KeyDown += Window_KeyDown;
        Window.KeyUp += Window_KeyUp;
        Window.TextInput += Window_TextInput;
    }

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
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
    }

    public void ChangeResolution(int width, int height, bool fullscreen = false)
    {
        if (_graphicsDeviceManager.PreferredBackBufferWidth == width &&
            _graphicsDeviceManager.PreferredBackBufferHeight == height &&
            _graphicsDeviceManager.IsFullScreen == fullscreen)
            return;

        _graphicsDeviceManager.PreferredBackBufferWidth = width;
        _graphicsDeviceManager.PreferredBackBufferHeight = height;
        _graphicsDeviceManager.IsFullScreen = fullscreen;

        _graphicsDeviceManager.ApplyChanges();
    }

    protected override void LoadContent()
    {
        _settings?.Load();
        _assets.Load();

        // SceneManager ist allein verantwortlich für Scene-Loading
        _scenes.Load();
    }

    protected override void UnloadContent()
    {
        _assets.Unload();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (_scenes.IsLoaded)
            _scenes.Draw(gameTime);
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);

        if (_scenes.IsLoaded && _scenes.CurrentScene is IClientActivator activator)
            activator.OnClientActivate();
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (_scenes.IsLoaded && _scenes.CurrentScene is IClientActivator activator)
            activator.OnClientDeactivate();
    }

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

    protected virtual GameRegistry CreateRegistry(GameConfiguration configuration) => new GameRegistry();

    protected virtual AssetStore? CreateAssets(GameConfiguration configuration) => null;

    protected abstract ISceneManager CreateScenes(GameConfiguration configuration);

    protected virtual IGameSettings? CreateSettings(GameConfiguration configuration) => null;

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
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Basic;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Sasogine;

public abstract class GameApplication : Game, IGameApplication
{
    protected private readonly LocalizationManager _localization;
    protected private readonly GameRegistry _registry;
    protected private readonly GameResourceManager _resources;
    protected private readonly ISceneManager _scenes;
    protected private readonly IGameSettings? _settings;

    private readonly GraphicsDeviceManager _graphicsDeviceManager;

    public GameApplication() : this(null, [])
    {
    }

    public GameApplication(params string[] args) : this(null, args)
    { 
    }

    public GameApplication(GameConfiguration? configuration, params string[] args)
    {
        if (IGameApplication.Current != null)
            throw new GameException("Game already was started.");

        // Optional Configuration: default erstellen, falls null
        Configuration = configuration ?? new GameConfiguration();

        _localization = new LocalizationManager(this);
        _registry = CreateRegistry() ?? throw new GameException("Registry creation failed.");
        _resources = CreateResources() ?? new GameResourceManager(this);
        _scenes = CreateScenes() ?? throw new GameException("Scene manager creation failed.");
        _settings = CreateSettings();

        _graphicsDeviceManager = ConfigureGraphicsDevice();
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

    public GameResourceManager Resources => _resources;

    public IGameSettings? Settings => _settings;

    public string CurrentDirectory => AppContext.BaseDirectory;

    public bool IsDebugMode { get; set; } = true;

    public static GameApplication Current =>
        IGameApplication.Current as GameApplication
        ?? throw new InvalidOperationException("GameApplication not initialized.");

    public Assembly Assembly =>
        Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    protected override sealed void Initialize()
    {
        if (Window != null)
            RegisterWindowEvents();

        _resources.Initialize();
        _scenes.Initialize();

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

    protected override sealed void LoadContent()
    {
        _settings?.Load();
        _resources.Load();
        _scenes.CurrentScene?.Load();
    }

    protected override sealed void UnloadContent()
    {
        _resources.Unload();
        base.UnloadContent();
    }

    protected override sealed void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _scenes.Update(gameTime);
    }

    protected override sealed void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _scenes.Draw(gameTime);
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        base.OnActivated(sender, args);

        if (_scenes.CurrentScene is IClientActivator activator)
            activator.OnClientActivate();
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
        base.OnDeactivated(sender, args);

        if (_scenes.CurrentScene is IClientActivator activator)
            activator.OnClientDeactivate();
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        if (_scenes.CurrentScene is IApplicationExitAware exitAware)
            exitAware.OnApplicationExited();

        foreach (var scene in _scenes.ActiveScenes.ToArray())
            scene.Unload();

        _settings?.Save();

        base.OnExiting(sender, args);
    }

    protected virtual GameRegistry CreateRegistry() => new GameRegistry();

    protected virtual GameResourceManager? CreateResources() => null;

    protected abstract ISceneManager CreateScenes();

    protected virtual IGameSettings? CreateSettings() => null;

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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sachssoft.Sasogine.Graphics;
using System.Globalization;
using Sachssoft.Sasogine.Features;
using Sachssoft.Inspection;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Localization;
using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine;

public abstract class GameApplication : Game, IGameApplication
{
    private GraphicsDeviceManager _graphicsDeviceManager;
    private SceneManager _viewManager;

    //private SurfaceHost? _surfaceHost;
    private IHost? _presentsationHost;
    protected private GameResourceManager _resourceManager;
    protected private PlatformProfiles _platform_profile;
    private readonly Dictionary<Type, GameSettings> _settings = new Dictionary<Type, GameSettings>();

    public static GameDispatcher Dispatcher = new GameDispatcher();
    private LocalizationManager _localization;

    private class ViewItem
    {
        public Type Type;
        public SceneBase? Instance;
    }

    static GameApplication()
    {
        TypeFactoryManager.Active();
        TypeRegistration.RegisterTypes();
    }

    public GameApplication(params string[] args)
    {
        if (IGameApplication.Current != null)
        {
            throw new GameException("Game already was started");
        }

        _graphicsDeviceManager = ConfigureGraphicsDevice();
        // Share GraphicsDeviceManager as a service.
        Services.AddService(typeof(GraphicsDeviceManager), _graphicsDeviceManager);
        _graphicsDeviceManager.ApplyChanges();

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        IGameApplication.Current = this;
    }

    public LocalizationManager Localization
    {
        get => _localization;
    }

    public static string CurrentDirectory => AppContext.BaseDirectory;

    /// <summary>
    /// Indicates whether the application is running on a mobile platform (Android or iOS),
    /// or if mobile simulation is enabled for testing purposes.
    /// </summary>
    public static bool IsMobile => OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || MobileSimulation;

    /// <summary>
    /// Indicates whether the application is running on a desktop platform (Windows, Linux, or macOS).
    /// </summary>
    public static bool IsDesktop => OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

    /// <summary>
    /// Enables simulation of a mobile environment on desktop for testing purposes only.
    /// </summary>
    protected static bool MobileSimulation { get; set; }

    /// <summary>
    /// Indicates whether the application is running in debug mode. This can be used to enable
    /// developer-specific features or diagnostics.
    /// </summary>
    public static bool IsDebugMode { get; set; } = true;

    protected override sealed void Initialize()
    {
        _localization = new LocalizationManager(this);

        if (Window != null)
            Window.ClientSizeChanged += (s, e) => PresensationHost?.Scene?.OnClientSizeChanged();

        _resourceManager = ResourcesOverride() ?? new GameResourceManager(this);
        _resourceManager.OnInitialize();

        base.Initialize();
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

    public static GameApplication Current
    {
        get => (GameApplication)IGameApplication.Current;
    }

    public IHost? PresensationHost
    {
        get => _presentsationHost;
    }

    protected virtual GameResourceManager? ResourcesOverride() => null;

    public GameResourceManager Resources => _resourceManager;

    GameResourceManager IGameApplication.Resources => throw new NotImplementedException();

    protected override sealed void LoadContent()
    {
        foreach (var setting in _settings.Values)
            setting.OnLoad();

        _platform_profile = DetermineGraphicsPlatformProfile();

        // ### Assets
        _resourceManager.OnLoad();
        //TypeFactoryManager.InvokeAssetRegistrations();

        // ## UI
        //UIEnvironment.Game = this;
        _presentsationHost = CreatePresensationHost();

        if (_presentsationHost != null)
        {
            //_surfaceHost.Game = this;
            _presentsationHost.Initialize(this);

            _viewManager = new SceneManager(_presentsationHost);
            InitializeViews(_viewManager);
        }

        OnLoad();
        _viewManager?.Load();

        // Schließen
        _localization.Close();
    }

    protected virtual IHost? CreatePresensationHost()
    {
        return null;
    }

    protected virtual void InitializeViews(SceneManager view)
    {
    }

    protected virtual void OnLoad()
    {
    }

    protected override sealed void UnloadContent()
    {
        OnUnload();

        _resourceManager.OnUnload();

        base.UnloadContent();
    }

    protected virtual void OnUnload()
    {
    }

    protected override sealed void Update(GameTime gameTime)
    {
        Dispatcher.ExecutePending();

        base.Update(gameTime);

        if (_viewManager != null)
        {
            _viewManager?.Update(gameTime, OnUpdate);
        }
        else
        {
            OnUpdate(CreateGameFrameContext(gameTime));
        }
    }

    protected virtual void OnUpdate(GameFrameContext context)
    {
    }

    protected override sealed void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        if (_viewManager != null)
        {
            _viewManager?.Draw(gameTime, OnDraw, OnDrawAfterGUI);
        }
        else
        {
            OnDraw(CreateGameFrameContext(gameTime));
        }
    }

    protected virtual void OnDraw(GameFrameContext context)
    {
    }

    protected virtual void OnDrawAfterGUI(GameFrameContext context)
    {
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        if (_presentsationHost != null)
        {
            var view = _presentsationHost.Scene;
            view?.OnUnload();

            _presentsationHost.Dispose(); //UIEnvironment.Game.Exit();
        }

        foreach (var setting in _settings.Values)
            setting.OnSave();

        base.OnExiting(sender, args);
    }

    public PlatformProfiles PlatformProfile => _platform_profile;

    private GameFrameContext CreateGameFrameContext(GameTime time)
    {
        return new GameFrameContext(null, time);
    }

    private PlatformProfiles DetermineGraphicsPlatformProfile()
    {
        // https://community.monogame.net/t/solved-how-to-determine-if-the-app-is-using-desktop-gl-or-dx/10494

        var a = Assembly.GetAssembly(typeof(Microsoft.Xna.Framework.Game));
        var shader_type = a.GetType("Microsoft.Xna.Framework.Graphics.Shader");
        var profile_property = shader_type.GetProperty("Profile");
        var value = (int)profile_property.GetValue(null);

        // https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Platform/Graphics/Shader/Shader.OpenGL.cs
        // 0 = OpenGl (Shader.OpenGL.cs bei GitHub)
        // 1 = DirectX (Shader.DirectX.cs bei GitHub)
        // ...

        return value switch
        {
            0 => PlatformProfiles.OpenGL,
            1 => PlatformProfiles.DirectX,
            _ => PlatformProfiles.Unknown

            // Zukünftig Vulkan, Metal, ...

            // Derzeit ist in der aktuellen Monogame-Version
            // nicht verfügar
        };
    }

    public void ChangeResolution(int width, int height, bool fullscreen = false)
    {
        _graphicsDeviceManager.PreferredBackBufferWidth = width;
        _graphicsDeviceManager.PreferredBackBufferHeight = height;
        _graphicsDeviceManager.IsFullScreen = fullscreen;
        _graphicsDeviceManager.ApplyChanges();
    }

    public SceneManager View => _viewManager;

    public Assembly Assembly => Assembly.GetEntryAssembly()!;

    // Bezeichnung von einem Typ
    public string? GetCaption(Type type, string? default_value = null)
    {
        var name = type.Name;

        // z.B. TestClass => Test Class
        // var policy = ...

        return GetCaption(name, name);
    }

    public string? GetCaption(string key, string? default_value = null)
    {
        // Sprach-JSON
        // Name => Key -> Bezeichnung finden!
        //

        return default_value;
    }

    protected void AddSettings<TSettings>() where TSettings : GameSettings, new()
    {
        if (_settings.ContainsKey(typeof(TSettings)))
            throw new InvalidOperationException("Setting Type already exists");

        TypeFactoryManager.Register<TSettings>();
        _settings.Add(typeof(TSettings), new TSettings());
    }

    public TSettings GetSettings<TSettings>() where TSettings : GameSettings, new()
    {
        if (!_settings.TryGetValue(typeof(TSettings), out var settings))
            throw new InvalidOperationException("Setting Type does not exists");

        return (TSettings)settings;
    }
}
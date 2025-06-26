/* 
 * © 2024 Tobias Sachs
 * MyGameApp
 * 08.07.2024 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using sachssoft.Sasogine.Graphics;
using sachssoft.Sasogine.Providers;
using sachssoft.Localization;
using System.Globalization;
using sachssoft.Sasogine.Features;
using sachssoft.Sasogine.Surface;
using sachssoft.Sasogine.Elements;

namespace sachssoft.Sasogine;

public abstract class MyGameApp<TAssetManager> : Game, IMyGameApp where TAssetManager : GameAssetManager
{
    private GraphicsDeviceManager _graphics_device_manager;
    private ViewManager _view_manager;
    private GameContext _game_context;
    private List<Region> _regions;

    private SurfaceHost? _surface_host;
    private int _selected_region_index;
    private TAssetManager _assets;
    protected private PlatformProfiles _platform_profile;
    private List<GameSettings> _settings;

    public static GameDispatcher Dispatcher = new GameDispatcher();

    public readonly static bool IsMobile = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();

    public readonly static bool IsDesktop = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsWindows();

    public static bool IsDebugMode { get; set; } = true; // 

    private class ViewItem
    {
        public Type Type;
        public ViewBase? Instance;
    }

    static MyGameApp()
    {
        TypeManager.EnsureInitialized();
    }

    public MyGameApp(params string[] args)
    {
        if (IMyGameApp.Current != null)
        {
            throw new GameException("Game already was started");
        }

        _regions = new List<Region>();
        _settings = new List<GameSettings>();

        _graphics_device_manager = ConfigureGraphicsDevice();
        // Share GraphicsDeviceManager as a service.
        Services.AddService(typeof(GraphicsDeviceManager), _graphics_device_manager);
        _graphics_device_manager.ApplyChanges();

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        IMyGameApp.Current = this;
    }

    protected override void Initialize()
    {
        base.Initialize();


        // Load supported languages and set the default language.
        List<CultureInfo> cultures = LocalizationManager.GetSupportedCultures();
        var languages = new List<CultureInfo>();
        for (int i = 0; i < cultures.Count; i++)
        {
            languages.Add(cultures[i]);
        }

        // TODO You should load this from a settings file or similar,
        // based on what the user or operating system selected.
        var selectedLanguage = LocalizationManager.DEFAULT_CULTURE_CODE;
        LocalizationManager.SetCulture(selectedLanguage);
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

    public static MyGameApp<TAssetManager> Current
    {
        get => (MyGameApp<TAssetManager>)IMyGameApp.Current;
    }

    //public Desktop Desktop
    //{
    //    get => _gui_desktop;
    //}

    public SurfaceHost? SurfaceHost
    {
        get => _surface_host;
    }

    public IWebBrowserProvider? WebBrowser
    {
        get;
        set;
    }

    public IFileExplorerProvider? FileExplorer
    {
        get;
        set;
    }

    protected abstract TAssetManager CreateAssetManager();

    protected override sealed void LoadContent()
    {
        base.LoadContent();

        _settings.ForEach(x => x.OnLoad());

        _platform_profile = DetermineGraphicsPlatformProfile();

        // ### Assets
        //_assets = (TAssetManager)Activator.CreateInstance(typeof(TAssetManager), this); // AOT-Problem!
        _assets = CreateAssetManager();
        _assets.OnLoad();
        TypeManager.InvokeAssetRegistrations();

        // ## UI
        //UIEnvironment.Game = this;
        _surface_host = CreateSurfaceHost();
        _surface_host?.Initialize(this); // <--- UIEnvironment.Game

        if (_surface_host != null)
        {
            _view_manager = new ViewManager(_surface_host);
            InitializeViews(_view_manager);
        }

        OnLoad();
        _view_manager?.Load();
    }

    protected virtual SurfaceHost? CreateSurfaceHost()
    {
        return null;
    }

    protected virtual void InitializeViews(ViewManager view)
    {
    }

    protected virtual void OnLoad()
    {
    }

    protected override sealed void UnloadContent()
    {
        OnUnload();

        _assets.OnUnload();

        base.UnloadContent();
    }

    protected virtual void OnUnload()
    {
    }

    protected override sealed void Update(GameTime time)
    {
        Dispatcher.ExecutePending();

        base.Update(time);

        _view_manager.Update(time, OnUpdate);
    }

    protected virtual void OnUpdate(GameContext context)
    {
    }

    protected override sealed void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        _view_manager.Draw(gameTime, OnDraw, OnDrawAfterGUI);
    }

    protected virtual void OnDraw(GameContext context)
    {
    }

    protected virtual void OnDrawAfterGUI(GameContext context)
    {
    }

    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        if (_surface_host != null)
        {
            var view = _surface_host.View;
            view?.OnUnload();

            _surface_host.Dispose(); //UIEnvironment.Game.Exit();
        }

        _settings.ForEach(x => x.OnSave());

        base.OnExiting(sender, args);
    }

    public PlatformProfiles PlatformProfile => _platform_profile;

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

    public TAssetManager Assets => _assets;

    GameAssetManager IMyGameApp.Assets => _assets;

    public void SelectRegion(int index)
    {
        _selected_region_index = index;
    }

    // Regionabhängige Assets
    protected void AddRegion(Region region)
    {
        if (_regions.Where(r => region.Name == region.Name || region.Prefix == region.Prefix).Count() > 1)
            throw new GameException("Region already exists");

        _regions.Add(region);
    }

    public Region CurrentRegion => _regions[_selected_region_index];

    public IEnumerable<Region> Regions => _regions.AsEnumerable();

    public ViewManager View => _view_manager;

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

    // Für AOT Umgestellt

    //#region View Manager

    ////private readonly Dictionary<Type, ViewBase> _views = new();
    //private readonly Dictionary<Type, (Func<ViewBase> Factory, ViewBase? Instance)> _views = new();

    //public void RegisterView<TView>() where TView : ViewBase, new()
    //    => RegisterView(() => new TView());

    //public void RegisterView<TView>(Func<TView> factory) where TView : ViewBase
    //{
    //    var type = typeof(TView);

    //    if (_views.ContainsKey(type))
    //        throw new InvalidOperationException("ViewOwner already registered.");

    //    //var instance = factory();
    //    //_views.Add(type, instance);

    //    _views.Add(type, (() => factory(), null));
    //}

    ////public void SetDefaultView<TView>(Action<TView>? init = null) where TView : ViewBase
    ////{
    ////    if (!_views.ContainsKey(typeof(TView)))
    ////        throw new InvalidOperationException("ViewOwner type not registered.");

    ////    _default_view_type = typeof(TView);
    ////    _default_view_init_action = init != null ? v => init((TView)v) : null;
    ////}

    //public void SwitchView<TView>(Action<TView>? init = null) where TView : ViewBase
    //{
    //    if (!_views.ContainsKey(typeof(TView)))
    //        throw new InvalidOperationException("ViewOwner type not registered.");

    //    SwitchViewImpl(typeof(TView), init != null ? v => init((TView)v!) : null);
    //}

    ////public void GoBackView(Action<ViewBase>? init = null)
    ////{
    ////    if (_default_view_type == null)
    ////        throw new InvalidOperationException("No default new_view defined.");

    ////    SwitchViewImpl(_default_view_type, init ?? _default_view_init_action);
    ////}

    //public void OpenView<TView>(Action<TView>? init = null) where TView : ViewBase
    //{
    //    if (!_views.TryGetValue(typeof(TView), out var item))
    //        throw new InvalidOperationException("ViewOwner item not found.");

    //    if (item.Instance != null)
    //        throw new InvalidOperationException("View already open.");

    //    var new_view = item.Factory();
    //    SurfaceHost!.View = new_view;

    //    if (!new_view.IsLoaded && new_view.ViewSwitchMode == ViewSwitchMode.Restart)
    //    {
    //        new_view.OnLoadPrepareInternal();
    //        new_view.OnLoad();
    //        new_view.IsLoaded = true;
    //    }

    //    init?.Invoke((TView)new_view);
    //    new_view.IsActive = true;
    //}

    //public void CloseView<TView>() where TView : ViewBase
    //{
    //}

    //private void SwitchViewImpl(Type type, Action<ViewBase>? init = null)
    //{
    //    if (SurfaceHost == null)
    //        throw new InvalidOperationException("No surface host set.");

    //    if (!_views.TryGetValue(type, out var item))
    //        throw new InvalidOperationException("ViewOwner item not found.");

    //    var new_view = item.Instance;

    //    if (new_view == null)
    //        throw new InvalidOperationException("View was not open.");

    //    var old_view = SurfaceHost.View;

    //    if (old_view != null)
    //    {
    //        old_view.IsActive = false;

    //        if (old_view.ViewSwitchMode == ViewSwitchMode.Restart && old_view.IsLoaded)
    //        {
    //            old_view.OnUnload();
    //            old_view.IsLoaded = false;
    //        }
    //    }

    //    SurfaceHost.View = new_view;

    //    if (!new_view.IsLoaded && new_view.ViewSwitchMode == ViewSwitchMode.Restart)
    //    {
    //        new_view.OnLoadPrepareInternal();
    //        new_view.OnLoad();
    //        new_view.IsLoaded = true;
    //    }

    //    init?.Invoke(new_view);
    //    new_view.IsActive = true;
    //}

    //public ViewBase? View => SurfaceHost?.View;

    //ViewBase? IMyGameApp.GetCurrentView() => View;

    //#endregion

    protected void AddSettings(GameSettings settings) => _settings.Add(settings);

    public virtual GameSettings GetSettings(int index) => _settings[index];

}
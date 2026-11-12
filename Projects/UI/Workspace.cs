using Sachssoft.Engine.Graphics.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.ComponentModel;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Owns one deterministic control tree, input routing, focus and frame boundaries.
/// </summary>
public sealed class Workspace : Element, IDisposable
{
    private float _dpi = 96f;
    private float _frameDpi = 96f;
    private Rectangle? _frameViewport;
    private static readonly PropertyChangedEventArgs DpiArgs = new(nameof(Dpi));
    private static readonly PropertyChangedEventArgs DpiScaleArgs = new(nameof(DpiScale));
    /// <summary>
    /// Gets or sets pixel density in dots per inch; 96 means 100 percent. Changes apply on the next input frame.
    /// </summary>
    public float Dpi
    {
        get => _dpi;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!float.IsFinite(value) || value <= 0 || value / 96f <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "DPI must be finite and positive.");
            if (_drawing)
                throw new InvalidOperationException("Drawing cannot change workspace DPI.");
            if (SetNotify(ref _dpi, value, DpiArgs))
                NotifyPropertyChanged(DpiScaleArgs);
        }
    }

    /// <summary>
    /// Gets the requested physical pixels per logical UI unit.
    /// </summary>
    public float DpiScale => _dpi / 96f;

    internal float FrameScale => _frameDpi / 96f;
    internal Point FrameOrigin => _frameViewport?.Location ?? Point.Zero;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(Dpi))
            _layoutDirty = true;
        base.OnPropertyChanged(notification);
    }

    /// <summary>
    /// Gets or sets the application's optional viewport color source.
    /// </summary>
    public IColorPickSource? ColorPickSource { get; set; }
    /// <summary>
    /// Gets the input-isolated color-picking session.
    /// </summary>
    public ColorPickSession ColorPicking { get; } = new();
    /// <summary>
    /// Gets the workspace-local asset drag gesture.
    /// </summary>
    public AssetDragSession AssetDragging { get; } = new();
    /// <summary>
    /// Gets explicit bindings processed before layout and after input.
    /// </summary>
    public IList<IFrameBinding> Bindings { get; } = new List<IFrameBinding>();

    private void UpdateBindings()
    {
        for (int i = 0; i < Bindings.Count; i++)
            Bindings[i].Update();
    }

    /// <summary>Coalesces multiple changes to the same scroll offset before arranging its content.</summary>
    private readonly HashSet<ScrollViewer> _queuedScrollViewers = new(ReferenceEqualityComparer.Instance);

    /// <summary>Preserves the order in which scroll offsets changed.</summary>
    private readonly Queue<ScrollViewer> _scrollViewers = new();

    /// <summary>Stores the controls on the previous pointer hit path.</summary>
    private List<Control> _hoverPath = new(16);

    /// <summary>Reuses storage for the next pointer hit path.</summary>
    private List<Control> _nextHoverPath = new(16);

    /// <summary>Tracks committed membership and ordering changes separately from layout.</summary>
    private bool _controlsDirty = true;
    private ControlPointer _hoverPointer;

    /// <summary>Gets or sets whether pressing and dragging the mouse can pan scroll viewers and lists.</summary>
    public bool IsMouseDragScrollingEnabled { get; set; }

    /// <summary>Gets or sets whether scroll gestures start smoothly and decelerate after release.</summary>
    public bool IsSmoothScrollingEnabled { get; set; }

    /// <summary>Gets the elapsed time used to sample scroll gestures in this workspace.</summary>
    internal double ScrollInputTime { get; private set; }

    /// <summary>Gets or sets whether scrollbars draw only while hovered or captured by the mouse.</summary>
    /// <remarks>Hidden artwork preserves the viewport and its scroll range.</remarks>
    public bool ShowScrollBarsOnMouseHover { get; set; }

    /// <summary>Gets or sets whether sampled touch contacts take precedence over simultaneous mouse events.</summary>
    /// <remarks>Host sampling omits unchanged pointer positions in this mode.</remarks>
    public bool PreferTouchInput { get; set; }

    /// <summary>Gets whether the current hover path belongs to the mouse.</summary>
    internal bool HasMouseHover => _hasPointer && _hoverPointer.Kind == ControlPointerKind.Mouse;

    private bool _layoutDirty = true;

    /// <summary>Tracks changes that require preparing controls before measurement.</summary>
    private bool _prepareDirty = true;
    private bool _prepareAll = true;
    private bool _measurePending = true;
    private bool _layoutPassActive;
    private ulong _layoutEpoch = 1;

    /// <summary>Gets the epoch for coalescing requests outside an active layout pass.</summary>
    // A control can invalidate an ancestor that has already started or completed its work in this pass.
    internal ulong LayoutInvalidationEpoch => _layoutPassActive ? 0 : _layoutEpoch;

    /// <summary>Gets the epoch for coalescing visual revisions between frame boundaries.</summary>
    internal ulong VisualInvalidationEpoch { get; private set; } = 1;

    /// <summary>Starts a new invalidation epoch when local layout consumes cached work.</summary>
    internal void ObserveExternalLayout()
    {
        if (!_layoutPassActive)
            _layoutEpoch = unchecked(_layoutEpoch + 1);
    }

    private int _styleRevision = -1;
    private Size2 _previousSize;
    /// <summary>
    /// Invalidates layout after explicit property or tree changes.
    /// </summary>
    internal void InvalidateLayout()
    {
        _prepareAll = true;
        ScheduleControlLayout(measure: true, prepare: true);
    }

    /// <summary>Schedules the work requested by a control for the next layout pass.</summary>
    /// <param name="measure">Whether measurement is required.</param>
    /// <param name="prepare">Whether preparation is required.</param>
    internal void ScheduleControlLayout(bool measure, bool prepare)
    {
        _layoutDirty = true;
        _measurePending |= measure;
        _prepareDirty |= prepare;
    }

    /// <summary>Invalidates the snapshot of the committed control tree.</summary>
    internal void InvalidateControlTree()
    {
        _controlsDirty = true;
        _layoutEpoch = unchecked(_layoutEpoch + 1);
        VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
        InvalidateFontInheritance();
        InvalidateLayout();
    }

    /// <summary>Gets the revision of font inputs and logical ancestry in this workspace.</summary>
    internal ulong FontInheritanceRevision { get; private set; }

    /// <summary>Gets or sets the row whose binding changes are handled by its current layout pass.</summary>
    internal Control? LocalLayoutRoot { get; set; }

    /// <summary>Invalidates cached checks of inherited font descriptors.</summary>
    internal void InvalidateFontInheritance() => FontInheritanceRevision = unchecked(FontInheritanceRevision + 1);

    /// <summary>Queues content arrangement for a scroll viewer whose range value changed.</summary>
    /// <param name="viewer">The scroll viewer whose content offset changed.</param>
    internal void QueueScrollOffset(ScrollViewer viewer)
    {
        if (_queuedScrollViewers.Add(viewer))
            _scrollViewers.Enqueue(viewer);
    }
    /// <summary>
    /// Gets central styles without requiring the caller to create a registry.
    /// </summary>
    public StyleRegistry Styles => Registry.Styles;

    private Dictionary<string, ControlSkinRegistration>? _skins;
    private Dictionary<string, ControlSkinRegistration> Skins => _skins ??= new(StringComparer.Ordinal)
    {
        [nameof(SkinPreset.SachssoftDark)] = new(static device => ControlSkin.LoadEmbedded(device, SkinPreset.SachssoftDark)),
        [nameof(SkinPreset.SachssoftLight)] = new(static device => ControlSkin.LoadEmbedded(device, SkinPreset.SachssoftLight)),
        [nameof(SkinPreset.SachssoftModern)] = new(static device => ControlSkin.LoadEmbedded(device, SkinPreset.SachssoftModern)),
        [nameof(SkinPreset.SachssoftFuturistic)] = new(static device => ControlSkin.LoadEmbedded(device, SkinPreset.SachssoftFuturistic)),
        [nameof(SkinPreset.SachssoftMedieval)] = new(static device => ControlSkin.LoadEmbedded(device, SkinPreset.SachssoftMedieval))
    };
    private string? _skinName = nameof(SkinPreset.SachssoftDark);
    private ControlSkin? _directSkin;
    private GraphicsDevice? _skinDevice;

    /// <summary>
    /// Gets the case-sensitive names of the built-in and custom skins registered in this workspace.
    /// </summary>
    public IReadOnlyCollection<string> AvailableSkins => Skins.Keys;

    /// <summary>Checks whether a skin is registered under the exact name.</summary>
    /// <param name="name">The case-sensitive registered name.</param>
    /// <returns>Whether the name is registered.</returns>
    public bool ContainsSkin(string name) => Skins.ContainsKey(name);

    /// <summary>Registers a caller-owned skin for selection by name.</summary>
    /// <param name="name">The case-sensitive name to register.</param>
    /// <param name="skin">The skin borrowed by the workspace.</param>
    /// <param name="replaceExisting">Whether to replace an existing registration with the same name.</param>
    /// <exception cref="ArgumentException">The name already exists and replacement was not requested.</exception>
    public void RegisterSkin(string name, ControlSkin skin, bool replaceExisting = false)
    {
        ArgumentNullException.ThrowIfNull(skin);
        if (_skins is not null && _skins.Values.Any(entry => entry.Factory is not null && ReferenceEquals(entry.Skin, skin)))
            throw new ArgumentException("A workspace-owned skin cannot be registered as a borrowed skin.", nameof(skin));
        RegisterSkinCore(name, new ControlSkinRegistration(skin), replaceExisting);
    }

    /// <summary>Registers a factory that creates a workspace-owned skin when first selected on a graphics device.</summary>
    /// <param name="name">The case-sensitive name to register.</param>
    /// <param name="factory">Creates a new skin for the supplied graphics device. The workspace disposes each result.</param>
    /// <param name="replaceExisting">Whether to replace an existing registration with the same name.</param>
    /// <exception cref="ArgumentException">The name already exists and replacement was not requested.</exception>
    public void RegisterSkin(string name, Func<GraphicsDevice, ControlSkin> factory, bool replaceExisting = false)
    {
        ArgumentNullException.ThrowIfNull(factory);
        RegisterSkinCore(name, new ControlSkinRegistration(factory), replaceExisting);
    }

    private void RegisterSkinCore(string name, ControlSkinRegistration registration, bool replaceExisting)
    {
        VerifyTreeMutation();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (Skins.TryGetValue(name, out ControlSkinRegistration? previous) && !replaceExisting)
            throw new ArgumentException($"A control skin named '{name}' is already registered.", nameof(name));

        if (_skinName == name)
            ApplyRegistration(registration);
        Skins[name] = registration;
        if (previous?.Factory is not null)
            previous.Skin?.Dispose();
    }

    /// <summary>Selects a built-in skin using the same registry as named skins.</summary>
    /// <param name="preset">The built-in selection. Default selects Sachssoft Dark.</param>
    /// <exception cref="ArgumentOutOfRangeException">The preset is undefined.</exception>
    public void SetSkin(SkinPreset preset)
    {
        string name = preset switch
        {
            SkinPreset.Default or SkinPreset.SachssoftDark => nameof(SkinPreset.SachssoftDark),
            SkinPreset.SachssoftLight => nameof(SkinPreset.SachssoftLight),
            SkinPreset.SachssoftModern => nameof(SkinPreset.SachssoftModern),
            SkinPreset.SachssoftFuturistic => nameof(SkinPreset.SachssoftFuturistic),
            SkinPreset.SachssoftMedieval => nameof(SkinPreset.SachssoftMedieval),
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unknown skin preset.")
        };
        SetSkin(name);
    }

    /// <summary>Selects a registered skin. Loading waits for graphics initialization if necessary.</summary>
    /// <param name="name">The case-sensitive registered name.</param>
    /// <exception cref="KeyNotFoundException">The name is not registered.</exception>
    public void SetSkin(string name)
    {
        VerifyTreeMutation();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!Skins.TryGetValue(name, out ControlSkinRegistration? registration))
            throw new KeyNotFoundException($"No control skin named '{name}' is registered.");
        ApplyRegistration(registration);
        _skinName = name;
        _directSkin = null;
    }

    /// <summary>Applies a caller-owned skin directly, including metadata-only skins.</summary>
    /// <param name="skin">The skin borrowed by the workspace.</param>
    public void SetSkin(ControlSkin skin)
    {
        VerifyTreeMutation();
        ArgumentNullException.ThrowIfNull(skin);
        if (_skins is not null)
            foreach (var entry in _skins)
                if (entry.Value.Factory is not null && ReferenceEquals(entry.Value.Skin, skin))
                {
                    SetSkin(entry.Key);
                    return;
                }
        ApplySkin(skin, _skinDevice);
        _skinName = null;
        _directSkin = skin;
    }

    /// <summary>
    /// Connects the graphics device and applies the selected skin, or Sachssoft Dark by default.
    /// Hosts that supply their own input snapshots call this before the first update and after device replacement.
    /// </summary>
    /// <param name="device">The graphics device that will draw the workspace.</param>
    public void InitializeGraphics(GraphicsDevice device)
    {
        VerifyTreeMutation();
        ArgumentNullException.ThrowIfNull(device);
        if (ReferenceEquals(_skinDevice, device) && Registry.Skin?.Texture?.IsDisposed != true)
            return;

        ControlSkinRegistration? registration = _skinName is null ? null : Skins[_skinName];
        ControlSkin? skin = registration?.Factory is { } factory
            ? CreateAndApplySkin(factory, device)
            : registration?.Skin ?? _directSkin;
        if (registration?.Factory is null && skin is not null)
            ApplySkin(skin, device);

        ReleaseOwnedSkins();
        if (registration?.Factory is not null)
            registration.Skin = skin;
        _skinDevice = device;
    }

    private void ApplyRegistration(ControlSkinRegistration registration)
    {
        if (registration.Skin is { } skin && (registration.Factory is null || skin.Texture?.IsDisposed != true))
            ApplySkin(skin, _skinDevice);
        else if (_skinDevice is { } device && registration.Factory is { } factory)
        {
            ControlSkin? previous = registration.Skin;
            registration.Skin = CreateAndApplySkin(factory, device);
            previous?.Dispose();
        }
    }

    private ControlSkin CreateAndApplySkin(Func<GraphicsDevice, ControlSkin> factory, GraphicsDevice device)
    {
        ControlSkin skin = factory(device) ?? throw new InvalidOperationException("The control skin factory returned null.");
        try
        {
            ApplySkin(skin, device);
            return skin;
        }
        catch
        {
            skin.Dispose();
            throw;
        }
    }

    private void ApplySkin(ControlSkin skin, GraphicsDevice? device)
    {
        if (skin.Texture is { } texture && (texture.IsDisposed || device is not null && !ReferenceEquals(texture.GraphicsDevice, device)))
            throw new InvalidOperationException("The skin texture must be usable on the workspace graphics device.");
        if (ReferenceEquals(Registry.Skin, skin))
            return;
        skin.Apply(Registry);
        InvalidateLayout();
    }

    private void ReleaseOwnedSkins()
    {
        if (_skins is null)
            return;
        foreach (ControlSkinRegistration registration in _skins.Values)
        {
            if (registration.Factory is null || registration.Skin is not { } skin)
                continue;
            if (ReferenceEquals(Registry.Skin, skin))
                ControlSkin.Clear(Registry);
            skin.Dispose();
            registration.Skin = null;
        }
    }

    private readonly List<Action> _pending = new();
    private readonly ControlInputContext _input;
    private bool _busy;
    private bool _drawing;
    private bool _hasFrame;
    private bool _hasPointer;
    private Vector2 _pointer;
    private ControlLayoutContext? _layout;
    private GameApplicationBase? _hostApplication;
    private ControlLayoutContext? _hostLayout;
    private ControlInputSampler? _hostInput;
    private ControlDrawContext? _hostDraw;
    private GraphicsDevice? _hostDrawDevice;
    /// <summary>
    /// Attaches an unowned control tree.
    /// </summary>
    public Workspace(Control content, UIRegistry? registry = null)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        if (content.Parent is not null || content.RootOwner is not null || content.Workspace is not null || content.CommittedParent is not null)
            throw new InvalidOperationException("Content already has an owner.");
        Registry = registry ?? content.FactoryRegistry ?? new UIRegistry();
        if (Registry.Skin is { } skin)
        {
            _skinName = null;
            _directSkin = skin;
        }
        ReserveRoot(content);
        ContainerControl.Attach(content, this);
        _input = new ControlInputContext(this);
        RebuildControls();
    }

    /// <summary>Connects the workspace to an application for managed input, layout, and drawing.</summary>
    /// <param name="application">The application supplying focus state, graphics, and services.</param>
    /// <param name="defaultFont">The optional caller-owned default font.</param>
    /// <exception cref="InvalidOperationException">The workspace is processing a frame or already has a host.</exception>
    public void Initialize(GameApplicationBase application, IFont? defaultFont = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        Initialize(application, defaultFont, application.GetService<IFontBackend>());
    }

    internal void Initialize(GameApplicationBase application, IFont? defaultFont, IFontBackend? applicationBackend)
    {
        VerifyTreeMutation();
        if (_busy || _hostApplication is not null)
            throw new InvalidOperationException("Initialize the workspace once, outside frame processing.");

        SetApplicationFontBackend(applicationBackend);
        InitializeGraphics(application.GraphicsDevice);
        _hostLayout = defaultFont is null ? ControlLayoutContext.Default : new ControlLayoutContext(defaultFont);
        _hostApplication = application;
        _hasFrame = false;
    }

    /// <summary>Queues a native text character for the next managed input update.</summary>
    /// <param name="character">The character supplied by the host window.</param>
    public void AddTextInput(char character)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        (_hostInput ??= new ControlInputSampler()).AddText(character);
    }

    internal void ResetInput() => _hostInput = null;

    private IFontBackend? _applicationFontBackend;
    private IFontBackend? _defaultLayoutBackend;
    private ControlLayoutContext? _backendDefaultLayout;
    private bool _fontsDirty;
    private int _fontRegistryRevision = -1;
    internal int FontRevision { get; private set; }
    internal IFont? LayoutFont => _layout?.DefaultFont;
    /// <summary>
    /// Gets the borrowed Sasogine font registry used by this workspace.
    /// </summary>
    public FontRegistry Fonts => Registry.Fonts;

    /// <summary>
    /// Invalidates font adapters after replacing families or changing externally managed font resources.
    /// </summary>
    /// <remarks>Registry edits are detected automatically. Call this when external font data changes.</remarks>
    public void InvalidateFonts()
    {
        VerifyTreeMutation();
        _fontsDirty = true;
        InvalidateLayout();
        _fontRegistryRevision = Fonts.Revision;
    }

    internal void SetApplicationFontBackend(IFontBackend? backend)
    {
        if (ReferenceEquals(_applicationFontBackend, backend))
            return;
        _applicationFontBackend = backend;
        InvalidateFonts();
    }

    internal IFont ResolveFont(ControlFontRequest request, IFont fallback) =>
        Fonts.ResolveFont(request.Source ?? fallback, request.Name, request.Size, request.Weight, request.Style,
            Registry.FontBackend ?? _applicationFontBackend);

    private ControlLayoutContext ResolveDefaultLayout(ControlLayoutContext layout)
    {
        IFontBackend? backend = Registry.FontBackend ?? _applicationFontBackend;
        if (!layout.UsesDefaultFont || backend is null)
            return layout;
        if (_backendDefaultLayout is null || !ReferenceEquals(_defaultLayoutBackend, backend))
        {
            _backendDefaultLayout = new ControlLayoutContext(
                BuiltInFonts.GetFont(BuiltInFont.Sans, BuiltInFonts.DefaultSize, backend: backend));
            _defaultLayoutBackend = backend;
        }
        return _backendDefaultLayout;
    }
    internal Control? FontParent(Control control)
    {
        if (ReferenceEquals(control, Content))
            return null;
        if (ReferenceEquals(control, _visibleToolTip))
            return _tipOwner ?? Content;
        foreach (ControlOverlay overlay in _overlays)
            if (ReferenceEquals(overlay.Control, control))
                return overlay.Owner ?? Content;
        return null;
    }

    /// <summary>
    /// Gets the explicit UI factory and style registry shared by this tree.
    /// </summary>
    public UIRegistry Registry { get; }
    /// <summary>
    /// Gets the root control.
    /// </summary>
    public Control Content { get; }
    /// <summary>
    /// Gets the last processed logical viewport size.
    /// </summary>
    public Size2 Size { get; private set; }
    /// <summary>
    /// Gets the current keyboard focus owner.
    /// </summary>
    public Control? FocusedControl { get; private set; }
    /// <summary>
    /// Gets the mouse capture owner. Use GetCapturedControl for touch identities.
    /// </summary>
    public Control? CapturedControl => GetCapturedControl(ControlPointer.Mouse);

    private readonly Dictionary<ControlPointer, Control> _captures = new();
    /// <summary>
    /// Gets the owner of an independent mouse or touch pointer.
    /// </summary>
    public Control? GetCapturedControl(ControlPointer pointer) => _captures.TryGetValue(pointer, out Control? owner) ? owner : null;
    /// <summary>
    /// Processes one application UI context.
    /// </summary>
    public void Update(ControlUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        SetApplicationFontBackend(context.FontBackend);
        InitializeGraphics(context.GraphicsDevice);
        ProcessFrame(context.Input, context.Layout);
    }

    /// <summary>Polls mouse, keyboard, and touch once and updates the workspace.</summary>
    /// <param name="context">The application frame context supplying timing and graphics.</param>
    /// <param name="viewport">The physical viewport override. Defaults to the current device viewport.</param>
    /// <param name="active">The focus override. Defaults to the application's active state.</param>
    /// <returns>The sampled input, available to other consumers of the same frame.</returns>
    /// <exception cref="InvalidOperationException">The workspace has no host or is already processing a frame.</exception>
    public InputSnapshot Update(GameContext context, Rectangle? viewport = null, bool? active = null)
    {
        GameApplicationBase application = GetHost(context, out GraphicsDevice device);
        return PollHost(application, device, context.GameTime, viewport, active);
    }

    /// <summary>Polls input and updates a workspace directly from the MonoGame update loop.</summary>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="viewport">The physical viewport override. Defaults to the current device viewport.</param>
    /// <param name="active">The focus override. Defaults to the application's active state.</param>
    /// <returns>The sampled input for this update.</returns>
    public InputSnapshot Update(GameTime gameTime, Rectangle? viewport = null, bool? active = null)
    {
        ArgumentNullException.ThrowIfNull(gameTime);
        GameApplicationBase application = GetHost();
        return PollHost(application, application.GraphicsDevice, gameTime, viewport, active);
    }

    private InputSnapshot PollHost(GameApplicationBase application, GraphicsDevice device, GameTime gameTime,
        Rectangle? viewport, bool? active)
    {
        InputSnapshot input = InputSnapshot.Capture();
        UpdateHost(application, device, gameTime, input, viewport, active);
        return input;
    }

    /// <summary>Updates the workspace using a snapshot already captured by the caller.</summary>
    /// <param name="context">The application frame context supplying timing and graphics.</param>
    /// <param name="input">The mouse, keyboard, and touch states to consume.</param>
    /// <param name="viewport">The physical viewport override. Defaults to the current device viewport.</param>
    /// <param name="active">The focus override. Defaults to the application's active state.</param>
    /// <exception cref="InvalidOperationException">The workspace has no host or is already processing a frame.</exception>
    public void Update(GameContext context, in InputSnapshot input, Rectangle? viewport = null, bool? active = null)
    {
        GameApplicationBase application = GetHost(context, out GraphicsDevice device);
        UpdateHost(application, device, context.GameTime, input, viewport, active);
    }

    private void UpdateHost(GameApplicationBase application, GraphicsDevice device, GameTime gameTime,
        in InputSnapshot input, Rectangle? viewport, bool? active)
    {
        InitializeGraphics(device);
        Update(_hostInput ??= new ControlInputSampler(), input.Mouse, input.Keyboard, input.Touches,
            viewport ?? device.Viewport.Bounds, gameTime.ElapsedGameTime, _hostLayout!, active ?? application.IsActive);
    }

    private GameApplicationBase GetHost(GameContext context, out GraphicsDevice device)
    {
        ArgumentNullException.ThrowIfNull(context);
        GameApplicationBase application = GetHost();
        device = application.GraphicsDevice;
        if (!ReferenceEquals(context.GraphicsDevice, device))
            throw new ArgumentException("The frame context must use the workspace host's graphics device.", nameof(context));
        return application;
    }

    private GameApplicationBase GetHost()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_busy)
            throw new InvalidOperationException("UI processing cannot be reentered.");
        return _hostApplication ??
            throw new InvalidOperationException("Initialize the workspace with an application before using managed updates or drawing.");
    }

    /// <summary>
    /// Samples caller-provided mouse and keyboard states using this workspace's DPI and a physical viewport.
    /// </summary>
    public void Update(ControlInputSampler sampler, MouseState mouse, KeyboardState keyboard, Rectangle viewport, TimeSpan elapsed, ControlLayoutContext layout, bool active = true) => Update(sampler, mouse, keyboard, default, viewport, elapsed, layout, active);
    /// <summary>
    /// Samples mouse/touch/keyboard once, derives logical layout size and synchronizes subsequent drawing automatically.
    /// </summary>
    public void Update(ControlInputSampler sampler, MouseState mouse, KeyboardState keyboard, TouchCollection touches, Rectangle viewport, TimeSpan elapsed, ControlLayoutContext layout, bool active = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(sampler);
        ArgumentNullException.ThrowIfNull(layout);
        if (_busy)
            throw new InvalidOperationException("UI processing cannot be reentered.");
        float dpi = Dpi;
        ControlInputFrame frame = sampler.SampleReusable(mouse, keyboard, touches, viewport, dpi / 96f, elapsed, active, PreferTouchInput);
        ProcessFrameCore(frame, layout, viewport, dpi);
    }

    /// <summary>
    /// Updates one frame using the supplied layout context.
    /// </summary>
    public void Update(ControlInputFrame frame, ControlLayoutContext layout) => ProcessFrame(frame, layout);
    /// <summary>Updates one input frame using the configured or built-in default font.</summary>
    /// <param name="frame">The input snapshot to process.</param>
    public void Update(ControlInputFrame frame) => ProcessFrame(frame, _hostLayout ?? ControlLayoutContext.Default);
    /// <summary>
    /// Processes ordered input, updates controls once, commits mutations, and finalizes layout.
    /// </summary>
    public void ProcessFrame(ControlInputFrame frame, ControlLayoutContext layout) => ProcessFrameCore(frame, layout, frame?.PhysicalViewport, Dpi);
    private void ProcessFrameCore(ControlInputFrame frame, ControlLayoutContext layout, Rectangle? viewport, float frameDpi)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(layout);
        if (_busy)
            throw new InvalidOperationException("UI processing cannot be reentered.");
        Size2 logicalSize = viewport is Rectangle physical ? new Size2(physical.Width / (frameDpi / 96f), physical.Height / (frameDpi / 96f)) : frame.Size;
        Control.ValidateSize(logicalSize, false);
        float inputScale = frame.PhysicalViewport.HasValue ? frame.SourceScale / (frameDpi / 96f) : 1;
        if (!float.IsFinite(inputScale) || inputScale <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameDpi));
        _hasFrame = false;
        _frameViewport = viewport;
        _frameDpi = frameDpi;
        if (IsSmoothScrollingEnabled)
            ScrollInputTime += frame.Elapsed.TotalSeconds;
        _busy = true;
        VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
        try
        {
            while (_posted.TryDequeue(out Action? posted))
                posted();
            Commit();
            UpdateBindings();
            RebuildControls();
            foreach (Control control in EnumerateControls())
                control.BeginFrame();
            if (!_previousSize.Equals(logicalSize))
            {
                InvalidateLayout();
                _previousSize = logicalSize;
            }

            Size = logicalSize;
            layout = ResolveDefaultLayout(layout);
            if (!ReferenceEquals(_layout?.DefaultFont, layout.DefaultFont))
                InvalidateLayout();
            _layout = layout;
            if (_fontRegistryRevision != Fonts.Revision)
                InvalidateFonts();
            Layout();
            ValidateInteraction();
            for (int eventIndex = 0; eventIndex < frame.Events.Count; eventIndex++)
            {
                ControlInputEvent input = frame.Events[eventIndex];
                if (inputScale != 1 && input.Type is (ControlInputEventType.PointerMove or ControlInputEventType.PointerDown or ControlInputEventType.PointerUp or ControlInputEventType.PointerWheel or ControlInputEventType.PointerCancel))
                    input = input with
                    {
                        Position = input.Position * inputScale
                    };
                Dispatch(input);
                UpdateBindings();
                Layout();
                ValidateInteraction();
            }

            ColorPicking.Update(this);
            AssetDragging.Update(this);
            UpdateBindings();
            foreach (Control control in EnumerateControls())
                if (Visible(control))
                    control.Update(frame.Elapsed);
            Commit();
            Layout();
            ValidateInteraction();
            Control? hover = RefreshHover();
            UpdateToolTip(frame.Elapsed, hover);
            _hasFrame = true;
        }
        finally
        {
            VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
            _busy = false;
        }
    }

    /// <summary>Draws the last processed frame with the workspace-owned renderer.</summary>
    /// <param name="context">The application draw context supplying timing and graphics.</param>
    /// <param name="includePickingOverlay">Whether to draw the color-picking overlay with the tree.</param>
    /// <exception cref="InvalidOperationException">The workspace has no host or matching processed frame.</exception>
    public void Draw(GameContext context, bool includePickingOverlay = true)
    {
        if (GetHostDrawContext(context) is { } draw)
        {
            draw.SceneContext = context as Sachssoft.Engine.Scenes.SceneDrawContext;
            try { Draw(draw, includePickingOverlay); }
            finally { draw.SceneContext = null; }
        }
    }

    /// <summary>Draws the last processed frame directly from the MonoGame draw loop.</summary>
    /// <param name="gameTime">The current game time.</param>
    /// <param name="includePickingOverlay">Whether to draw the color-picking overlay with the tree.</param>
    public void Draw(GameTime gameTime, bool includePickingOverlay = true)
    {
        ArgumentNullException.ThrowIfNull(gameTime);
        GameApplicationBase application = GetHost();
        if (GetHostDrawContext(application, application.GraphicsDevice, gameTime) is { } draw)
            Draw(draw, includePickingOverlay);
    }

    private ControlDrawContext? GetHostDrawContext(GameContext context)
    {
        GameApplicationBase application = GetHost(context, out GraphicsDevice device);
        return GetHostDrawContext(application, device, context.GameTime);
    }

    private ControlDrawContext? GetHostDrawContext(GameApplicationBase application, GraphicsDevice device, GameTime gameTime)
    {
        if (!_hasFrame || !ReferenceEquals(device, _skinDevice))
            throw new InvalidOperationException("Process a UI frame on the current graphics device before drawing.");

        Rectangle viewport = _frameViewport ?? device.Viewport.Bounds;
        if (viewport.Width <= 0 || viewport.Height <= 0)
            return null;
        float scale = _frameDpi / 96f;
        if (_hostDraw is null || !ReferenceEquals(_hostDrawDevice, device))
        {
            _hostDraw?.Dispose();
            _hostDraw = null;
            _hostDraw = new ControlDrawContext(application, _layout!.DefaultFont, viewport, scale, useWorkspaceFont: true);
            _hostDrawDevice = device;
        }
        else if (_frameViewport is null)
        {
            _hostDraw.SetViewport(viewport, scale);
        }
        _hostDraw.SetFrameTime(gameTime);
        return _hostDraw;
    }

    /// <summary>
    /// Draws the committed tree. Requires a preceding ProcessFrame with the same size and font.
    /// </summary>
    public void Draw(ControlDrawContext context, bool includePickingOverlay = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(context);
        if (_busy || !_hasFrame)
            throw new InvalidOperationException("Process a UI frame before drawing; drawing cannot be reentered.");
        SynchronizeDrawing(context);
        if (!Size.Equals(context.LogicalSize) || !ReferenceEquals(_layout?.DefaultFont, context.DefaultFont))
            throw new InvalidOperationException("Drawing must use the processed viewport size and default font.");
        _busy = true;
        VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
        _drawing = true;
        try
        {
            context.Bind(Registry);
            context.Begin();
            try
            {
                DrawTree(Content, new Bounds2(0, 0, Size.Width, Size.Height), 1f, context, Matrix.Identity);
                DrawOverlays(context);
                if (includePickingOverlay)
                    ColorPicking.Draw(context, Size);
                AssetDragging.Draw(context, Size);
                if (_visibleToolTip is ToolTip tip)
                    DrawTree(tip, new Bounds2(0, 0, Size.Width, Size.Height), 1, context, Matrix.Identity);
            }
            finally
            {
                context.End();
            }
        }
        finally
        {
            _drawing = false;
            VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
            _busy = false;
        }
    }

    private void SynchronizeDrawing(ControlDrawContext context)
    {
        if (_layout is { } layout)
            context.UseLayoutFont(layout.DefaultFont);
        if (_frameViewport is Rectangle viewport)
            context.SetViewport(viewport, _frameDpi / 96f);
    }

    /// <summary>Draws the pipette overlay with the workspace-owned renderer.</summary>
    /// <param name="context">The application draw context supplying timing and graphics.</param>
    public void DrawColorPickingOverlay(GameContext context)
    {
        if (GetHostDrawContext(context) is { } draw)
            DrawColorPickingOverlay(draw);
    }

    /// <summary>
    /// Draws only the pipette overlay after presenting a clean sampled render target.
    /// </summary>
    public void DrawColorPickingOverlay(ControlDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_busy && _hasFrame)
            SynchronizeDrawing(context);
        if (_busy || !_hasFrame || !Size.Equals(context.LogicalSize) || !ReferenceEquals(_layout?.DefaultFont, context.DefaultFont))
            throw new InvalidOperationException("Process a matching UI frame before drawing the picking overlay.");
        _busy = true;
        VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
        _drawing = true;
        try
        {
            context.Bind(Registry);
            context.Begin();
            try
            {
                ColorPicking.Draw(context, Size);
            }
            finally
            {
                context.End();
            }
        }
        finally
        {
            _drawing = false;
            VisualInvalidationEpoch = unchecked(VisualInvalidationEpoch + 1);
            _busy = false;
        }
    }

    internal bool DropAsset(AssetItem asset, Vector2 position)
    {
        Control? hit = HitTarget(position);
        while (hit is not null)
        {
            if (hit is ReferencePicker picker && picker.IsAvailable() && picker.CanSelectAsset(asset))
            {
                picker.SelectAsset(asset);
                return true;
            }

            hit = hit.Parent;
        }

        return false;
    }

    /// <summary>
    /// Assigns keyboard focus to a visible, enabled, focusable control in this root.
    /// </summary>
    public void Focus(Control? control)
    {
        if (_drawing)
            throw new InvalidOperationException("Drawing cannot change keyboard focus.");
        if (control is not null && (control.Workspace != this || !control.IsAvailable() || !control.IsFocusable || !IsInActiveScope(control)))
            throw new InvalidOperationException("The control cannot receive focus.");
        if (FocusedControl == control)
            return;
        Control? previous = FocusedControl;
        FocusedControl = control;
        previous?.LoseFocus();
        previous?.NotifyFocus();
        control?.NotifyFocus();
    }

    internal void CapturePointer(Control control) => CapturePointer(control, ControlPointer.Mouse);
    internal void CapturePointer(Control control, ControlPointer pointer)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (control.Workspace != this || !control.IsAvailable(true) || !IsInActiveScope(control))
            throw new InvalidOperationException("The control cannot capture input.");
        Control? previous = GetCapturedControl(pointer);
        if (previous == control)
            return;
        _captures[pointer] = control;
        previous?.PointerCaptureLost(pointer);
    }

    internal void ReleasePointer(Control control) => ReleasePointer(control, ControlPointer.Mouse);
    internal void ReleasePointer(Control control, ControlPointer pointer)
    {
        if (GetCapturedControl(pointer) != control)
            return;
        _captures.Remove(pointer);
        control.PointerCaptureLost(pointer);
    }

    private void ReleasePointers(Func<Control, bool> predicate)
    {
        if (_captures.Count == 0)
            return;
        var pointers = _captures.Keys.OrderBy(p => p.Kind).ThenBy(p => p.Id).ToArray();
        foreach (var pointer in pointers)
            if (GetCapturedControl(pointer)is Control owner && predicate(owner))
                ReleasePointer(owner, pointer);
    }

    internal bool IsTraversing => _busy && !_committing;

    private readonly HashSet<Control> _rootReservations = new(ReferenceEqualityComparer.Instance);
    private void ReserveRoot(Control control)
    {
        VerifyTreeMutation();
        control.Workspace?.VerifyTreeMutation();
        if (control.Parent is not null || control.RootOwner is not null)
            throw new InvalidOperationException("The control already has an owner.");
        if (control.CommittedParent is not null && control.Workspace?.IsTraversing == true && control.Workspace != this)
            throw new InvalidOperationException("Wait for the previous workspace to commit this control's removal.");
        _rootReservations.Add(control);
        control.RootOwner = this;
    }

    private void ReleaseRoot(Control control)
    {
        _rootReservations.Remove(control);
        if (ReferenceEquals(control.RootOwner, this))
            control.RootOwner = null;
    }

    internal void VerifyTreeMutation()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_drawing)
            throw new InvalidOperationException("The control tree cannot change while drawing.");
    }

    internal void Mutate(Action action)
    {
        VerifyTreeMutation();
        if (_busy && !_committing)
            _pending.Add(action);
        else
            action();
    }

    private void Commit()
    {
        if (_pending.Count == 0)
            return;
        Action[] batch = _pending.ToArray();
        _pending.Clear();
        _committing = true;
        try
        {
            foreach (Action action in batch)
                action();
        }
        finally
        {
            _committing = false;
        }
    }

    /// <summary>Prepares a control tree or follows only branches with pending preparation.</summary>
    /// <param name="control">The root of the subtree.</param>
    /// <param name="dirtyOnly">Whether to skip branches without pending preparation.</param>
    internal static void PrepareTree(Control control, bool dirtyOnly = false)
    {
        if (!control.BeginPreparation(dirtyOnly))
            return;

#if LAYOUT_METRICS
        LayoutMetrics.Prepared++;
#endif
        control.Prepare();
        if (control is ContainerControl container)
            foreach (Control child in container.OrderedChildren)
                if (ReferenceEquals(child.CommittedParent, container) && ReferenceEquals(child.Parent, container))
                    PrepareTree(child, dirtyOnly);
    }

    private void Layout()
    {
        Commit();
        if (_fontsDirty)
        {
            unchecked
            {
                FontRevision++;
            }

            _fontsDirty = false;
        }

        Registry.Styles.PollChanges();
        if (_styleRevision != Registry.Styles.Revision)
        {
            InvalidateLayout();
            _styleRevision = Registry.Styles.Revision;
        }

        bool geometryChanged = _layoutDirty;
        if (_layoutDirty)
        {
#if LAYOUT_METRICS
            LayoutMetrics.Passes++;
#endif
            _layoutDirty = false;
            _layoutEpoch = unchecked(_layoutEpoch + 1);
            _layoutPassActive = true;
            try
            {
                RebuildControls();
                if (_prepareDirty)
                {
                    bool dirtyOnly = !_prepareAll;
                    _prepareDirty = false;
                    _prepareAll = false;
                    try
                    {
                        PrepareTree(Content, dirtyOnly);
                        foreach (ControlOverlay overlay in _overlays)
                            PrepareTree(overlay.Control, dirtyOnly);
                    }
                    catch
                    {
                        _prepareDirty = true;
                        _prepareAll = true;
                        throw;
                    }
                    RebuildControls();
                }

                bool measure = _measurePending || !Content.HasValidMeasure;
                _measurePending = false;
                if (measure)
                    Content.Measure(_layout!, Size);

                Content.Arrange(new Bounds2(0, 0, Size.Width, Size.Height));
                LayoutOverlays();
            }
            catch
            {
                _layoutDirty = true;
                throw;
            }
            finally
            {
                _layoutPassActive = false;
            }
        }

        int scrollCount = _scrollViewers.Count;
        for (int i = 0; i < scrollCount; i++)
        {
            ScrollViewer viewer = _scrollViewers.Dequeue();
            _queuedScrollViewers.Remove(viewer);
            if (ReferenceEquals(viewer.Workspace, this))
            {
                try
                {
                    viewer.ArrangeScrollOffset();
                    geometryChanged = true;
                }
                catch
                {
                    QueueScrollOffset(viewer);
                    throw;
                }
            }
        }

        if (geometryChanged)
            RefreshHover();
    }

    private void ValidateInteraction()
    {
        if (FocusedControl is Control focus && (focus.Workspace != this || !focus.IsAvailable() || !focus.IsFocusable || !IsInActiveScope(focus)))
            Focus(null);
        foreach (var capture in _captures)
            if (capture.Value.Workspace != this || !capture.Value.IsAvailable(true) || !IsInActiveScope(capture.Value))
            {
                ReleasePointers(c => c.Workspace != this || !c.IsAvailable(true) || !IsInActiveScope(c));
                break;
            }
    }

    internal bool PointerHits(Control control, Vector2 position) => HitTarget(position)is Control target && Within(target, control);
    private bool CapturedByAnotherPointer(Control? target, ControlPointer pointer)
    {
        if (target is null || GetCapturedControl(pointer)is not null)
            return false;
        foreach (var capture in _captures)
            if (capture.Key != pointer && Within(target, capture.Value))
                return true;
        return false;
    }

    private readonly List<Control> _route = new(16);
    private ControlInputEventArgs? _routedArgs;
    private ControlOverlay? FindOverlay(Control control)
    {
        for (int i = _overlays.Count - 1; i >= 0; i--)
            if (_overlays[i].Control == control)
                return _overlays[i];
        return null;
    }

    private void Dispatch(ControlInputEvent input)
    {
        if (ColorPicking.Handle(input) || AssetDragging.Handle(this, input))
            return;
        _input.Pointer = input.Pointer;
        if (input.Type != ControlInputEventType.PointerMove || input.PointerKind != ControlPointerKind.Mouse)
        {
            _tipSuppressed = true;
            HideToolTip();
            _tipElapsed = 0;
        }
        else
            _tipSuppressed = false;
        if (input.Type == ControlInputEventType.PointerCancel)
        {
            if (GetCapturedControl(input.Pointer) is Control cancelTarget)
            {
                _route.Clear();
                for (Control? node = cancelTarget; node is not null; node = node.Parent)
                    _route.Add(node);
                var cancelledArgs = (_routedArgs ??= new ControlInputEventArgs(input, cancelTarget)).Reset(input, cancelTarget);
                for (int i = _route.Count - 1; i >= 0 && !cancelledArgs.Handled; i--)
                {
                    cancelledArgs.CurrentTarget = _route[i];
                    _route[i].PreviewInput(_input, cancelledArgs);
                }
            }
            if (GetCapturedControl(input.Pointer)is Control cancelled)
                ReleasePointer(cancelled, input.Pointer);
            if (input.PointerKind == ControlPointerKind.Touch)
                _hasPointer = false;
            return;
        }

        if (input.Type == ControlInputEventType.Deactivate)
        {
            Focus(null);
            ReleasePointers(static _ => true);
            foreach (Control control in EnumerateControls())
                control.CancelInteraction();
            _hasPointer = false;
            RefreshHover();
            return;
        }

        if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape && ActiveOverlay is Control popup && FindOverlay(popup)is { Popup: true })
        {
            ClosePopup(popup);
            return;
        }

        bool pointer = input.Type is ControlInputEventType.PointerMove or ControlInputEventType.PointerDown or ControlInputEventType.PointerUp or ControlInputEventType.PointerWheel;
        Control? target;
        if (pointer)
        {
            _pointer = input.Position;
            _hoverPointer = input.Pointer;
            _hasPointer = true;
            Control? hit = RefreshHover();
            if (input.Type == ControlInputEventType.PointerDown && DismissOutsidePopup(_pointer))
                return;
            target = GetCapturedControl(input.Pointer) ?? hit;
            if (ShowScrollBarsOnMouseHover && input.PointerKind == ControlPointerKind.Touch && target is ScrollBar bar)
                target = bar.Parent;

            // Older controls have one gesture state. Never let another contact steal it.
            if (CapturedByAnotherPointer(target, input.Pointer))
                return;
            if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
            {
                Control? focus = target;
                while (focus is not null && !focus.IsFocusable)
                    focus = focus.Parent;
                Focus(focus);
            }
        }
        else
        {
            if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Tab && !input.Control && !(FocusedControl is TextBox { AcceptsTab: true }))
            {
                if (!input.IsRepeat)
                    MoveFocus(input.Shift);
                return;
            }

            target = FocusedControl ?? ActiveOverlay ?? Content;
        }

        if (target is null)
            return;
        var route = _route;
        route.Clear();
        for (Control? node = target; node is not null; node = node.Parent)
            route.Add(node);
        var args = (_routedArgs ??= new ControlInputEventArgs(input, target)).Reset(input, target);
        for (int i = route.Count - 1; i >= 0 && !args.Handled; i--)
        {
            Control node = route[i];
            if (!node.IsAvailable()) break;
            args.SetTarget(node);
            node.PreviewInput(_input, args);
        }
        foreach (Control node in route)
        {
            if (args.Handled) break;
            if (!node.IsAvailable())
                break;
            args.SetTarget(node);
            node.Input(_input, args);
            if (args.Handled)
                break;
        }

        if (!args.Handled && input.Type == ControlInputEventType.KeyDown)
            foreach (Control node in EnumerateControls())
                if (node is MenuBar menu && IsInActiveScope(menu) && menu.HandleShortcut(input))
                    break;
        if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            if (GetCapturedControl(input.Pointer)is Control owner)
                ReleasePointer(owner, input.Pointer);
            if (input.PointerKind == ControlPointerKind.Touch)
                _hasPointer = false;
        }
    }

    private void MoveFocus(bool reverse)
    {
        Control[] controls = EnumerateControls().Where(c => c.IsAvailable() && c.IsFocusable && IsInActiveScope(c)).OrderBy(c => c.TabIndex).ToArray();
        if (controls.Length == 0)
        {
            Focus(null);
            return;
        }

        int index = Array.IndexOf(controls, FocusedControl);
        int next = index < 0 ? (reverse ? controls.Length - 1 : 0) : (index + (reverse ? controls.Length - 1 : 1)) % controls.Length;
        Focus(controls[next]);
    }

    /// <summary>Updates hover only on controls entering or leaving the pointer hit path.</summary>
    /// <returns>The hit target used for hover and pointer dispatch.</returns>
    private Control? RefreshHover()
    {
        Control? hit = null;
        if (_hasPointer)
        {
            // Once a scroll host owns the gesture, routing no longer needs the row beneath the pointer.
            Control? capture = PreferTouchInput ? GetCapturedControl(_hoverPointer) : null;
            if (capture is ScrollViewer { IsSwiping: true } or ListBox { IsSwiping: true })
                hit = _hoverPointer.Kind == ControlPointerKind.Mouse ? capture : null;
            else
                hit = HitTarget(_pointer);
        }

        _nextHoverPath.Clear();
        for (Control? node = hit; node is not null; node = node.Parent)
            _nextHoverPath.Add(node);

        int previous = _hoverPath.Count - 1;
        int next = _nextHoverPath.Count - 1;
        while (previous >= 0 && next >= 0 && _nextHoverPath[next].IsPointerOver &&
            ReferenceEquals(_hoverPath[previous], _nextHoverPath[next]))
        {
            previous--;
            next--;
        }

        for (int i = 0; i <= previous; i++)
            if (ReferenceEquals(_hoverPath[i].Workspace, this))
                _hoverPath[i].IsPointerOver = false;
        for (int i = next; i >= 0; i--)
            if (ReferenceEquals(_nextHoverPath[i].Workspace, this))
                _nextHoverPath[i].IsPointerOver = true;

        (_hoverPath, _nextHoverPath) = (_nextHoverPath, _hoverPath);
        _nextHoverPath.Clear();
        return hit;
    }

    /// <summary>Finds the topmost eligible control at a point inside the current clip.</summary>
    /// <param name="control">The root of the branch being searched.</param>
    /// <param name="clip">The ancestor clip in workspace coordinates.</param>
    /// <param name="point">The pointer in workspace coordinates.</param>
    /// <param name="parentTransform">The composed transform of the already visited ancestors.</param>
    /// <returns>The hit control, if any.</returns>
    private static Control? Hit(Control control, Bounds2 clip, Vector2 point, Matrix parentTransform)
    {
        if (control.Workspace is null || !control.IsVisible || !control.IsEnabled || !control.IsHitTestVisible ||
            !Control.Contains(clip, point.X, point.Y))
        {
            return null;
        }

        Matrix transform = ControlTransforms.Compose(control, parentTransform);
        Bounds2 visible = Control.Intersect(clip, ControlTransforms.TransformBounds(control.Bounds, transform));
        if (!Control.Contains(visible, point.X, point.Y))
            return null;
        if (transform != Matrix.Identity)
        {
            Vector2 local = Vector2.Transform(point, Matrix.Invert(transform));
            if (!Control.Contains(control.Bounds, local.X, local.Y))
                return null;
        }

        if (control is ContainerControl container)
        {
            Control[] children = container.OrderedChildren;
            for (int i = children.Length - 1; i >= 0; i--)
            {
                Control child = children[i];
                if (!ReferenceEquals(child.Parent, container))
                    continue;
                Bounds2 childClip = Control.Intersect(visible, ControlTransforms.TransformBounds(control.GetChildClip(child), transform));
                Control? hit = Hit(child, childClip, point, transform);
                if (hit is not null)
                    return hit;
            }
        }

        return control;
    }

    private static IEnumerable<Control> Walk(Control control)
    {
        yield return control;
        if (control is ContainerControl container)
            foreach (Control child in container.Children)
                foreach (Control descendant in Walk(child))
                    yield return descendant;
    }

    private static bool Visible(Control control)
    {
        for (Control? node = control; node is not null; node = node.Parent)
            if (!node.IsVisible)
                return false;
        return true;
    }

    /// <summary>Paints a visible control and its children in their established drawing order.</summary>
    /// <param name="control">The control being painted.</param>
    /// <param name="clip">The ancestor clip in workspace coordinates.</param>
    /// <param name="opacity">The combined opacity of the ancestors.</param>
    /// <param name="context">The active drawing context.</param>
    /// <param name="parentTransform">The composed transform of the ancestors.</param>
    private static void DrawTree(Control control, Bounds2 clip, float opacity, ControlDrawContext context, Matrix parentTransform)
    {
        if (!control.IsVisible || control is ScrollBar { IsPresentationVisible: false })
            return;
        Matrix transform = ControlTransforms.Compose(control, parentTransform);
        clip = Control.Intersect(clip, ControlTransforms.TransformBounds(control.Bounds, transform));
        opacity *= control.Opacity;
        if (clip.Width <= 0 || clip.Height <= 0 || opacity <= 0)
            return;
        int previousPlanes = context.PushVisualClip(control.Bounds, transform);
        try
        {
            context.SetControl(control.Bounds, clip, opacity, visualTransform: transform);
            control.Paint(context);
            if (control is ContainerControl container)
                foreach (Control child in container.OrderedChildren)
                {
                    Bounds2 childClip = control.GetChildClip(child);
                    int childPlanes = context.PushVisualClip(childClip, transform);
                    try { DrawTree(child, Control.Intersect(clip, ControlTransforms.TransformBounds(childClip, transform)), opacity, context, transform); }
                    finally { context.RestoreVisualClip(childPlanes); }
                }
            context.SetControl(control.Bounds, clip, opacity, visualTransform: transform);
            control.PaintOverlay(context);
            if (control is IControlAdornment adorned) adorned.DrawAdornment(context);
        }
        finally { context.RestoreVisualClip(previousPlanes); }
    }

    private readonly List<ControlOverlay> _overlays = new();
    private readonly ConcurrentQueue<Action> _posted = new();
    private bool _committing;
    /// <summary>
    /// Gets pointer position.
    /// </summary>
    public Vector2 PointerPosition => _pointer;
    /// <summary>
    /// Gets overlay count.
    /// </summary>
    public int OverlayCount => _overlays.Count;
    private Control? ActiveOverlay => _overlays.Count == 0 ? null : _overlays[^1].Control;

    /// <summary>
    /// Performs post using the current control state.
    /// </summary>
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _posted.Enqueue(action);
    }

    private readonly List<Control> _controls = new();
    internal List<Control> EnumerateControls() => _controls;
    private void RebuildControls()
    {
        if (!_controlsDirty)
            return;
        _controlsDirty = false;
        _controls.Clear();
        Collect(Content);
        for (int i = 0; i < _overlays.Count; i++)
            Collect(_overlays[i].Control);
    }

    private void Collect(Control control)
    {
        _controls.Add(control);
        if (control is ContainerControl c)
            for (int i = 0; i < c.Children.Count; i++)
                Collect(c.Children[i]);
    }

    private static bool Within(Control control, Control ancestor)
    {
        for (Control? n = control; n is not null; n = n.Parent)
            if (n == ancestor)
                return true;
        return false;
    }

    internal bool IsInActiveScope(Control control)
    {
        int modal = _overlays.FindLastIndex(e => e.Modal);
        if (modal < 0)
            return true;
        for (int i = modal; i < _overlays.Count; i++)
            if (Within(control, _overlays[i].Control))
                return true;
        return false;
    }

    /// <summary>Releases interactions owned by a row before it is assigned to different data.</summary>
    /// <param name="row">The recycled presentation.</param>
    /// <param name="selector">The optional control that should retain keyboard focus.</param>
    internal void ResetRecycledInteraction(Control row, Control? selector)
    {
        if (FocusedControl is { } focused && Within(focused, row))
            Focus(selector is { IsFocusable: true } && selector.IsAvailable() && IsInActiveScope(selector) ? selector : null);
        if (_captures.Count > 0)
        {
            foreach (Control capture in _captures.Values)
                if (Within(capture, row))
                {
                    ReleasePointers(control => Within(control, row));
                    break;
                }
        }
    }

    /// <summary>
    /// Performs show popup using the current control state.
    /// </summary>
    public void ShowPopup(Control popup, Bounds2 placement, Control? owner = null, Action? closed = null)
    {
        ArgumentNullException.ThrowIfNull(popup);
        if (owner is not null && owner.Workspace != this)
            throw new InvalidOperationException("Popup owner belongs to another workspace.");
        ReserveRoot(popup);
        Mutate(() => AddOverlay(popup, placement, false, true, owner, closed));
    }

    /// <summary>
    /// Performs show window using the current control state.
    /// </summary>
    public void ShowWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ReserveRoot(window);
        Mutate(() => AddOverlay(window, Bounds2.Zero, false, false, null, null));
    }

    /// <summary>
    /// Performs show dialog using the current control state.
    /// </summary>
    public Task<DialogResult> ShowDialog(Dialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        ReserveRoot(dialog);
        Task<DialogResult> task;
        try
        {
            task = dialog.Prepare(this);
        }
        catch
        {
            ReleaseRoot(dialog);
            throw;
        }

        Mutate(() =>
        {
            AddOverlay(dialog, Bounds2.Zero, true, false, null, null);
            try
            {
                dialog.Open();
            }
            catch
            {
                CloseOverlay(dialog);
                throw;
            }
        });
        return task;
    }

    private void AddOverlay(Control control, Bounds2 placement, bool modal, bool popup, Control? owner, Action? closed)
    {
        if (!ReferenceEquals(control.RootOwner, this) || control.Parent is not null)
            throw new InvalidOperationException("Overlay reservation is no longer valid.");
        if (owner is not null && owner.Workspace != this)
        {
            ReleaseRoot(control);
            if (control is Dialog rejected)
                rejected.NotifyClosed();
            throw new InvalidOperationException("Popup owner belongs to another workspace.");
        }

        control.CommittedParent?.DetachCommittedChild(control);
        Control? old = FocusedControl;
        ReleasePointers(static _ => true);
        _overlays.Add(new ControlOverlay(control, placement, modal, popup, owner, old, closed));
        InvalidateControlTree();
        try
        {
            ContainerControl.Attach(control, this);
            PrepareTree(control);
            Focus(null);
            Control? focus = Walk(control).FirstOrDefault(c => c.IsAvailable() && c.IsFocusable);
            if (focus is not null)
                Focus(focus);
        }
        catch
        {
            CloseOverlay(control);
            throw;
        }
    }

    /// <summary>
    /// Performs close popup using the current control state.
    /// </summary>
    public void ClosePopup(Control popup) => Mutate(() => CloseOverlay(popup));
    internal void CloseOverlay(Control control)
    {
        ControlOverlay? entry = _overlays.FirstOrDefault(e => e.Control == control);
        if (entry is null)
            return;
        foreach (ControlOverlay child in _overlays.Where(e => e.Owner is not null && Within(e.Owner, control)).ToArray())
            CloseOverlay(child.Control);
        _overlays.Remove(entry);
        InvalidateControlTree();
        ContainerControl.Attach(control, null);
        ReleaseRoot(control);
        if (FocusedControl is Control focus && Within(focus, control))
            Focus(null);
        ReleasePointers(c => Within(c, control));
        if (entry.PreviousFocus is Control previous && previous.Workspace == this && previous.IsAvailable() && previous.IsFocusable && IsInActiveScope(previous))
            Focus(previous);
        entry.Closed?.Invoke();
        if (control is Window window)
            window.NotifyClosed();
    }

    private bool DismissOutsidePopup(Vector2 point)
    {
        ControlOverlay? popup = _overlays.LastOrDefault(e => e.Popup);
        if (popup is null || Control.Contains(popup.Control.Bounds, point.X, point.Y))
            return false;
        if (popup.Owner is Control owner && Control.Contains(owner.Bounds, point.X, point.Y))
            return false;
        while (popup.Owner is Control parent)
        {
            ControlOverlay? ancestor = _overlays.LastOrDefault(e => e.Popup && Within(parent, e.Control));
            if (ancestor is null || Control.Contains(ancestor.Control.Bounds, point.X, point.Y))
                break;
            popup = ancestor;
        }

        Control close = popup.Control;
        Mutate(() => CloseOverlay(close));
        return true;
    }

    private Control? HitTarget(Vector2 point)
    {
        var bounds = new Bounds2(0, 0, Size.Width, Size.Height);
        for (int i = _overlays.Count - 1; i >= 0; i--)
        {
            ControlOverlay e = _overlays[i];
            Control? hit = Hit(e.Control, bounds, point, Matrix.Identity);
            if (hit is not null)
                return hit;
            if (e.Modal)
                return null;
        }

        return Hit(Content, bounds, point, Matrix.Identity);
    }

    private void LayoutOverlays()
    {
        foreach (ControlOverlay e in _overlays)
        {
            e.Control.Measure(_layout!, Size);
            Bounds2 slot = e.Placement;
            if (e.Control is Window window)
            {
                float w = MathF.Min(Size.Width, window.DesiredSize.Width), h = MathF.Min(Size.Height, window.DesiredSize.Height);
                Vector2 position = window.Position ?? new Vector2((Size.Width - w) / 2, (Size.Height - h) / 2);
                slot = new Bounds2(Math.Clamp(position.X, 0, MathF.Max(0, Size.Width - w)), Math.Clamp(position.Y, 0, MathF.Max(0, Size.Height - h)), w, h);
            }
            else
            {
                float w = MathF.Min(Size.Width, slot.Width > 0 ? slot.Width : e.Control.DesiredSize.Width), h = MathF.Min(Size.Height, slot.Height > 0 ? slot.Height : e.Control.DesiredSize.Height);
                slot = new Bounds2(Math.Clamp(slot.X, 0, MathF.Max(0, Size.Width - w)), Math.Clamp(slot.Y, 0, MathF.Max(0, Size.Height - h)), w, h);
            }

            e.Control.Arrange(slot, true);
        }
    }

    private void DrawOverlays(ControlDrawContext context)
    {
        var bounds = new Bounds2(0, 0, Size.Width, Size.Height);
        foreach (ControlOverlay entry in _overlays)
        {
            if (ColorPicking.IsActive && ReferenceEquals(entry.Control, ColorPicking.Owner))
                continue;
            context.SetControl(bounds, bounds, 1);
            if (entry.Modal && !context.DrawSkinRegion("modal_overlay", bounds))
                context.FillRectangle(bounds, new Color(0, 0, 0, 160));
            Bounds2 b = entry.Control.Bounds;
            if (entry.Control is not MenuBar { UseSkin: false })
                context.DrawSkinPart(entry.Control, "Shadow", new Bounds2(b.X - 8, b.Y - 8, b.Width + 16, b.Height + 16));
            DrawTree(entry.Control, bounds, 1, context, Matrix.Identity);
        }
    }

    private ToolTip? _visibleToolTip;
    private Control? _tipOwner;
    private string _tipText = string.Empty;
    private Control? _tipContent;
    private double _tipElapsed;
    private bool _tipSuppressed;
    /// <summary>
    /// Gets the currently drawn noninteractive hover tooltip.
    /// </summary>
    public ToolTip? VisibleToolTip => _visibleToolTip;

    private void HideToolTip()
    {
        if (_visibleToolTip is not ToolTip tip)
            return;
        ContainerControl.Attach(tip, null);
        ReleaseRoot(tip);
        _visibleToolTip = null;
    }

    private void UpdateToolTip(TimeSpan elapsed, Control? hover)
    {
        Control? owner = null;
        if (_hasPointer && !_tipSuppressed && _captures.Count == 0)
            owner = PreferTouchInput ? hover : HitTarget(_pointer);
        while (owner is not null && owner.ToolTip.Length == 0 && owner.ToolTipContent is null)
            owner = owner.Parent;
        if (owner is not null && (!IsInActiveScope(owner) || PreferTouchInput && !owner.IsAvailable(true)))
            owner = null;
        string text = owner?.ToolTip ?? string.Empty;
        Control? content = owner?.ToolTipContent;
        if (owner != _tipOwner || text != _tipText || !ReferenceEquals(content, _tipContent))
        {
            HideToolTip();
            _tipOwner = owner;
            _tipText = text;
            _tipContent = content;
            _tipElapsed = 0;
        }

        if (owner is null)
        {
            HideToolTip();
            return;
        }

        var d = owner;
        if (!float.IsFinite(d.ConfigToolTipDelay) || d.ConfigToolTipDelay < 0 || !float.IsFinite(d.ConfigToolTipDuration) || d.ConfigToolTipDuration < 0 || !float.IsFinite(d.ConfigToolTipMaxWidth) || d.ConfigToolTipMaxWidth <= 0)
            throw new InvalidOperationException("Invalid tooltip timing or width.");
        _tipElapsed += elapsed.TotalSeconds;
        if (_tipElapsed < d.ConfigToolTipDelay || d.ConfigToolTipDuration > 0 && _tipElapsed >= d.ConfigToolTipDelay + d.ConfigToolTipDuration)
        {
            HideToolTip();
            return;
        }

        if (_visibleToolTip is null)
        {
            ToolTip tip = content is not null ? owner.ToolTipPresenter : new ToolTip
            {
                Content = new Label
                {
                    Text = text,
                    WrapText = true
                }
            };
            ReserveRoot(tip);
            ContainerControl.Attach(tip, this);
            _visibleToolTip = tip;
        }

        PrepareTree(_visibleToolTip);
        foreach (Control node in Walk(_visibleToolTip))
            if (Visible(node))
                node.Update(elapsed);
        _visibleToolTip.Measure(_layout!, new Size2(MathF.Min(Size.Width, d.ConfigToolTipMaxWidth), Size.Height));
        float w = MathF.Min(Size.Width, _visibleToolTip.DesiredSize.Width), h = MathF.Min(Size.Height, _visibleToolTip.DesiredSize.Height);
        float x = Math.Clamp(_pointer.X + 16, 0, MathF.Max(0, Size.Width - w)), y = _pointer.Y + 22;
        if (y + h > Size.Height)
            y = MathF.Max(0, _pointer.Y - h - 10);
        _visibleToolTip.Arrange(new Bounds2(x, y, w, h), true);
    }

    private bool _disposed;
    /// <summary>
    /// Detaches the tree and releases subscriptions, the owned renderer, and internally loaded skins.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_busy)
            throw new InvalidOperationException("Cannot dispose a root during update or draw.");
        ColorPicking.Cancel();
        AssetDragging.Cancel();
        Focus(null);
        ReleasePointers(static _ => true);
        HideToolTip();
        while (_overlays.Count > 0)
        {
            var control = _overlays[^1].Control;
            if (control is Dialog dialog)
                dialog.Complete(DialogResult.Cancel);
            else
                CloseOverlay(control);
        }

        for (int i = 0; i < Bindings.Count; i++)
            if (Bindings[i] is IDisposable binding)
                binding.Dispose();
        Bindings.Clear();
        ContainerControl.Attach(Content, null);
        foreach (Control reserved in _rootReservations.ToArray())
        {
            ContainerControl.Attach(reserved, null);
            ReleaseRoot(reserved);
            if (reserved is Dialog pendingDialog)
                pendingDialog.NotifyClosed();
        }

        _hostDraw?.Dispose();
        _hostDraw = null;
        _hostDrawDevice = null;
        _hostInput = null;
        _hostLayout = null;
        _hostApplication = null;
        ReleaseOwnedSkins();
        _skins?.Clear();
        _controls.Clear();
        _hoverPath.Clear();
        _nextHoverPath.Clear();
        _scrollViewers.Clear();
        _queuedScrollViewers.Clear();
        _pending.Clear();
        while (_posted.TryDequeue(out _))
        {
        };
        _disposed = true;
    }
}

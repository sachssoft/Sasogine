using Sachssoft.Engine.Graphics.Text;
using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Base of persistent controls with observable properties and separate runtime state.</summary>
/// <remarks>Controls belong to one UI thread. Drawing must not mutate propertiess or interaction state.</remarks>
public abstract class Control : Element
{
    private string _styleId = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs StyleIdChangedArgs = new(nameof(StyleId));
    private StyleRegistry? _appliedRegistry;
    private int _appliedRevision = -1, _appliedConverters = -1;
    private bool _applyingStyle;
    /// <summary>Gets or sets the central style ID; empty uses this control's default style.</summary>
    public string StyleId
    {
        get => _styleId;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            SetNotify(ref _styleId, value, StyleIdChangedArgs);
        }
    }

    /// <summary>Gets or sets an application-owned identifier.</summary>
    public string? Id { get; set; }

    internal void EnsureStyle()
    {
        var registry = Workspace?.Registry ?? FactoryRegistry ?? DetachedRegistry;
        if (_applyingStyle || ReferenceEquals(_appliedRegistry, registry.Styles) && _appliedRevision == registry.Styles.Revision && _appliedConverters == registry.Converters.Revision)
            return;
        _applyingStyle = true;
        try
        {
            registry.Styles.Validate(DefaultStyleName);
            if (StyleId.Length > 0)
                registry.Styles.Validate(StyleId);
            ResetStyledValues();
            registry.Styles.ApplyChain(this, DefaultStyleName);
            if (StyleId.Length > 0 && StyleId != DefaultStyleName)
                registry.Styles.ApplyChain(this, StyleId);
            CommitStyledValues();
            _appliedRegistry = registry.Styles;
            _appliedRevision = registry.Styles.Revision;
            _appliedConverters = registry.Converters.Revision;
        }
        finally
        {
            _applyingStyle = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(StyleId))
            _appliedRevision = -1;
        if (notification.PropertyName == nameof(ZIndex))
            Parent?.InvalidateOrder();
        if (notification.PropertyName is nameof(Opacity) or nameof(Background) or nameof(BorderColor) or nameof(TextColor) or nameof(HoverBackground) or nameof(PressedBackground) or nameof(DisabledBackground) or nameof(DisabledTextColor) or nameof(FocusColor) or "IsPressed" or "IsReleased" or "IsHeld" or "IsFocused" or "IsHovered" or "IsPointerOver")
            return;
        InvalidateMeasure();
    }

    private static readonly UIRegistry DetachedRegistry = new();
    internal UIRegistry? FactoryRegistry { get; set; }
    /// <summary>Gets the central standard style ID for this control kind.</summary>
    protected virtual string DefaultStyleName => "Control";
    /// <summary>Gets the explicit converters belonging to this control's registry.</summary>
    protected ValueConverterRegistry Converters => (Workspace?.Registry ?? FactoryRegistry ?? DetachedRegistry).Converters;
    /// <summary>Gets or sets tooltip text on any control, including scroll viewers.</summary>
    public string ToolTip { get => ConfigToolTip; set => ConfigToolTip = value; }
    /// <summary>Gets or sets tooltip content reserved by a stable tooltip presenter.</summary>
    public Control? ToolTipContent { get => ConfigToolTipContent; set => ConfigToolTipContent = value; }

    internal virtual void Prepare()
    {
        EnsureStyle();
        // Derived preparation must never reuse metrics from a retired workspace font cache.
        if (Workspace?.LayoutFont is IControlFont fallback)
        {
            try
            {
                EnsureFont(fallback);
            }
            catch
            {
                InvalidateMeasure();
                throw;
            }
        }
    }

    /// <summary>Gets the effective IsVisible value or sets a local override.</summary>
    public bool IsVisible { get => ConfigIsVisible; set => ConfigIsVisible = value; }
    /// <summary>Gets the effective IsEnabled value or sets a local override.</summary>
    public virtual bool IsEnabled { get => ConfigIsEnabled; set => ConfigIsEnabled = value; }
    /// <summary>Gets the effective IsHitTestVisible value or sets a local override.</summary>
    public bool IsHitTestVisible { get => ConfigIsHitTestVisible; set => ConfigIsHitTestVisible = value; }
    /// <summary>Gets the effective IsFocusable value or sets a local override.</summary>
    public bool IsFocusable { get => ConfigIsFocusable; set => ConfigIsFocusable = value; }
    /// <summary>Gets the effective ZIndex value or sets a local override.</summary>
    public int ZIndex { get => ConfigZIndex; set => ConfigZIndex = value; }
    /// <summary>Gets the effective TabIndex value or sets a local override.</summary>
    public int TabIndex { get => ConfigTabIndex; set => ConfigTabIndex = value; }
    /// <summary>Gets the effective Width value or sets a local override.</summary>
    public float? Width { get => ConfigWidth; set => ConfigWidth = value; }
    /// <summary>Gets the effective Height value or sets a local override.</summary>
    public float? Height { get => ConfigHeight; set => ConfigHeight = value; }
    /// <summary>Gets the effective Margin value or sets a local override.</summary>
    public Insets2 Margin { get => ConfigMargin; set => ConfigMargin = value; }
    /// <summary>Gets the effective Padding value or sets a local override.</summary>
    public Insets2 Padding { get => ConfigPadding; set => ConfigPadding = value; }
    /// <summary>Gets the effective HorizontalAlignment value or sets a local override.</summary>
    public ControlAlignment HorizontalAlignment { get => ConfigHorizontalAlignment; set => ConfigHorizontalAlignment = value; }
    /// <summary>Gets the effective VerticalAlignment value or sets a local override.</summary>
    public ControlAlignment VerticalAlignment { get => ConfigVerticalAlignment; set => ConfigVerticalAlignment = value; }
    /// <summary>Gets the effective Opacity value or sets a local override.</summary>
    public float Opacity { get => ConfigOpacity; set => ConfigOpacity = value; }
    /// <summary>Gets the effective Background value or sets a local override.</summary>
    public Color Background { get => ConfigBackground; set => ConfigBackground = value; }
    /// <summary>Gets the effective BorderColor value or sets a local override.</summary>
    public Color BorderColor { get => ConfigBorderColor; set => ConfigBorderColor = value; }
    /// <summary>Gets the effective BorderThickness value or sets a local override.</summary>
    public float BorderThickness { get => ConfigBorderThickness; set => ConfigBorderThickness = value; }
    /// <summary>Gets the owning container, including reservations awaiting a layout boundary.</summary>
    public ContainerControl? Parent { get; internal set; }
    internal ContainerControl? CommittedParent { get; set; }
    internal uint OwnershipRevision { get; set; }
    internal uint CommittedOwnershipRevision { get; set; }
    internal Workspace? RootOwner { get; set; }

    internal void VerifyTreeMutation()
    {
        for (Control? node = this; node is not null; node = node.Parent)
            if ((node.Workspace ?? node.RootOwner)is Workspace root)
            {
                root.VerifyTreeMutation();
                return;
            }
    }

    private Workspace? _workspace;
    /// <summary>Gets the workspace that currently owns this control.</summary>
    public Workspace? Workspace
    {
        get => _workspace;
        internal set
        {
            if (ReferenceEquals(_workspace, value))
                return;
            if (_workspace is not null)
                OnDetached();
            _workspace = value;
            _appliedRevision = -1;
            if (_workspace is not null)
                OnAttached();
        }
    }

    /// <summary>Invalidates layout after a change in an external, non-observable resource.</summary>
    public virtual void InvalidateLayout() => InvalidateMeasure();
    /// <summary>Connects resources when this control joins a workspace.</summary>
    protected virtual void OnAttached()
    {
    }

    /// <summary>Disconnects resources when this control leaves its root.</summary>
    protected virtual void OnDetached()
    {
    }

    /// <summary>Gets measured size including margin.</summary>
    public Size2 DesiredSize { get; private set; }
    /// <summary>Gets arranged bounds in root logical coordinates.</summary>
    public Bounds2 Bounds { get; private set; }
    /// <summary>Gets whether the control currently owns keyboard focus.</summary>
    public bool IsFocused => Workspace?.FocusedControl == this;

    /// <summary>Gets whether the pointer is over this control or its descendants.</summary>
    private bool _isPointerOver;
    private static readonly System.ComponentModel.PropertyChangedEventArgs PointerOverArgs = new(nameof(IsPointerOver)), HoveredArgs = new(nameof(IsHovered)), FocusedArgs = new(nameof(IsFocused));
    /// <summary>Gets or sets is pointer over.</summary>
    public bool IsPointerOver
    {
        get => _isPointerOver;
        internal set
        {
            if (SetNotify(ref _isPointerOver, value, PointerOverArgs))
                NotifyPropertyChanged(HoveredArgs);
        }
    }

    /// <summary>Gets whether the pointer is over this control or its descendants.</summary>
    public bool IsHovered => _isPointerOver;

    internal void NotifyFocus() => NotifyPropertyChanged(FocusedArgs);
    internal virtual Bounds2 ContentBounds => new(Bounds.X + Padding.Left, Bounds.Y + Padding.Top, MathF.Max(0, Bounds.Width - Padding.Horizontal), MathF.Max(0, Bounds.Height - Padding.Vertical));

    private bool _measureDirty = true, _arrangeDirty = true, _lastConstrain;
    private Size2 _lastAvailable;
    private IControlFont? _measureFont;
    private Bounds2 _lastSlot;
    /// <summary>Invalidates measurement for this control and its layout ancestors.</summary>
    protected internal void InvalidateMeasure()
    {
        _measureDirty = _arrangeDirty = true;
        Parent?.InvalidateMeasure();
        Workspace?.InvalidateLayout();
    }

    internal void Measure(ControlLayoutContext context, Size2 available)
    {
        try
        {
            EnsureFont(context.DefaultFont);
        }
        catch
        {
            InvalidateMeasure();
            throw;
        }

        if (!_measureDirty && _measureFontRevision == _fontResolutionRevision && _lastAvailable.Width == available.Width && _lastAvailable.Height == available.Height && ReferenceEquals(_measureFont, context.DefaultFont))
            return;
        _measureDirty = false;
        _measureFontRevision = _fontResolutionRevision;
        _arrangeDirty = true;
        _lastAvailable = available;
        _measureFont = context.DefaultFont;
        try
        {
            Validate();
            if (!IsVisible)
            {
                DesiredSize = Size2.Zero;
                return;
            }

            Insets2 margin = Margin, padding = Padding;
            float width = MathF.Max(0, available.Width - margin.Horizontal);
            float height = MathF.Max(0, available.Height - margin.Vertical);
            Size2 content = MeasureOverride(context, new Size2(MathF.Max(0, (Width ?? width) - padding.Horizontal), MathF.Max(0, (Height ?? height) - padding.Vertical)));
            ValidateSize(content, false);
            DesiredSize = new Size2(MathF.Max(MinWidth, Width ?? content.Width + padding.Horizontal) + margin.Horizontal, MathF.Max(MinHeight, Height ?? content.Height + padding.Vertical) + margin.Vertical);
            ValidateSize(DesiredSize, false);
        }
        catch
        {
            InvalidateMeasure();
            throw;
        }
    }

    internal void Arrange(Bounds2 slot, bool constrain = false)
    {
        if (!_arrangeDirty && _lastSlot.X == slot.X && _lastSlot.Y == slot.Y && _lastSlot.Width == slot.Width && _lastSlot.Height == slot.Height && _lastConstrain == constrain)
            return;
        _arrangeDirty = false;
        _lastSlot = slot;
        _lastConstrain = constrain;
        try
        {
            if (!IsVisible)
            {
                Bounds = Bounds2.Zero;
                return;
            }

            Insets2 m = Margin;
            float availableWidth = MathF.Max(0, slot.Width - m.Horizontal);
            float availableHeight = MathF.Max(0, slot.Height - m.Vertical);
            float width = Width ?? (HorizontalAlignment == ControlAlignment.Stretch ? availableWidth : MathF.Max(0, DesiredSize.Width - m.Horizontal));
            float height = Height ?? (VerticalAlignment == ControlAlignment.Stretch ? availableHeight : MathF.Max(0, DesiredSize.Height - m.Vertical));
            width = MathF.Max(MinWidth, width);
            height = MathF.Max(MinHeight, height);
            if (constrain)
            {
                width = MathF.Min(width, availableWidth);
                height = MathF.Min(height, availableHeight);
            }

            Bounds = new Bounds2(slot.X + m.Left + Align(availableWidth - width, HorizontalAlignment), slot.Y + m.Top + Align(availableHeight - height, VerticalAlignment), width, height);
            ArrangeOverride(ContentBounds);
        }
        catch
        {
            _arrangeDirty = true;
            Workspace?.InvalidateLayout();
            throw;
        }
    }

    /// <summary>Gets local drawing bounds.</summary>
    protected Bounds2 LocalBounds => new(0, 0, Bounds.Width, Bounds.Height);
    /// <summary>Gets the central style name for renderer parts.</summary>
    internal string StandardStyleName => DefaultStyleName;

    internal virtual Bounds2 GetChildClip(Control child) => ContentBounds;
    /// <summary>Gets or sets the minimum logical width.</summary>
    public float MinWidth { get => ConfigMinWidth; set => ConfigMinWidth = value; }
    /// <summary>Gets or sets the minimum logical height.</summary>
    public float MinHeight { get => ConfigMinHeight; set => ConfigMinHeight = value; }

    internal static bool Contains(Bounds2 bounds, float x, float y) => x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom;
    internal static Bounds2 Intersect(Bounds2 a, Bounds2 b)
    {
        float x = MathF.Max(a.Left, b.Left), y = MathF.Max(a.Top, b.Top);
        return new Bounds2(x, y, MathF.Max(0, MathF.Min(a.Right, b.Right) - x), MathF.Max(0, MathF.Min(a.Bottom, b.Bottom) - y));
    }

    internal bool IsAvailable(bool hitTest = false)
    {
        for (Control? node = this; node is not null; node = node.Parent)
            if (!node.IsVisible || !node.IsEnabled || (hitTest && !node.IsHitTestVisible))
                return false;
        return Workspace is not null;
    }

    internal void Input(ControlInputContext context, ControlInputEventArgs args) => OnInput(context, args);
    internal void Update(TimeSpan elapsed) => OnUpdate(elapsed);
    internal void CancelInteraction() => OnInteractionCancelled();
    internal void LoseFocus() => OnFocusLost();
    /// <summary>Cancels keyboard interaction when focus leaves; pointer-specific controls may retain capture.</summary>
    protected virtual void OnFocusLost() => OnInteractionCancelled();
    internal void PointerCaptureLost(ControlPointer pointer) => OnPointerCaptureLost(pointer);
    /// <summary>Called after this control loses capture for one pointer.</summary>
    protected virtual void OnPointerCaptureLost(ControlPointer pointer) => OnInteractionCancelled();
    /// <summary>Chooses the skin focus state for the control.</summary>
    protected virtual string FocusVisualState => "Focused";
    /// <summary>Gets whether the focus overlay may use an atlas skin part instead of a solid outline.</summary>
    protected virtual bool UseSkinFocus => true;

    internal void Paint(ControlDrawContext context)
    {
        OnDraw(context);
    }

    internal void PaintOverlay(ControlDrawContext context)
    {
        context.StrokeRectangle(LocalBounds, BorderColor, BorderThickness);
        if (IsFocused && (!UseSkinFocus || !context.DrawSkinPart(this, "Focus", LocalBounds, FocusVisualState)))
            context.StrokeRectangle(LocalBounds, ConfigFocusColor, 1);
    }

    /// <summary>Measures content inside the control's padding.</summary>
    protected virtual Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => Size2.Zero;
    /// <summary>Arranges children inside absolute logical content bounds.</summary>
    protected virtual void ArrangeOverride(Bounds2 contentBounds)
    {
    }

    /// <summary>Receives a routed input event; set Handled to stop bubbling.</summary>
    protected virtual void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
    }

    /// <summary>Updates runtime state using only the supplied elapsed time.</summary>
    protected virtual void OnUpdate(TimeSpan elapsed)
    {
    }

    /// <summary>Clears transient interaction when capture or focus becomes invalid.</summary>
    protected virtual void OnInteractionCancelled()
    {
    }

    /// <summary>Draws in local coordinates. The root owns batch and clipping scopes.</summary>
    protected virtual void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds))
            context.FillRectangle(LocalBounds, Background);
    }

    private static float Align(float spare, ControlAlignment alignment) => alignment switch
    {
        ControlAlignment.Center => spare / 2,
        ControlAlignment.End => spare,
        _ => 0
    };
    internal static void ValidateSize(Size2 value, bool allowInfinity)
    {
        if (float.IsNaN(value.Width) || float.IsNaN(value.Height) || value.Width < 0 || value.Height < 0 || (!allowInfinity && (!float.IsFinite(value.Width) || !float.IsFinite(value.Height))))
            throw new ArgumentOutOfRangeException(nameof(value), "Layout sizes must be nonnegative and finite.");
    }

    private void Validate()
    {
        if (!float.IsFinite(MinWidth) || !float.IsFinite(MinHeight) || MinWidth < 0 || MinHeight < 0)
            throw new InvalidOperationException("Invalid minimum size.");
        if (Width is float w && (!float.IsFinite(w) || w < 0))
            throw new InvalidOperationException("Invalid control width.");
        if (Height is float h && (!float.IsFinite(h) || h < 0))
            throw new InvalidOperationException("Invalid control height.");
        if (!float.IsFinite(Opacity) || Opacity < 0 || Opacity > 1)
            throw new InvalidOperationException("Opacity must be in [0, 1].");
        if (!float.IsFinite(BorderThickness) || BorderThickness < 0)
            throw new InvalidOperationException("Invalid border thickness.");
        ValidateInsets(Margin);
        ValidateInsets(Padding);
        if (!IsAlignment(HorizontalAlignment) || !IsAlignment(VerticalAlignment))
            throw new InvalidOperationException("Invalid control alignment.");
    }

    internal static bool IsAlignment(ControlAlignment value) => value is ControlAlignment.Start or ControlAlignment.Center or ControlAlignment.End or ControlAlignment.Stretch;
    private static void ValidateInsets(Insets2 value)
    {
        if (!float.IsFinite(value.Left) || !float.IsFinite(value.Top) || !float.IsFinite(value.Right) || !float.IsFinite(value.Bottom) || value.Left < 0 || value.Top < 0 || value.Right < 0 || value.Bottom < 0)
            throw new InvalidOperationException("Insets must be finite and nonnegative.");
    }

    private StyleProperty<bool> _styleIsVisible = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsVisibleChangedArgs = new(nameof(IsVisible));
    internal bool ConfigIsVisible
    {
        get => _styleIsVisible.Value;
        set
        {
            if (_styleIsVisible.SetLocal(value))
                NotifyPropertyChanged(IsVisibleChangedArgs);
        }
    }

    private StyleProperty<bool> _styleIsEnabled = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsEnabledChangedArgs = new(nameof(IsEnabled));
    internal bool ConfigIsEnabled
    {
        get => _styleIsEnabled.Value;
        set
        {
            if (_styleIsEnabled.SetLocal(value))
                NotifyPropertyChanged(IsEnabledChangedArgs);
        }
    }

    private StyleProperty<bool> _styleIsHitTestVisible = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsHitTestVisibleChangedArgs = new(nameof(IsHitTestVisible));
    internal bool ConfigIsHitTestVisible
    {
        get => _styleIsHitTestVisible.Value;
        set
        {
            if (_styleIsHitTestVisible.SetLocal(value))
                NotifyPropertyChanged(IsHitTestVisibleChangedArgs);
        }
    }

    private StyleProperty<bool> _styleIsFocusable = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsFocusableChangedArgs = new(nameof(IsFocusable));
    internal bool ConfigIsFocusable
    {
        get => _styleIsFocusable.Value;
        set
        {
            if (_styleIsFocusable.SetLocal(value))
                NotifyPropertyChanged(IsFocusableChangedArgs);
        }
    }

    private StyleProperty<int> _styleZIndex = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ZIndexChangedArgs = new(nameof(ZIndex));
    internal int ConfigZIndex
    {
        get => _styleZIndex.Value;
        set
        {
            if (_styleZIndex.SetLocal(value))
                NotifyPropertyChanged(ZIndexChangedArgs);
        }
    }

    private StyleProperty<int> _styleTabIndex = new(0);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TabIndexChangedArgs = new(nameof(TabIndex));
    internal int ConfigTabIndex
    {
        get => _styleTabIndex.Value;
        set
        {
            if (_styleTabIndex.SetLocal(value))
                NotifyPropertyChanged(TabIndexChangedArgs);
        }
    }

    private StyleProperty<float?> _styleWidth = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs WidthChangedArgs = new(nameof(Width));
    internal float? ConfigWidth
    {
        get => _styleWidth.Value;
        set
        {
            if (_styleWidth.SetLocal(value))
                NotifyPropertyChanged(WidthChangedArgs);
        }
    }

    private StyleProperty<float?> _styleHeight = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeightChangedArgs = new(nameof(Height));
    internal float? ConfigHeight
    {
        get => _styleHeight.Value;
        set
        {
            if (_styleHeight.SetLocal(value))
                NotifyPropertyChanged(HeightChangedArgs);
        }
    }

    private StyleProperty<Insets2> _styleMargin = new(Insets2.Zero);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MarginChangedArgs = new(nameof(Margin));
    internal Insets2 ConfigMargin
    {
        get => _styleMargin.Value;
        set
        {
            if (_styleMargin.SetLocal(value, Insets2EqualityComparer.Instance))
                NotifyPropertyChanged(MarginChangedArgs);
        }
    }

    private StyleProperty<Insets2> _stylePadding = new(Insets2.Zero);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PaddingChangedArgs = new(nameof(Padding));
    internal Insets2 ConfigPadding
    {
        get => _stylePadding.Value;
        set
        {
            if (_stylePadding.SetLocal(value, Insets2EqualityComparer.Instance))
                NotifyPropertyChanged(PaddingChangedArgs);
        }
    }

    private StyleProperty<ControlAlignment> _styleHorizontalAlignment = new(ControlAlignment.Stretch);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HorizontalAlignmentChangedArgs = new(nameof(HorizontalAlignment));
    internal ControlAlignment ConfigHorizontalAlignment
    {
        get => _styleHorizontalAlignment.Value;
        set
        {
            if (_styleHorizontalAlignment.SetLocal(value))
                NotifyPropertyChanged(HorizontalAlignmentChangedArgs);
        }
    }

    private StyleProperty<ControlAlignment> _styleVerticalAlignment = new(ControlAlignment.Stretch);
    private static readonly System.ComponentModel.PropertyChangedEventArgs VerticalAlignmentChangedArgs = new(nameof(VerticalAlignment));
    internal ControlAlignment ConfigVerticalAlignment
    {
        get => _styleVerticalAlignment.Value;
        set
        {
            if (_styleVerticalAlignment.SetLocal(value))
                NotifyPropertyChanged(VerticalAlignmentChangedArgs);
        }
    }

    private StyleProperty<float> _styleOpacity = new(1f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs OpacityChangedArgs = new(nameof(Opacity));
    internal float ConfigOpacity
    {
        get => _styleOpacity.Value;
        set
        {
            if (_styleOpacity.SetLocal(value))
                NotifyPropertyChanged(OpacityChangedArgs);
        }
    }

    private StyleProperty<Color> _styleBackground = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs BackgroundChangedArgs = new(nameof(Background));
    internal Color ConfigBackground
    {
        get => _styleBackground.Value;
        set
        {
            if (_styleBackground.SetLocal(value))
                NotifyPropertyChanged(BackgroundChangedArgs);
        }
    }

    private StyleProperty<Color> _styleBorderColor = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs BorderColorChangedArgs = new(nameof(BorderColor));
    internal Color ConfigBorderColor
    {
        get => _styleBorderColor.Value;
        set
        {
            if (_styleBorderColor.SetLocal(value))
                NotifyPropertyChanged(BorderColorChangedArgs);
        }
    }

    private StyleProperty<float> _styleBorderThickness = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs BorderThicknessChangedArgs = new(nameof(BorderThickness));
    internal float ConfigBorderThickness
    {
        get => _styleBorderThickness.Value;
        set
        {
            if (_styleBorderThickness.SetLocal(value))
                NotifyPropertyChanged(BorderThicknessChangedArgs);
        }
    }

    private ControlFontRequest _fontRequest;
    private IControlFont? _resolvedFont, _resolvedFallback;
    private Workspace? _resolvedFontWorkspace;
    private int _resolvedRegistryRevision = -1, _fontResolutionRevision, _measureFontRevision = -1;
    private bool _fontReady;
    /// <summary>Gets the font resolved by the last measurement. Null means no measurement has completed.</summary>
    public IControlFont? EffectiveFont => _resolvedFont;

    /// <summary>Gets the matching font for measuring text, resolving it only when its request changes.</summary>
    protected internal IControlFont GetFont(ControlLayoutContext context)
    {
        EnsureFont(context.DefaultFont);
        return _resolvedFont!;
    }

    /// <summary>Gets the last measured font for drawing without performing registry or cache lookups.</summary>
    protected internal IControlFont GetFont(ControlDrawContext context) => _resolvedFont ?? Font ?? context.DefaultFont;
    /// <summary>Invalidates derived text metrics when the resolved font request or font resources change.</summary>
    protected virtual void OnFontChanged()
    {
    }

    private void EnsureFont(IControlFont fallback)
    {
        Control? parent = Parent ?? Workspace?.FontParent(this);
        ControlFontRequest inherited = default;
        if (parent is not null)
        {
            parent.EnsureFont(fallback);
            inherited = parent._fontRequest;
        }

        // An explicit renderer adapter starts a new font scope; descriptor properties can refine it.
        if (Font is not null)
            inherited = default;
        var request = new ControlFontRequest(_styleFontName.Value ?? inherited.Name, _styleFontSize.Value ?? inherited.Size, _styleFontWeight.Value ?? inherited.Weight, _styleFontStyle.Value ?? inherited.Style, Font ?? inherited.Source);
        int revision = Workspace?.FontRevision ?? 0;
        if (_fontReady && _fontRequest == request && ReferenceEquals(_resolvedFallback, fallback) && ReferenceEquals(_resolvedFontWorkspace, Workspace) && _resolvedRegistryRevision == revision)
            return;
        IControlFont font;
        if (!request.HasDescriptor)
            font = request.Source ?? fallback;
        else if (Workspace is Workspace workspace)
            font = workspace.ResolveFont(request, fallback);
        else
            throw new InvalidOperationException("Attach a control to a Workspace before resolving named font properties.");
        // Commit only after successful resolution, so failed requests never become reusable cache entries.
        _resolvedFont = font;
        _fontRequest = request;
        _resolvedFallback = fallback;
        _resolvedFontWorkspace = Workspace;
        _resolvedRegistryRevision = revision;
        _fontReady = false;
        unchecked
        {
            _fontResolutionRevision++;
        }

        OnFontChanged();
        _fontReady = true;
    }

    private StyleProperty<string?> _styleFontName = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FontNameChangedArgs = new(nameof(FontName));
    /// <summary>Gets or sets the requested family name; null inherits from the parent.</summary>
    public string? FontName
    {
        get => _styleFontName.Value;
        set
        {
            VerifyTreeMutation();
            if (value is not null && string.IsNullOrWhiteSpace(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleFontName.SetLocal(value))
                NotifyPropertyChanged(FontNameChangedArgs);
        }
    }

    private StyleProperty<int?> _styleFontSize = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FontSizeChangedArgs = new(nameof(FontSize));
    /// <summary>Gets or sets the logical font size; null inherits from the parent or default font.</summary>
    public int? FontSize
    {
        get => _styleFontSize.Value;
        set
        {
            VerifyTreeMutation();
            if (value.HasValue && value.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleFontSize.SetLocal(value))
                NotifyPropertyChanged(FontSizeChangedArgs);
        }
    }

    private StyleProperty<FontWeight?> _styleFontWeight = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FontWeightChangedArgs = new(nameof(FontWeight));
    /// <summary>Gets or sets the requested engine weight; null inherits from the parent.</summary>
    public FontWeight? FontWeight
    {
        get => _styleFontWeight.Value;
        set
        {
            VerifyTreeMutation();
            if (value.HasValue && value.Value is not (Sachssoft.Engine.Graphics.Text.FontWeight.Normal or Sachssoft.Engine.Graphics.Text.FontWeight.Bold))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleFontWeight.SetLocal(value))
                NotifyPropertyChanged(FontWeightChangedArgs);
        }
    }

    private StyleProperty<FontStyle?> _styleFontStyle = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FontStyleChangedArgs = new(nameof(FontStyle));
    /// <summary>Gets or sets the requested engine style; null inherits from the parent.</summary>
    public FontStyle? FontStyle
    {
        get => _styleFontStyle.Value;
        set
        {
            VerifyTreeMutation();
            if (value.HasValue && value.Value is not (Sachssoft.Engine.Graphics.Text.FontStyle.Normal or Sachssoft.Engine.Graphics.Text.FontStyle.Italic))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleFontStyle.SetLocal(value))
                NotifyPropertyChanged(FontStyleChangedArgs);
        }
    }

    private StyleProperty<IControlFont?> _styleFont = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FontChangedArgs = new(nameof(Font));
    internal IControlFont? ConfigFont
    {
        get => _styleFont.Value;
        set
        {
            if (_styleFont.SetLocal(value))
                NotifyPropertyChanged(FontChangedArgs);
        }
    }

    /// <summary>Gets or sets Font; a local assignment overrides the style.</summary>
    public IControlFont? Font { get => ConfigFont; set => ConfigFont = value; }

    private StyleProperty<Color> _styleTextColor = new(Color.White);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TextColorChangedArgs = new(nameof(TextColor));
    internal Color ConfigTextColor
    {
        get => _styleTextColor.Value;
        set
        {
            if (_styleTextColor.SetLocal(value))
                NotifyPropertyChanged(TextColorChangedArgs);
        }
    }

    /// <summary>Gets or sets TextColor; a local assignment overrides the style.</summary>
    public Color TextColor { get => ConfigTextColor; set => ConfigTextColor = value; }

    private StyleProperty<Color> _styleHoverBackground = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HoverBackgroundChangedArgs = new(nameof(HoverBackground));
    internal Color ConfigHoverBackground
    {
        get => _styleHoverBackground.Value;
        set
        {
            if (_styleHoverBackground.SetLocal(value))
                NotifyPropertyChanged(HoverBackgroundChangedArgs);
        }
    }

    /// <summary>Gets or sets HoverBackground; a local assignment overrides the style.</summary>
    public Color HoverBackground { get => ConfigHoverBackground; set => ConfigHoverBackground = value; }

    private StyleProperty<Color> _stylePressedBackground = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PressedBackgroundChangedArgs = new(nameof(PressedBackground));
    internal Color ConfigPressedBackground
    {
        get => _stylePressedBackground.Value;
        set
        {
            if (_stylePressedBackground.SetLocal(value))
                NotifyPropertyChanged(PressedBackgroundChangedArgs);
        }
    }

    /// <summary>Gets or sets PressedBackground; a local assignment overrides the style.</summary>
    public Color PressedBackground { get => ConfigPressedBackground; set => ConfigPressedBackground = value; }

    private StyleProperty<Color> _styleDisabledBackground = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs DisabledBackgroundChangedArgs = new(nameof(DisabledBackground));
    internal Color ConfigDisabledBackground
    {
        get => _styleDisabledBackground.Value;
        set
        {
            if (_styleDisabledBackground.SetLocal(value))
                NotifyPropertyChanged(DisabledBackgroundChangedArgs);
        }
    }

    /// <summary>Gets or sets DisabledBackground; a local assignment overrides the style.</summary>
    public Color DisabledBackground { get => ConfigDisabledBackground; set => ConfigDisabledBackground = value; }

    private StyleProperty<Color> _styleFocusColor = new(Color.Transparent);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FocusColorChangedArgs = new(nameof(FocusColor));
    internal Color ConfigFocusColor
    {
        get => _styleFocusColor.Value;
        set
        {
            if (_styleFocusColor.SetLocal(value))
                NotifyPropertyChanged(FocusColorChangedArgs);
        }
    }

    /// <summary>Gets or sets FocusColor; a local assignment overrides the style.</summary>
    public Color FocusColor { get => ConfigFocusColor; set => ConfigFocusColor = value; }

    private StyleProperty<float> _styleMinWidth = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinWidthChangedArgs = new(nameof(MinWidth));
    internal float ConfigMinWidth
    {
        get => _styleMinWidth.Value;
        set
        {
            if (_styleMinWidth.SetLocal(value))
                NotifyPropertyChanged(MinWidthChangedArgs);
        }
    }

    private StyleProperty<float> _styleMinHeight = new(0f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinHeightChangedArgs = new(nameof(MinHeight));
    internal float ConfigMinHeight
    {
        get => _styleMinHeight.Value;
        set
        {
            if (_styleMinHeight.SetLocal(value))
                NotifyPropertyChanged(MinHeightChangedArgs);
        }
    }

    private StyleProperty<Color> _styleDisabledTextColor = new(new Color(97, 113, 133));
    private static readonly System.ComponentModel.PropertyChangedEventArgs DisabledTextColorChangedArgs = new(nameof(DisabledTextColor));
    internal Color ConfigDisabledTextColor
    {
        get => _styleDisabledTextColor.Value;
        set
        {
            if (_styleDisabledTextColor.SetLocal(value))
                NotifyPropertyChanged(DisabledTextColorChangedArgs);
        }
    }

    /// <summary>Gets or sets DisabledTextColor; a local assignment overrides the style.</summary>
    public Color DisabledTextColor { get => ConfigDisabledTextColor; set => ConfigDisabledTextColor = value; }

    private StyleProperty<string> _styleToolTip = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ToolTipChangedArgs = new(nameof(ToolTip));
    internal string ConfigToolTip
    {
        get => _styleToolTip.Value;
        set
        {
            if (_styleToolTip.SetLocal(value))
                NotifyPropertyChanged(ToolTipChangedArgs);
        }
    }

    private StyleProperty<Control?> _styleToolTipContent = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ToolTipContentChangedArgs = new(nameof(ToolTipContent));
    internal Control? ConfigToolTipContent
    {
        get => _styleToolTipContent.Value;
        set
        {
            StyleProperty<Control?> candidate = _styleToolTipContent;
            bool changed = candidate.SetLocal(value);
            CommitToolTipContent(candidate, changed);
        }
    }

    private ToolTip? _toolTipPresenter;
    internal ToolTip ToolTipPresenter => _toolTipPresenter ??= new ToolTip();

    private void CommitToolTipContent(StyleProperty<Control?> candidate, bool changed)
    {
        if (changed)
        {
            VerifyTreeMutation();
            for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
                if (ReferenceEquals(ancestor, candidate.Value))
                    throw new InvalidOperationException("Tooltip content cannot contain its owner or an ancestor.");
            if (candidate.Value is not null || _toolTipPresenter is not null)
                ToolTipPresenter.Content = candidate.Value;
        }

        _styleToolTipContent = candidate;
        if (changed)
            NotifyPropertyChanged(ToolTipContentChangedArgs);
    }

    private StyleProperty<float> _styleToolTipDelay = new(0.5f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ToolTipDelayChangedArgs = new(nameof(ToolTipDelay));
    internal float ConfigToolTipDelay
    {
        get => _styleToolTipDelay.Value;
        set
        {
            if (_styleToolTipDelay.SetLocal(value))
                NotifyPropertyChanged(ToolTipDelayChangedArgs);
        }
    }

    /// <summary>Gets or sets ToolTipDelay; a local assignment overrides the style.</summary>
    public float ToolTipDelay { get => ConfigToolTipDelay; set => ConfigToolTipDelay = value; }

    private StyleProperty<float> _styleToolTipDuration = new(6f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ToolTipDurationChangedArgs = new(nameof(ToolTipDuration));
    internal float ConfigToolTipDuration
    {
        get => _styleToolTipDuration.Value;
        set
        {
            if (_styleToolTipDuration.SetLocal(value))
                NotifyPropertyChanged(ToolTipDurationChangedArgs);
        }
    }

    /// <summary>Gets or sets ToolTipDuration; a local assignment overrides the style.</summary>
    public float ToolTipDuration { get => ConfigToolTipDuration; set => ConfigToolTipDuration = value; }

    private StyleProperty<float> _styleToolTipMaxWidth = new(320f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ToolTipMaxWidthChangedArgs = new(nameof(ToolTipMaxWidth));
    internal float ConfigToolTipMaxWidth
    {
        get => _styleToolTipMaxWidth.Value;
        set
        {
            if (_styleToolTipMaxWidth.SetLocal(value))
                NotifyPropertyChanged(ToolTipMaxWidthChangedArgs);
        }
    }

    /// <summary>Gets or sets ToolTipMaxWidth; a local assignment overrides the style.</summary>
    public float ToolTipMaxWidth { get => ConfigToolTipMaxWidth; set => ConfigToolTipMaxWidth = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public virtual void ApplyStyle(Style style)
    {
        ArgumentNullException.ThrowIfNull(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsVisible":
                    if (_styleIsVisible.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsVisibleChangedArgs);
                    break;
                case "IsEnabled":
                    if (_styleIsEnabled.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsEnabledChangedArgs);
                    break;
                case "IsHitTestVisible":
                    if (_styleIsHitTestVisible.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsHitTestVisibleChangedArgs);
                    break;
                case "IsFocusable":
                    if (_styleIsFocusable.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsFocusableChangedArgs);
                    break;
                case "ZIndex":
                    if (_styleZIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(ZIndexChangedArgs);
                    break;
                case "TabIndex":
                    if (_styleTabIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(TabIndexChangedArgs);
                    break;
                case "Width":
                    if (_styleWidth.Apply(Converters.Convert<float?>("NullableSingle", entry.Value)))
                        NotifyPropertyChanged(WidthChangedArgs);
                    break;
                case "Height":
                    if (_styleHeight.Apply(Converters.Convert<float?>("NullableSingle", entry.Value)))
                        NotifyPropertyChanged(HeightChangedArgs);
                    break;
                case "Margin":
                    if (_styleMargin.Apply(Converters.Convert<Insets2>("Insets2", entry.Value)))
                        NotifyPropertyChanged(MarginChangedArgs);
                    break;
                case "Padding":
                    if (_stylePadding.Apply(Converters.Convert<Insets2>("Insets2", entry.Value)))
                        NotifyPropertyChanged(PaddingChangedArgs);
                    break;
                case "HorizontalAlignment":
                    if (_styleHorizontalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(HorizontalAlignmentChangedArgs);
                    break;
                case "VerticalAlignment":
                    if (_styleVerticalAlignment.Apply(Converters.Convert<ControlAlignment>("ControlAlignment", entry.Value)))
                        NotifyPropertyChanged(VerticalAlignmentChangedArgs);
                    break;
                case "Opacity":
                    if (_styleOpacity.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(OpacityChangedArgs);
                    break;
                case "Background":
                    if (_styleBackground.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(BackgroundChangedArgs);
                    break;
                case "BorderColor":
                    if (_styleBorderColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(BorderColorChangedArgs);
                    break;
                case "BorderThickness":
                    if (_styleBorderThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(BorderThicknessChangedArgs);
                    break;
                case "FontName":
                    if (_styleFontName.Apply(Converters.Convert<string?>("NullableFontName", entry.Value)))
                        NotifyPropertyChanged(FontNameChangedArgs);
                    break;
                case "FontSize":
                    if (_styleFontSize.Apply(Converters.Convert<int?>("NullableFontSize", entry.Value)))
                        NotifyPropertyChanged(FontSizeChangedArgs);
                    break;
                case "FontWeight":
                    if (_styleFontWeight.Apply(Converters.Convert<FontWeight?>("NullableFontWeight", entry.Value)))
                        NotifyPropertyChanged(FontWeightChangedArgs);
                    break;
                case "FontStyle":
                    if (_styleFontStyle.Apply(Converters.Convert<FontStyle?>("NullableFontStyle", entry.Value)))
                        NotifyPropertyChanged(FontStyleChangedArgs);
                    break;
                case "Font":
                    if (_styleFont.Apply(Converters.Convert<IControlFont?>("Font", entry.Value)))
                        NotifyPropertyChanged(FontChangedArgs);
                    break;
                case "TextColor":
                    if (_styleTextColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(TextColorChangedArgs);
                    break;
                case "HoverBackground":
                    if (_styleHoverBackground.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(HoverBackgroundChangedArgs);
                    break;
                case "PressedBackground":
                    if (_stylePressedBackground.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(PressedBackgroundChangedArgs);
                    break;
                case "DisabledBackground":
                    if (_styleDisabledBackground.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(DisabledBackgroundChangedArgs);
                    break;
                case "FocusColor":
                    if (_styleFocusColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(FocusColorChangedArgs);
                    break;
                case "MinWidth":
                    if (_styleMinWidth.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinWidthChangedArgs);
                    break;
                case "MinHeight":
                    if (_styleMinHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MinHeightChangedArgs);
                    break;
                case "DisabledTextColor":
                    if (_styleDisabledTextColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(DisabledTextColorChangedArgs);
                    break;
                case "ToolTip":
                    if (_styleToolTip.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(ToolTipChangedArgs);
                    break;
                case "ToolTipContent":
                {
                    StyleProperty<Control?> candidate = _styleToolTipContent;
                    bool changed = candidate.Apply(Converters.Convert<Control?>("Control", entry.Value));
                    CommitToolTipContent(candidate, changed);
                    break;
                }

                case "ToolTipDelay":
                    if (_styleToolTipDelay.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ToolTipDelayChangedArgs);
                    break;
                case "ToolTipDuration":
                    if (_styleToolTipDuration.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ToolTipDurationChangedArgs);
                    break;
                case "ToolTipMaxWidth":
                    if (_styleToolTipMaxWidth.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ToolTipMaxWidthChangedArgs);
                    break;
            }
        }
    }

    internal virtual bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsVisible":
                return _styleIsVisible.HasLocalValue;
            case "IsEnabled":
                return _styleIsEnabled.HasLocalValue;
            case "IsHitTestVisible":
                return _styleIsHitTestVisible.HasLocalValue;
            case "IsFocusable":
                return _styleIsFocusable.HasLocalValue;
            case "ZIndex":
                return _styleZIndex.HasLocalValue;
            case "TabIndex":
                return _styleTabIndex.HasLocalValue;
            case "Width":
                return _styleWidth.HasLocalValue;
            case "Height":
                return _styleHeight.HasLocalValue;
            case "Margin":
                return _styleMargin.HasLocalValue;
            case "Padding":
                return _stylePadding.HasLocalValue;
            case "HorizontalAlignment":
                return _styleHorizontalAlignment.HasLocalValue;
            case "VerticalAlignment":
                return _styleVerticalAlignment.HasLocalValue;
            case "Opacity":
                return _styleOpacity.HasLocalValue;
            case "Background":
                return _styleBackground.HasLocalValue;
            case "BorderColor":
                return _styleBorderColor.HasLocalValue;
            case "BorderThickness":
                return _styleBorderThickness.HasLocalValue;
            case "FontName":
                return _styleFontName.HasLocalValue;
            case "FontSize":
                return _styleFontSize.HasLocalValue;
            case "FontWeight":
                return _styleFontWeight.HasLocalValue;
            case "FontStyle":
                return _styleFontStyle.HasLocalValue;
            case "Font":
                return _styleFont.HasLocalValue;
            case "TextColor":
                return _styleTextColor.HasLocalValue;
            case "HoverBackground":
                return _styleHoverBackground.HasLocalValue;
            case "PressedBackground":
                return _stylePressedBackground.HasLocalValue;
            case "DisabledBackground":
                return _styleDisabledBackground.HasLocalValue;
            case "FocusColor":
                return _styleFocusColor.HasLocalValue;
            case "MinWidth":
                return _styleMinWidth.HasLocalValue;
            case "MinHeight":
                return _styleMinHeight.HasLocalValue;
            case "DisabledTextColor":
                return _styleDisabledTextColor.HasLocalValue;
            case "ToolTip":
                return _styleToolTip.HasLocalValue;
            case "ToolTipContent":
                return _styleToolTipContent.HasLocalValue;
            case "ToolTipDelay":
                return _styleToolTipDelay.HasLocalValue;
            case "ToolTipDuration":
                return _styleToolTipDuration.HasLocalValue;
            case "ToolTipMaxWidth":
                return _styleToolTipMaxWidth.HasLocalValue;
            default:
                return false;
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public virtual void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsVisible":
                if (_styleIsVisible.ClearLocal())
                    NotifyPropertyChanged(IsVisibleChangedArgs);
                return;
            case "IsEnabled":
                if (_styleIsEnabled.ClearLocal())
                    NotifyPropertyChanged(IsEnabledChangedArgs);
                return;
            case "IsHitTestVisible":
                if (_styleIsHitTestVisible.ClearLocal())
                    NotifyPropertyChanged(IsHitTestVisibleChangedArgs);
                return;
            case "IsFocusable":
                if (_styleIsFocusable.ClearLocal())
                    NotifyPropertyChanged(IsFocusableChangedArgs);
                return;
            case "ZIndex":
                if (_styleZIndex.ClearLocal())
                    NotifyPropertyChanged(ZIndexChangedArgs);
                return;
            case "TabIndex":
                if (_styleTabIndex.ClearLocal())
                    NotifyPropertyChanged(TabIndexChangedArgs);
                return;
            case "Width":
                if (_styleWidth.ClearLocal())
                    NotifyPropertyChanged(WidthChangedArgs);
                return;
            case "Height":
                if (_styleHeight.ClearLocal())
                    NotifyPropertyChanged(HeightChangedArgs);
                return;
            case "Margin":
                if (_styleMargin.ClearLocal())
                    NotifyPropertyChanged(MarginChangedArgs);
                return;
            case "Padding":
                if (_stylePadding.ClearLocal())
                    NotifyPropertyChanged(PaddingChangedArgs);
                return;
            case "HorizontalAlignment":
                if (_styleHorizontalAlignment.ClearLocal())
                    NotifyPropertyChanged(HorizontalAlignmentChangedArgs);
                return;
            case "VerticalAlignment":
                if (_styleVerticalAlignment.ClearLocal())
                    NotifyPropertyChanged(VerticalAlignmentChangedArgs);
                return;
            case "Opacity":
                if (_styleOpacity.ClearLocal())
                    NotifyPropertyChanged(OpacityChangedArgs);
                return;
            case "Background":
                if (_styleBackground.ClearLocal())
                    NotifyPropertyChanged(BackgroundChangedArgs);
                return;
            case "BorderColor":
                if (_styleBorderColor.ClearLocal())
                    NotifyPropertyChanged(BorderColorChangedArgs);
                return;
            case "BorderThickness":
                if (_styleBorderThickness.ClearLocal())
                    NotifyPropertyChanged(BorderThicknessChangedArgs);
                return;
            case "FontName":
                if (_styleFontName.ClearLocal())
                    NotifyPropertyChanged(FontNameChangedArgs);
                return;
            case "FontSize":
                if (_styleFontSize.ClearLocal())
                    NotifyPropertyChanged(FontSizeChangedArgs);
                return;
            case "FontWeight":
                if (_styleFontWeight.ClearLocal())
                    NotifyPropertyChanged(FontWeightChangedArgs);
                return;
            case "FontStyle":
                if (_styleFontStyle.ClearLocal())
                    NotifyPropertyChanged(FontStyleChangedArgs);
                return;
            case "Font":
                if (_styleFont.ClearLocal())
                    NotifyPropertyChanged(FontChangedArgs);
                return;
            case "TextColor":
                if (_styleTextColor.ClearLocal())
                    NotifyPropertyChanged(TextColorChangedArgs);
                return;
            case "HoverBackground":
                if (_styleHoverBackground.ClearLocal())
                    NotifyPropertyChanged(HoverBackgroundChangedArgs);
                return;
            case "PressedBackground":
                if (_stylePressedBackground.ClearLocal())
                    NotifyPropertyChanged(PressedBackgroundChangedArgs);
                return;
            case "DisabledBackground":
                if (_styleDisabledBackground.ClearLocal())
                    NotifyPropertyChanged(DisabledBackgroundChangedArgs);
                return;
            case "FocusColor":
                if (_styleFocusColor.ClearLocal())
                    NotifyPropertyChanged(FocusColorChangedArgs);
                return;
            case "MinWidth":
                if (_styleMinWidth.ClearLocal())
                    NotifyPropertyChanged(MinWidthChangedArgs);
                return;
            case "MinHeight":
                if (_styleMinHeight.ClearLocal())
                    NotifyPropertyChanged(MinHeightChangedArgs);
                return;
            case "DisabledTextColor":
                if (_styleDisabledTextColor.ClearLocal())
                    NotifyPropertyChanged(DisabledTextColorChangedArgs);
                return;
            case "ToolTip":
                if (_styleToolTip.ClearLocal())
                    NotifyPropertyChanged(ToolTipChangedArgs);
                return;
            case "ToolTipContent":
            {
                StyleProperty<Control?> candidate = _styleToolTipContent;
                bool changed = candidate.ClearLocal();
                CommitToolTipContent(candidate, changed);
                return;
            }

            case "ToolTipDelay":
                if (_styleToolTipDelay.ClearLocal())
                    NotifyPropertyChanged(ToolTipDelayChangedArgs);
                return;
            case "ToolTipDuration":
                if (_styleToolTipDuration.ClearLocal())
                    NotifyPropertyChanged(ToolTipDurationChangedArgs);
                return;
            case "ToolTipMaxWidth":
                if (_styleToolTipMaxWidth.ClearLocal())
                    NotifyPropertyChanged(ToolTipMaxWidthChangedArgs);
                return;
            default:
                throw new ArgumentException("Unknown style property.", nameof(property));
        }
    }

    /// <summary>Performs reset styled values using the current control state.</summary>
    protected virtual void ResetStyledValues()
    {
        if (_styleIsVisible.BeginStyle(true))
            NotifyPropertyChanged(IsVisibleChangedArgs);
        if (_styleIsEnabled.BeginStyle(true))
            NotifyPropertyChanged(IsEnabledChangedArgs);
        if (_styleIsHitTestVisible.BeginStyle(true))
            NotifyPropertyChanged(IsHitTestVisibleChangedArgs);
        if (_styleIsFocusable.BeginStyle(false))
            NotifyPropertyChanged(IsFocusableChangedArgs);
        if (_styleZIndex.BeginStyle(0))
            NotifyPropertyChanged(ZIndexChangedArgs);
        if (_styleTabIndex.BeginStyle(0))
            NotifyPropertyChanged(TabIndexChangedArgs);
        if (_styleWidth.BeginStyle(null))
            NotifyPropertyChanged(WidthChangedArgs);
        if (_styleHeight.BeginStyle(null))
            NotifyPropertyChanged(HeightChangedArgs);
        if (_styleMargin.BeginStyle(Insets2.Zero))
            NotifyPropertyChanged(MarginChangedArgs);
        if (_stylePadding.BeginStyle(Insets2.Zero))
            NotifyPropertyChanged(PaddingChangedArgs);
        if (_styleHorizontalAlignment.BeginStyle(ControlAlignment.Stretch))
            NotifyPropertyChanged(HorizontalAlignmentChangedArgs);
        if (_styleVerticalAlignment.BeginStyle(ControlAlignment.Stretch))
            NotifyPropertyChanged(VerticalAlignmentChangedArgs);
        if (_styleOpacity.BeginStyle(1f))
            NotifyPropertyChanged(OpacityChangedArgs);
        if (_styleBackground.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(BackgroundChangedArgs);
        if (_styleBorderColor.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(BorderColorChangedArgs);
        if (_styleBorderThickness.BeginStyle(0f))
            NotifyPropertyChanged(BorderThicknessChangedArgs);
        if (_styleFontName.BeginStyle(null))
            NotifyPropertyChanged(FontNameChangedArgs);
        if (_styleFontSize.BeginStyle(null))
            NotifyPropertyChanged(FontSizeChangedArgs);
        if (_styleFontWeight.BeginStyle(null))
            NotifyPropertyChanged(FontWeightChangedArgs);
        if (_styleFontStyle.BeginStyle(null))
            NotifyPropertyChanged(FontStyleChangedArgs);
        if (_styleFont.BeginStyle(null))
            NotifyPropertyChanged(FontChangedArgs);
        if (_styleTextColor.BeginStyle(Color.White))
            NotifyPropertyChanged(TextColorChangedArgs);
        if (_styleHoverBackground.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(HoverBackgroundChangedArgs);
        if (_stylePressedBackground.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(PressedBackgroundChangedArgs);
        if (_styleDisabledBackground.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(DisabledBackgroundChangedArgs);
        if (_styleFocusColor.BeginStyle(Color.Transparent))
            NotifyPropertyChanged(FocusColorChangedArgs);
        if (_styleMinWidth.BeginStyle(0f))
            NotifyPropertyChanged(MinWidthChangedArgs);
        if (_styleMinHeight.BeginStyle(0f))
            NotifyPropertyChanged(MinHeightChangedArgs);
        if (_styleDisabledTextColor.BeginStyle(new Color(97, 113, 133)))
            NotifyPropertyChanged(DisabledTextColorChangedArgs);
        if (_styleToolTip.BeginStyle(string.Empty))
            NotifyPropertyChanged(ToolTipChangedArgs);
        if (_styleToolTipContent.BeginStyle(null))
            NotifyPropertyChanged(ToolTipContentChangedArgs);
        if (_styleToolTipDelay.BeginStyle(0.5f))
            NotifyPropertyChanged(ToolTipDelayChangedArgs);
        if (_styleToolTipDuration.BeginStyle(6f))
            NotifyPropertyChanged(ToolTipDurationChangedArgs);
        if (_styleToolTipMaxWidth.BeginStyle(320f))
            NotifyPropertyChanged(ToolTipMaxWidthChangedArgs);
    }

    internal virtual void BeginFrame()
    {
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected virtual void CommitStyledValues()
    {
        if (_styleIsVisible.CommitStyle())
            NotifyPropertyChanged(IsVisibleChangedArgs);
        if (_styleIsEnabled.CommitStyle())
            NotifyPropertyChanged(IsEnabledChangedArgs);
        if (_styleIsHitTestVisible.CommitStyle())
            NotifyPropertyChanged(IsHitTestVisibleChangedArgs);
        if (_styleIsFocusable.CommitStyle())
            NotifyPropertyChanged(IsFocusableChangedArgs);
        if (_styleZIndex.CommitStyle())
            NotifyPropertyChanged(ZIndexChangedArgs);
        if (_styleTabIndex.CommitStyle())
            NotifyPropertyChanged(TabIndexChangedArgs);
        if (_styleWidth.CommitStyle())
            NotifyPropertyChanged(WidthChangedArgs);
        if (_styleHeight.CommitStyle())
            NotifyPropertyChanged(HeightChangedArgs);
        if (_styleMargin.CommitStyle())
            NotifyPropertyChanged(MarginChangedArgs);
        if (_stylePadding.CommitStyle())
            NotifyPropertyChanged(PaddingChangedArgs);
        if (_styleHorizontalAlignment.CommitStyle())
            NotifyPropertyChanged(HorizontalAlignmentChangedArgs);
        if (_styleVerticalAlignment.CommitStyle())
            NotifyPropertyChanged(VerticalAlignmentChangedArgs);
        if (_styleOpacity.CommitStyle())
            NotifyPropertyChanged(OpacityChangedArgs);
        if (_styleBackground.CommitStyle())
            NotifyPropertyChanged(BackgroundChangedArgs);
        if (_styleBorderColor.CommitStyle())
            NotifyPropertyChanged(BorderColorChangedArgs);
        if (_styleBorderThickness.CommitStyle())
            NotifyPropertyChanged(BorderThicknessChangedArgs);
        if (_styleFontName.CommitStyle())
            NotifyPropertyChanged(FontNameChangedArgs);
        if (_styleFontSize.CommitStyle())
            NotifyPropertyChanged(FontSizeChangedArgs);
        if (_styleFontWeight.CommitStyle())
            NotifyPropertyChanged(FontWeightChangedArgs);
        if (_styleFontStyle.CommitStyle())
            NotifyPropertyChanged(FontStyleChangedArgs);
        if (_styleFont.CommitStyle())
            NotifyPropertyChanged(FontChangedArgs);
        if (_styleTextColor.CommitStyle())
            NotifyPropertyChanged(TextColorChangedArgs);
        if (_styleHoverBackground.CommitStyle())
            NotifyPropertyChanged(HoverBackgroundChangedArgs);
        if (_stylePressedBackground.CommitStyle())
            NotifyPropertyChanged(PressedBackgroundChangedArgs);
        if (_styleDisabledBackground.CommitStyle())
            NotifyPropertyChanged(DisabledBackgroundChangedArgs);
        if (_styleFocusColor.CommitStyle())
            NotifyPropertyChanged(FocusColorChangedArgs);
        if (_styleMinWidth.CommitStyle())
            NotifyPropertyChanged(MinWidthChangedArgs);
        if (_styleMinHeight.CommitStyle())
            NotifyPropertyChanged(MinHeightChangedArgs);
        if (_styleDisabledTextColor.CommitStyle())
            NotifyPropertyChanged(DisabledTextColorChangedArgs);
        if (_styleToolTip.CommitStyle())
            NotifyPropertyChanged(ToolTipChangedArgs);
        StyleProperty<Control?> tooltipCandidate = _styleToolTipContent;
        bool tooltipChanged = tooltipCandidate.CommitStyle();
        CommitToolTipContent(tooltipCandidate, tooltipChanged);
        if (_styleToolTipDelay.CommitStyle())
            NotifyPropertyChanged(ToolTipDelayChangedArgs);
        if (_styleToolTipDuration.CommitStyle())
            NotifyPropertyChanged(ToolTipDurationChangedArgs);
        if (_styleToolTipMaxWidth.CommitStyle())
            NotifyPropertyChanged(ToolTipMaxWidthChangedArgs);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// A clipped content viewport with independent scrollbars and threshold-based touch scrolling.
/// </summary>
public sealed class ScrollViewer : ContainerControl
{
    /// <summary>Caches the notification for IsMouseDragEnabled changes.</summary>
    private static readonly PropertyChangedEventArgs IsMouseDragEnabledArgs = new(nameof(IsMouseDragEnabled));

    /// <summary>Caches the notification for IsSwipeEnabled changes.</summary>
    private static readonly PropertyChangedEventArgs IsSwipeEnabledArgs = new(nameof(IsSwipeEnabled));

    /// <summary>Caches the active swipe state notification.</summary>
    private static readonly PropertyChangedEventArgs IsSwipingArgs = new(nameof(IsSwiping));

    /// <summary>Caches the notification for SwipeThreshold changes.</summary>
    private static readonly PropertyChangedEventArgs SwipeThresholdArgs = new(nameof(SwipeThreshold));

    /// <summary>Caches the property change notification for Content.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentChangedArgs = new(nameof(Content));

    /// <summary>Caches the property change notification for HorizontalScrollBarVisibility.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs HorizontalScrollBarVisibilityChangedArgs = new(nameof(HorizontalScrollBarVisibility));

    /// <summary>Caches the property change notification for ScrollBarThickness.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));

    /// <summary>Caches the property change notification for VerticalScrollBarVisibility.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs VerticalScrollBarVisibilityChangedArgs = new(nameof(VerticalScrollBarVisibility));

    /// <summary>Caches the content clipping and gesture area in workspace coordinates.</summary>
    private Bounds2 _viewport;

    /// <summary>Identifies the content instance used for the cached extent and scrollbar decisions.</summary>
    private Control? _cachedContent;

    /// <summary>Caches the committed content used for viewport layout and clipping.</summary>
    private Control? _content;

    /// <summary>Stores logically owned content, including pending traversal changes.</summary>
    private Control? _storedContent;

    /// <summary>Retains the latest context for bounded content measurement during arrangement.</summary>
    private ControlLayoutContext? _layout;

    /// <summary>Identifies the one pointer tracked by this scroll gesture.</summary>
    private ControlPointer _swipePointer;

    /// <summary>Retains the default font identity used by the last complete content measurement.</summary>
    private IControlFont? _lastDefaultFont;

    /// <summary>Retains the desired size produced by the last successful measurement.</summary>
    private Size2 _cachedDesiredSize;

    /// <summary>Caches the measured unscrolled content extent.</summary>
    private Size2 _extent;

    /// <summary>Stores the content-area size used by the last successful arrangement.</summary>
    private Size2 _lastArrangedSize;

    /// <summary>Stores the available size used for the cached content measurement.</summary>
    private Size2 _lastAvailable;

    /// <summary>Stores the styled horizontal scrollbar visibility policy.</summary>
    private StyleProperty<ScrollBarVisibility> _styleHorizontalScrollBarVisibility = new(ScrollBarVisibility.Auto);

    /// <summary>Stores the styled vertical scrollbar visibility policy.</summary>
    private StyleProperty<ScrollBarVisibility> _styleVerticalScrollBarVisibility = new(ScrollBarVisibility.Auto);

    /// <summary>Stores the styled and locally overridden IsMouseDragEnabled value.</summary>
    private StyleProperty<bool> _styleIsMouseDragEnabled = new(false);

    /// <summary>Stores the styled and locally overridden IsSwipeEnabled value.</summary>
    private StyleProperty<bool> _styleIsSwipeEnabled = new(true);

    /// <summary>Stores the styled and locally overridden scrollbar thickness.</summary>
    private StyleProperty<float> _styleScrollBarThickness = new(20f);

    /// <summary>Stores the styled and locally overridden SwipeThreshold value.</summary>
    private StyleProperty<float> _styleSwipeThreshold = new(12);

    /// <summary>Stores the previous pointer position for incremental scrolling and boundary handoff.</summary>
    private Vector2 _swipePrevious;

    /// <summary>Stores the initial workspace position used to evaluate the movement threshold.</summary>
    private Vector2 _swipeStart;

    /// <summary>Stores the horizontal scrollbar decision for the arranged viewport.</summary>
    private bool _horizontal;

    /// <summary>Records whether this viewer has taken capture for an active scroll gesture.</summary>
    private bool _isSwiping;

    /// <summary>Records whether successful measurement and arrangement established reusable content geometry.</summary>
    private bool _layoutCacheReady;

    /// <summary>Records whether a pointer is a pending scroll gesture candidate.</summary>
    private bool _trackingSwipe;

    /// <summary>Stores the vertical scrollbar decision for the arranged viewport.</summary>
    private bool _vertical;

    /// <summary>Records whether a descendant has consumed movement before this viewer takes over.</summary>
    private bool _yieldedSwipe;

    /// <summary>
    /// Initializes a new scroll viewer instance.
    /// </summary>
    public ScrollViewer() : base()
    {
        HorizontalScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Horizontal
        };
        VerticalScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Vertical
        };
        base.Add(HorizontalScrollBar);
        base.Add(VerticalScrollBar);
    }

    /// <summary>Gets whether retained geometry still belongs to unchanged, successfully measured content.</summary>
    private bool CanReuseGeometry => _layoutCacheReady && ReferenceEquals(_cachedContent, _content) && (_content?.HasValidMeasure ?? true);

    /// <summary>Gets whether this viewer currently owns a pointer-driven scroll gesture.</summary>
    public bool IsSwiping => _isSwiping;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ScrollViewer";

    /// <summary>
    /// Gets the independent horizontal scrollbar controlling the content offset.
    /// </summary>
    public ScrollBar HorizontalScrollBar
    {
        get;
    }

    /// <summary>
    /// Gets the independent vertical scrollbar controlling the content offset.
    /// </summary>
    public ScrollBar VerticalScrollBar
    {
        get;
    }

    /// <summary>
    /// Gets the arranged content viewport in workspace logical coordinates.
    /// </summary>
    public Bounds2 ViewportBounds => _viewport;

    /// <summary>
    /// Gets the measured content extent before clipping or scrolling.
    /// </summary>
    public Size2 Extent => _extent;

    /// <summary>
    /// Gets or sets horizontal scroll bar visibility.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility { get => ConfigHorizontalScrollBarVisibility; set => ConfigHorizontalScrollBarVisibility = value; }

    /// <summary>
    /// Gets or sets vertical scroll bar visibility.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility { get => ConfigVerticalScrollBarVisibility; set => ConfigVerticalScrollBarVisibility = value; }

    /// <summary>
    /// Gets or sets the single owned content control, or null for an empty viewport.
    /// </summary>
    public Control? Content { get => ConfigContent; set => ConfigContent = value; }

    /// <summary>Gets whether at least one arranged scroll axis has a nonempty range.</summary>
    private bool HasScrollableAxis => _horizontal && HorizontalScrollBar.Maximum > HorizontalScrollBar.Minimum || _vertical && VerticalScrollBar.Maximum > VerticalScrollBar.Minimum;

    /// <summary>Gets or locally overrides the effective scrollbar thickness.</summary>
    internal float ConfigScrollBarThickness
    {
        get => _styleScrollBarThickness.Value;
        set
        {
            if (_styleScrollBarThickness.SetLocal(value))
                NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets ScrollBarThickness; a local assignment overrides the style.
    /// </summary>
    public float ScrollBarThickness { get => ConfigScrollBarThickness; set => ConfigScrollBarThickness = value; }

    /// <summary>Gets or locally overrides the horizontal scrollbar visibility policy.</summary>
    internal ScrollBarVisibility ConfigHorizontalScrollBarVisibility
    {
        get => _styleHorizontalScrollBarVisibility.Value;
        set
        {
            if (_styleHorizontalScrollBarVisibility.SetLocal(value))
                NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        }
    }

    /// <summary>Gets or locally overrides the vertical scrollbar visibility policy.</summary>
    internal ScrollBarVisibility ConfigVerticalScrollBarVisibility
    {
        get => _styleVerticalScrollBarVisibility.Value;
        set
        {
            if (_styleVerticalScrollBarVisibility.SetLocal(value))
                NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
        }
    }

    /// <summary>Gets or replaces logically owned content through the shared ownership mechanism.</summary>
    internal Control? ConfigContent { get => _storedContent; set => SetOwnedControl(ref _storedContent, value, ContentChangedArgs); }

    /// <summary>Gets or sets whether touch swiping and optional mouse dragging can scroll the viewport. Defaults to true.</summary>
    public bool IsSwipeEnabled
    {
        get => _styleIsSwipeEnabled.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleIsSwipeEnabled.SetLocal(value))
                NotifyPropertyChanged(IsSwipeEnabledArgs);
        }
    }

    /// <summary>Gets or sets whether the primary mouse button can drag-scroll content when IsSwipeEnabled is true. Defaults to false.</summary>
    public bool IsMouseDragEnabled
    {
        get => _styleIsMouseDragEnabled.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleIsMouseDragEnabled.SetLocal(value))
                NotifyPropertyChanged(IsMouseDragEnabledArgs);
        }
    }

    /// <summary>Gets or sets the positive movement threshold in logical UI units before scrolling captures the pointer. Defaults to 12.</summary>
    public float SwipeThreshold
    {
        get => _styleSwipeThreshold.Value;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleSwipeThreshold.SetLocal(value))
                NotifyPropertyChanged(SwipeThresholdArgs);
        }
    }

    /// <summary>Clamps the cached content extent and visible scrollbar reservations to the available size.</summary>
    /// <param name = "available">The current content measurement constraints.</param>
    /// <param name = "thickness">The logical scrollbar thickness.</param>
    /// <returns>The desired size of the scroll viewer's content area.</returns>
    private Size2 MeasuredViewport(Size2 available, float thickness) => new(MathF.Min(available.Width, _extent.Width + (_vertical ? thickness : 0)), MathF.Min(available.Height, _extent.Height + (_horizontal ? thickness : 0)));

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        if (ReferenceEquals(_content, ConfigContent))
            return;
        if (ConfigContent is Control child)
            PresentOwnedChild(child, true);
        _content = ConfigContent;
    }

    /// <inheritdoc/>
    internal override Bounds2 GetChildClip(Control child) => child == _content ? _viewport : base.ContentBounds;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _layout = context;
        float thickness = ConfigScrollBarThickness;
        if (!float.IsFinite(thickness) || thickness <= 0)
            throw new InvalidOperationException("Invalid scrollbar thickness.");
        if (CanReuseGeometry && _lastAvailable.Width == available.Width && _lastAvailable.Height == available.Height && ReferenceEquals(_lastDefaultFont, context.DefaultFont))
        {
            HorizontalScrollBar.Measure(context, available);
            VerticalScrollBar.Measure(context, available);
            return _cachedDesiredSize;
        }

        _layoutCacheReady = false;
        _content?.Measure(context, new Size2(HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden ? available.Width : float.PositiveInfinity, VerticalScrollBarVisibility == ScrollBarVisibility.Hidden ? available.Height : float.PositiveInfinity));
        _extent = _content?.DesiredSize ?? Size2.Zero;
        _horizontal = HorizontalScrollBarVisibility == ScrollBarVisibility.Always;
        _vertical = VerticalScrollBarVisibility == ScrollBarVisibility.Always;
        for (int i = 0; i < 3; i++)
        {
            if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
                _horizontal = _extent.Width > MathF.Max(0, available.Width - (_vertical ? thickness : 0));
            if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                _vertical = _extent.Height > MathF.Max(0, available.Height - (_horizontal ? thickness : 0));
        }

        // Measurement can be unbounded; publish visibility only for the arranged viewport.
        HorizontalScrollBar.Measure(context, available);
        VerticalScrollBar.Measure(context, available);
        _lastAvailable = available;
        _lastDefaultFont = context.DefaultFont;
        _cachedContent = _content;
        _cachedDesiredSize = MeasuredViewport(available, thickness);
        return _cachedDesiredSize;
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        float t = ConfigScrollBarThickness;
        bool reuse = CanReuseGeometry && _lastArrangedSize.Width == contentBounds.Width && _lastArrangedSize.Height == contentBounds.Height;
        _layoutCacheReady = false;
        if (!reuse)
        {
            _horizontal = HorizontalScrollBarVisibility == ScrollBarVisibility.Always;
            _vertical = VerticalScrollBarVisibility == ScrollBarVisibility.Always;
            for (int i = 0; i < 3; i++)
            {
                if (_layout is not null && _content is not null)
                {
                    _content.Measure(_layout, new Size2(HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden ? MathF.Max(0, contentBounds.Width - (_vertical ? t : 0)) : float.PositiveInfinity, VerticalScrollBarVisibility == ScrollBarVisibility.Hidden ? MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0)) : float.PositiveInfinity));
                    _extent = _content.DesiredSize;
                }

                if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto)
                    _horizontal = _extent.Width > MathF.Max(0, contentBounds.Width - (_vertical ? t : 0));
                if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto)
                    _vertical = _extent.Height > MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0));
            }
        }

        HorizontalScrollBar.IsVisible = _horizontal;
        VerticalScrollBar.IsVisible = _vertical;
        if (_layout is not null)
        {
            HorizontalScrollBar.Measure(_layout, new Size2(contentBounds.Width, contentBounds.Height));
            VerticalScrollBar.Measure(_layout, new Size2(contentBounds.Width, contentBounds.Height));
        }

        _viewport = new Bounds2(contentBounds.X, contentBounds.Y, MathF.Max(0, contentBounds.Width - (_vertical ? t : 0)), MathF.Max(0, contentBounds.Height - (_horizontal ? t : 0)));
        HorizontalScrollBar.Maximum = MathF.Max(0, _extent.Width - _viewport.Width);
        HorizontalScrollBar.ViewportSize = _viewport.Width;
        HorizontalScrollBar.LargeChange = _viewport.Width;
        HorizontalScrollBar.SmallChange = 24;
        VerticalScrollBar.Maximum = MathF.Max(0, _extent.Height - _viewport.Height);
        VerticalScrollBar.ViewportSize = _viewport.Height;
        VerticalScrollBar.LargeChange = _viewport.Height;
        VerticalScrollBar.SmallChange = 24;
        HorizontalScrollBar.Arrange(new Bounds2(contentBounds.X, _viewport.Bottom, _viewport.Width, t), true);
        VerticalScrollBar.Arrange(new Bounds2(_viewport.Right, contentBounds.Y, t, _viewport.Height), true);
        _content?.Arrange(new Bounds2(_viewport.X - (_horizontal ? HorizontalScrollBar.Value : 0), _viewport.Y - (_vertical ? VerticalScrollBar.Value : 0), MathF.Max(_viewport.Width, _extent.Width), MathF.Max(_viewport.Height, _extent.Height)));
        _lastArrangedSize = new Size2(contentBounds.Width, contentBounds.Height);
        _layoutCacheReady = true;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type != ControlInputEventType.PointerWheel)
            return;
        ScrollBar bar = e.Shift && _horizontal ? HorizontalScrollBar : VerticalScrollBar;
        if (!bar.IsVisible)
            return;
        float old = bar.Value;
        bar.Value -= e.WheelDelta / 120f * bar.SmallChange;
        if (old != bar.Value)
            args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPreviewInput(ControlInputContext context, ControlInputEventArgs args)
    {
        ControlInputEvent input = args.Input;
        if (input.Type == ControlInputEventType.PointerCancel && _trackingSwipe && input.Pointer == _swipePointer)
        {
            CancelSwipe();
            return;
        }

        if (!IsSwipeEnabled || (input.PointerKind == ControlPointerKind.Mouse && !IsMouseDragEnabled))
            return;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            if (_trackingSwipe && !HasSwipeCapture(this, _swipePointer))
                CancelSwipe();
            if (_trackingSwipe || !Contains(_viewport, input.Position.X, input.Position.Y) || !HasScrollableAxis || IsDirectManipulationTarget(args.Source, this))
                return;
            _trackingSwipe = true;
            _yieldedSwipe = false;
            _swipePointer = input.Pointer;
            _swipeStart = _swipePrevious = input.Position;
            // Retain the child route for taps and cancellation without taking the gesture yet.
            context.CapturePointer(args.Source);
            return;
        }

        if (!_trackingSwipe || input.Pointer != _swipePointer)
            return;
        if (input.Type == ControlInputEventType.PointerMove)
        {
            Vector2 total = input.Position - _swipeStart;
            Vector2 incremental = input.Position - _swipePrevious;
            _swipePrevious = input.Position;
            if (!_isSwiping)
            {
                if (MathF.Max(MathF.Abs(total.X), MathF.Abs(total.Y)) < SwipeThreshold)
                    return;
                Vector2 movement = _yieldedSwipe ? incremental : total;
                for (Control? node = args.Source; node is not null && node != this; node = node.Parent)
                {
                    if (node is ScrollViewer child && child.CanConsumeSwipe(input.Pointer, movement) ||
                        node is ListBox list && list.CanConsumeSwipe(input.Pointer, movement))
                    {
                        _yieldedSwipe = true;
                        return;
                    }
                }

                if (!CanScrollInDirection(movement))
                    return;
                context.CapturePointer(this);
                SetNotify(ref _isSwiping, true, IsSwipingArgs);
                incremental = movement;
            }

            Pan(incremental);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            if (_isSwiping)
            {
                Pan(input.Position - _swipePrevious);
                args.Handled = true;
            }

            CancelSwipe();
        }
    }

    /// <summary>Reports whether this pending or active viewer can consume movement in its dominant direction.</summary>
    /// <param name = "pointer">The pointer proposing to move the content.</param>
    /// <param name = "movement">The proposed finger displacement in logical UI coordinates.</param>
    /// <returns>True when this viewer is tracking the same pointer and can scroll in that direction.</returns>
    internal bool CanConsumeSwipe(ControlPointer pointer, Vector2 movement) => _trackingSwipe && pointer == _swipePointer && IsSwipeEnabled && CanScrollInDirection(movement);

    /// <summary>Tests whether a pointer remains captured inside a gesture container.</summary>
    /// <param name = "container">The ancestor whose pending gesture is being checked.</param>
    /// <param name = "pointer">The tracked pointer identity.</param>
    /// <returns>True when capture belongs to the container or one of its descendants.</returns>
    internal static bool HasSwipeCapture(Control container, ControlPointer pointer)
    {
        for (Control? node = container.Workspace?.GetCapturedControl(pointer); node is not null; node = node.Parent)
            if (ReferenceEquals(node, container))
                return true;
        return false;
    }

    /// <summary>Preserves controls that own direct dragging or text selection instead of parent swiping.</summary>
    /// <param name = "source">The original hit-test target.</param>
    /// <param name = "boundary">The parent gesture container, excluded from the search.</param>
    /// <returns>True for a range editor, text editor, joystick, curve, rotation or scene interaction target.</returns>
    internal static bool IsDirectManipulationTarget(Control source, Control boundary)
    {
        for (Control? node = source; node is not null && node != boundary; node = node.Parent)
            if (node is RangeControl or TextBox or ValueTextBox or VirtualJoystick or CurveControl or RotationControl or SceneViewport)
                return true;
        return false;
    }

    /// <summary>Tests the scroll range matching the dominant finger movement.</summary>
    /// <param name = "movement">The signed finger displacement.</param>
    /// <returns>True if content can move opposite to the finger along the intended axis.</returns>
    private bool CanScrollInDirection(Vector2 movement)
    {
        float x = MathF.Abs(movement.X), y = MathF.Abs(movement.Y);
        return x >= y && _horizontal && CanMove(HorizontalScrollBar, movement.X) || y >= x && _vertical && CanMove(VerticalScrollBar, movement.Y);
    }

    /// <summary>Tests whether a range can consume the proposed finger displacement.</summary>
    /// <param name = "bar">The axis range controlling the content offset.</param>
    /// <param name = "movement">The signed finger displacement along that axis.</param>
    /// <returns>True when scrolling opposite to the finger remains within the available range.</returns>
    private static bool CanMove(ScrollBar bar, float movement) => movement < 0 ? bar.Value < bar.Maximum : movement > 0 && bar.Value > bar.Minimum;

    /// <summary>Moves content opposite to the finger through the existing clamped scrollbar values.</summary>
    /// <param name = "movement">The incremental logical pointer displacement.</param>
    private void Pan(Vector2 movement)
    {
        if (_horizontal)
            HorizontalScrollBar.Value = (float)Math.Clamp((double)HorizontalScrollBar.Value - movement.X, HorizontalScrollBar.Minimum, HorizontalScrollBar.Maximum);
        if (_vertical)
            VerticalScrollBar.Value = (float)Math.Clamp((double)VerticalScrollBar.Value - movement.Y, VerticalScrollBar.Minimum, VerticalScrollBar.Maximum);
    }

    /// <summary>Clears pending swipe state and releases capture if it belongs to this viewer.</summary>
    private void CancelSwipe()
    {
        _trackingSwipe = false;
        _yieldedSwipe = false;
        SetNotify(ref _isSwiping, false, IsSwipingArgs);
        if (Workspace?.GetCapturedControl(_swipePointer) == this)
            Workspace.ReleasePointer(this, _swipePointer);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_trackingSwipe && !HasSwipeCapture(this, _swipePointer))
            CancelSwipe();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => CancelSwipe();

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (pointer == _swipePointer)
            CancelSwipe();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(IsSwiping))
            return;
        if (notification.PropertyName is nameof(Content) or nameof(ScrollBarThickness) or nameof(HorizontalScrollBarVisibility) or nameof(VerticalScrollBarVisibility))
            _layoutCacheReady = false;
        if (notification.PropertyName is nameof(IsSwipeEnabled) or nameof(IsMouseDragEnabled) or nameof(SwipeThreshold))
        {
            CancelSwipe();
            return;
        }

        if (notification.PropertyName is nameof(Content) or nameof(HorizontalScrollBarVisibility) or nameof(VerticalScrollBarVisibility) || notification.PropertyName == nameof(IsEnabled) && !IsEnabled || notification.PropertyName == nameof(IsVisible) && !IsVisible || notification.PropertyName == nameof(IsHitTestVisible) && !IsHitTestVisible)
            CancelSwipe();
        base.OnPropertyChanged(notification);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (_horizontal && _vertical)
            context.DrawSkinPart(this, "Corner", new Bounds2(_viewport.Right - Bounds.X, _viewport.Bottom - Bounds.Y, ConfigScrollBarThickness, ConfigScrollBarThickness));
    }

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(IsSwipeEnabled):
                {
                    bool value = Converters.Convert<bool>("Boolean", entry.Value);
                    if (_styleIsSwipeEnabled.Apply(value))
                        NotifyPropertyChanged(IsSwipeEnabledArgs);
                    break;
                }

                case nameof(IsMouseDragEnabled):
                {
                    bool value = Converters.Convert<bool>("Boolean", entry.Value);
                    if (_styleIsMouseDragEnabled.Apply(value))
                        NotifyPropertyChanged(IsMouseDragEnabledArgs);
                    break;
                }

                case nameof(SwipeThreshold):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    if (!float.IsFinite(value) || value <= 0)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (_styleSwipeThreshold.Apply(value))
                        NotifyPropertyChanged(SwipeThresholdArgs);
                    break;
                }

                case nameof(ScrollBarThickness):
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case nameof(HorizontalScrollBarVisibility):
                    if (_styleHorizontalScrollBarVisibility.Apply(Converters.Convert<ScrollBarVisibility>("ScrollBarVisibility", entry.Value)))
                        NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
                    break;
                case nameof(VerticalScrollBarVisibility):
                    if (_styleVerticalScrollBarVisibility.Apply(Converters.Convert<ScrollBarVisibility>("ScrollBarVisibility", entry.Value)))
                        NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case nameof(IsSwipeEnabled):
                return _styleIsSwipeEnabled.HasLocalValue;
            case nameof(IsMouseDragEnabled):
                return _styleIsMouseDragEnabled.HasLocalValue;
            case nameof(SwipeThreshold):
                return _styleSwipeThreshold.HasLocalValue;
            case nameof(ScrollBarThickness):
                return _styleScrollBarThickness.HasLocalValue;
            case nameof(HorizontalScrollBarVisibility):
                return _styleHorizontalScrollBarVisibility.HasLocalValue;
            case nameof(VerticalScrollBarVisibility):
                return _styleVerticalScrollBarVisibility.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(IsSwipeEnabled):
                if (_styleIsSwipeEnabled.ClearLocal())
                    NotifyPropertyChanged(IsSwipeEnabledArgs);
                return;
            case nameof(IsMouseDragEnabled):
                if (_styleIsMouseDragEnabled.ClearLocal())
                    NotifyPropertyChanged(IsMouseDragEnabledArgs);
                return;
            case nameof(SwipeThreshold):
                if (_styleSwipeThreshold.ClearLocal())
                    NotifyPropertyChanged(SwipeThresholdArgs);
                return;
            case nameof(ScrollBarThickness):
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case nameof(HorizontalScrollBarVisibility):
                if (_styleHorizontalScrollBarVisibility.ClearLocal())
                    NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
                return;
            case nameof(VerticalScrollBarVisibility):
                if (_styleVerticalScrollBarVisibility.ClearLocal())
                    NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _styleIsSwipeEnabled.BeginStyle(true);
        _styleIsMouseDragEnabled.BeginStyle(false);
        _styleSwipeThreshold.BeginStyle(12);
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleHorizontalScrollBarVisibility.BeginStyle(ScrollBarVisibility.Auto))
            NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        if (_styleVerticalScrollBarVisibility.BeginStyle(ScrollBarVisibility.Auto))
            NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
    }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ValidateChild(child);
        Content = child;
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(Content, child))
            Content = null;
    }

    /// <inheritdoc/>
    public override void Clear() => Content = null;

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsSwipeEnabled.CommitStyle())
            NotifyPropertyChanged(IsSwipeEnabledArgs);
        if (_styleIsMouseDragEnabled.CommitStyle())
            NotifyPropertyChanged(IsMouseDragEnabledArgs);
        if (_styleSwipeThreshold.CommitStyle())
            NotifyPropertyChanged(SwipeThresholdArgs);
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleHorizontalScrollBarVisibility.CommitStyle())
            NotifyPropertyChanged(HorizontalScrollBarVisibilityChangedArgs);
        if (_styleVerticalScrollBarVisibility.CommitStyle())
            NotifyPropertyChanged(VerticalScrollBarVisibilityChangedArgs);
    }
}

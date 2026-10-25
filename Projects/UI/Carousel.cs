using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>Presents retained pages without tab headers, with optional arrows, swipe gestures, indicators and timed transitions.</summary>
/// <remarks>Only the selected and outgoing pages participate in traversal. Animation advances exclusively through Update;
/// inactive pages retain ownership and state. A new navigation request completes the previous transition before starting another.</remarks>
public class Carousel : ContainerControl
{
    /// <summary>Caches the notification for AutoPlayInterval changes.</summary>
    private static readonly PropertyChangedEventArgs AutoPlayIntervalArgs = new(nameof(AutoPlayInterval));

    /// <summary>Caches the notification for IndicatorSize changes.</summary>
    private static readonly PropertyChangedEventArgs IndicatorSizeArgs = new(nameof(IndicatorSize));

    /// <summary>Caches the notification for IndicatorSpacing changes.</summary>
    private static readonly PropertyChangedEventArgs IndicatorSpacingArgs = new(nameof(IndicatorSpacing));

    /// <summary>Caches the notification for IsLooping changes.</summary>
    private static readonly PropertyChangedEventArgs IsLoopingArgs = new(nameof(IsLooping));

    /// <summary>Caches the property change notification for Items.</summary>
    private static readonly PropertyChangedEventArgs ItemsArgs = new(nameof(Items));

    /// <summary>Caches the notification for NavigationButtonSize changes.</summary>
    private static readonly PropertyChangedEventArgs NavigationButtonSizeArgs = new(nameof(NavigationButtonSize));

    /// <summary>Caches the notification for NavigationMode changes.</summary>
    private static readonly PropertyChangedEventArgs NavigationModeArgs = new(nameof(NavigationMode));

    /// <summary>Caches the notification for Orientation changes.</summary>
    private static readonly PropertyChangedEventArgs OrientationArgs = new(nameof(Orientation));

    /// <summary>Caches the property change notification for Progress.</summary>
    private static readonly PropertyChangedEventArgs ProgressArgs = new(nameof(TransitionProgress));

    /// <summary>Caches the property change notification for SelectedIndex.</summary>
    private static readonly PropertyChangedEventArgs SelectedIndexArgs = new(nameof(SelectedIndex));

    /// <summary>Caches the property change notification for SelectedItem.</summary>
    private static readonly PropertyChangedEventArgs SelectedItemArgs = new(nameof(SelectedItem));

    /// <summary>Caches the notification for ShowIndicators changes.</summary>
    private static readonly PropertyChangedEventArgs ShowIndicatorsArgs = new(nameof(ShowIndicators));

    /// <summary>Caches the notification for Spacing changes.</summary>
    private static readonly PropertyChangedEventArgs SpacingArgs = new(nameof(Spacing));

    /// <summary>Caches the notification for SwipeThreshold changes.</summary>
    private static readonly PropertyChangedEventArgs SwipeThresholdArgs = new(nameof(SwipeThreshold));

    /// <summary>Caches the notification for Transition changes.</summary>
    private static readonly PropertyChangedEventArgs TransitionArgs = new(nameof(Transition));

    /// <summary>Caches the notification for TransitionDuration changes.</summary>
    private static readonly PropertyChangedEventArgs TransitionDurationArgs = new(nameof(TransitionDuration));

    /// <summary>Caches the property change notification for Transitioning.</summary>
    private static readonly PropertyChangedEventArgs TransitioningArgs = new(nameof(IsTransitioning));

    /// <summary>Retains the next-page navigation button.</summary>
    private readonly Button _next = new()
    {
        StyleId = "Carousel.Next",
        ZIndex = 2
    };

    /// <summary>Retains the previous-page navigation button.</summary>
    private readonly Button _previous = new()
    {
        StyleId = "Carousel.Previous",
        ZIndex = 2
    };

    /// <summary>Maps retained page instances to their private ownership and animation hosts.</summary>
    private readonly Dictionary<Control, CarouselPageHost> _hosts = new(ReferenceEqualityComparer.Instance);

    /// <summary>Retains the existing clickable page indicator buttons.</summary>
    private readonly List<ToggleButton> _indicators = new();

    /// <summary>Arranges page indicators independently of the page viewport.</summary>
    private readonly StackPanel _indicatorPanel = new()
    {
        Orientation = StackOrientation.Horizontal,
        ZIndex = 2
    };

    /// <summary>Caches the arranged page area after reserving navigation space.</summary>
    private Bounds2 _viewport;

    /// <summary>Stores the host for the current destination page.</summary>
    private CarouselPageHost? _incoming;

    /// <summary>Stores the host retained only for the current transition.</summary>
    private CarouselPageHost? _outgoing;

    /// <summary>Stores the selected source page independently of its current collection index.</summary>
    private Control? _selectedItem;

    /// <summary>Identifies the only pointer allowed to complete the pending gesture.</summary>
    private ControlPointer _gesturePointer;

    /// <summary>Stores the styled and locally overridden NavigationMode value.</summary>
    private StyleProperty<CarouselNavigationMode> _styleNavigationMode = new(CarouselNavigationMode.All);

    /// <summary>Stores the styled and locally overridden Transition value.</summary>
    private StyleProperty<CarouselTransition> _styleTransition = new(CarouselTransition.Slide);

    /// <summary>Stores the styled and locally overridden Orientation value.</summary>
    private StyleProperty<ControlOrientation> _styleOrientation = new(ControlOrientation.Horizontal);

    /// <summary>Stores the styled and locally overridden IsLooping value.</summary>
    private StyleProperty<bool> _styleIsLooping = new(false);

    /// <summary>Stores the styled and locally overridden ShowIndicators value.</summary>
    private StyleProperty<bool> _styleShowIndicators = new(true);

    /// <summary>Stores the styled and locally overridden AutoPlayInterval value.</summary>
    private StyleProperty<float> _styleAutoPlayInterval = new(0);

    /// <summary>Stores the styled and locally overridden IndicatorSize value.</summary>
    private StyleProperty<float> _styleIndicatorSize = new(16);

    /// <summary>Stores the styled and locally overridden IndicatorSpacing value.</summary>
    private StyleProperty<float> _styleIndicatorSpacing = new(6);

    /// <summary>Stores the styled and locally overridden NavigationButtonSize value.</summary>
    private StyleProperty<float> _styleNavigationButtonSize = new(36);

    /// <summary>Stores the styled and locally overridden Spacing value.</summary>
    private StyleProperty<float> _styleSpacing = new(8);

    /// <summary>Stores the styled and locally overridden SwipeThreshold value.</summary>
    private StyleProperty<float> _styleSwipeThreshold = new(24);

    /// <summary>Stores the styled and locally overridden TransitionDuration value.</summary>
    private StyleProperty<float> _styleTransitionDuration = new(0.25f);

    /// <summary>Stores the workspace position where the current gesture began.</summary>
    private Vector2 _gestureStart;

    /// <summary>Records whether the current transition pages have valid measurements.</summary>
    private bool _pagesMeasured;

    /// <summary>Records whether the caller explicitly requested an empty selection.</summary>
    private bool _selectionCleared;

    /// <summary>Records whether the swipe has exceeded its threshold and owns capture.</summary>
    private bool _swiping;

    /// <summary>Records whether one pointer has a pending swipe candidate.</summary>
    private bool _tracking;

    /// <summary>Accumulates supplied Update time toward automatic advancement.</summary>
    private double _autoElapsed;

    /// <summary>Stores normalized transition progress, with one representing an idle carousel.</summary>
    private float _progress = 1;

    /// <summary>Stores positive forward or negative backward slide direction.</summary>
    private int _direction = 1;

    /// <summary>Stores the selected index, or minus one for explicitly empty content.</summary>
    private int _selectedIndex = -1;

    /// <summary>Occurs after selected index and selected page notifications have been raised.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Creates an empty carousel with automatically registered navigation controls.</summary>
    public Carousel()
    {
        Items = new CarouselItemCollection(this);
        base.Add(_previous);
        base.Add(_next);
        base.Add(_indicatorPanel);
        _previous.Clicked += (_, _) => Previous();
        _next.Clicked += (_, _) => Next();
    }

    /// <summary>Gets retained pages. Any unowned Control can be used directly; no TabItem wrapper is required.</summary>
    /// <remarks>The first added page is selected automatically unless selection was explicitly cleared with -1.</remarks>
    public CarouselItemCollection Items
    {
        get;
    }

    /// <summary>Gets or sets the selected page index. Minus one explicitly hides all pages.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The index is outside [-1, Items.Count).</exception>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            VerifyTreeMutation();
            ValidateIndex(value);
            _selectionCleared = value == -1;
            Select(value, value < _selectedIndex ? -1 : 1, true);
        }
    }

    /// <summary>Gets or sets the selected retained page. Null clears selection.</summary>
    /// <exception cref = "ArgumentException">The supplied control is not a page in this carousel.</exception>
    public Control? SelectedItem
    {
        get => _selectedItem;
        set
        {
            int index = value is null ? -1 : Items.IndexOf(value);
            if (value is not null && index < 0)
                throw new ArgumentException("The control is not a carousel page.", nameof(value));
            SelectedIndex = index;
        }
    }

    /// <summary>Gets the retained previous-page button for optional customization.</summary>
    public Button PreviousButton => _previous;

    /// <summary>Gets the retained next-page button for optional customization.</summary>
    public Button NextButton => _next;

    /// <summary>Gets the arranged page viewport in workspace logical coordinates.</summary>
    public Bounds2 ViewportBounds => _viewport;

    /// <summary>Gets whether an outgoing page is currently transitioning to the selected page.</summary>
    public bool IsTransitioning => _outgoing is not null;

    /// <summary>Gets normalized transition progress, or one when no transition is active.</summary>
    public float TransitionProgress => _progress;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Carousel";

    /// <summary>Gets whether pages and swipe distances use the horizontal axis.</summary>
    private bool Horizontal => Orientation == ControlOrientation.Horizontal;

    /// <summary>Gets or sets the page and swipe axis. Defaults to Horizontal.</summary>
    public ControlOrientation Orientation
    {
        get => _styleOrientation.Value;
        set
        {
            VerifyTreeMutation();
            if (value is not (ControlOrientation.Horizontal or ControlOrientation.Vertical))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleOrientation.SetLocal(value))
                NotifyPropertyChanged(OrientationArgs);
        }
    }

    /// <summary>Gets or sets the enabled navigation inputs. Defaults to All; flags can be combined.</summary>
    public CarouselNavigationMode NavigationMode
    {
        get => _styleNavigationMode.Value;
        set
        {
            VerifyTreeMutation();
            if ((value & ~CarouselNavigationMode.All) != 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleNavigationMode.SetLocal(value))
                NotifyPropertyChanged(NavigationModeArgs);
        }
    }

    /// <summary>Gets or sets whether clickable page indicators are shown. Defaults to true.</summary>
    public bool ShowIndicators
    {
        get => _styleShowIndicators.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleShowIndicators.SetLocal(value))
                NotifyPropertyChanged(ShowIndicatorsArgs);
        }
    }

    /// <summary>Gets or sets whether Next and Previous wrap at the collection boundaries. Defaults to false.</summary>
    public bool IsLooping
    {
        get => _styleIsLooping.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleIsLooping.SetLocal(value))
                NotifyPropertyChanged(IsLoopingArgs);
        }
    }

    /// <summary>Gets or sets the page transition effect. Defaults to Slide.</summary>
    public CarouselTransition Transition
    {
        get => _styleTransition.Value;
        set
        {
            VerifyTreeMutation();
            if (value is < CarouselTransition.None or > CarouselTransition.Fade)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleTransition.SetLocal(value))
                NotifyPropertyChanged(TransitionArgs);
        }
    }

    /// <summary>Gets or sets the finite transition duration in seconds. Zero switches immediately. Defaults to 0.25.</summary>
    public float TransitionDuration
    {
        get => _styleTransitionDuration.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, false);
            if (_styleTransitionDuration.SetLocal(value))
                NotifyPropertyChanged(TransitionDurationArgs);
        }
    }

    /// <summary>Gets or sets the positive movement threshold in logical UI units before a swipe takes capture. Defaults to 24.</summary>
    public float SwipeThreshold
    {
        get => _styleSwipeThreshold.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, true);
            if (_styleSwipeThreshold.SetLocal(value))
                NotifyPropertyChanged(SwipeThresholdArgs);
        }
    }

    /// <summary>Gets or sets the positive logical size of navigation buttons. Defaults to 36.</summary>
    public float NavigationButtonSize
    {
        get => _styleNavigationButtonSize.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, true);
            if (_styleNavigationButtonSize.SetLocal(value))
                NotifyPropertyChanged(NavigationButtonSizeArgs);
        }
    }

    /// <summary>Gets or sets the positive logical size of each page indicator. Defaults to 16.</summary>
    public float IndicatorSize
    {
        get => _styleIndicatorSize.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, true);
            if (_styleIndicatorSize.SetLocal(value))
                NotifyPropertyChanged(IndicatorSizeArgs);
        }
    }

    /// <summary>Gets or sets the nonnegative logical gap between page indicators. Defaults to 6.</summary>
    public float IndicatorSpacing
    {
        get => _styleIndicatorSpacing.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, false);
            if (_styleIndicatorSpacing.SetLocal(value))
                NotifyPropertyChanged(IndicatorSpacingArgs);
        }
    }

    /// <summary>Gets or sets the nonnegative logical gap between the page and navigation controls. Defaults to 8.</summary>
    public float Spacing
    {
        get => _styleSpacing.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, false);
            if (_styleSpacing.SetLocal(value))
                NotifyPropertyChanged(SpacingArgs);
        }
    }

    /// <summary>Gets or sets the finite automatic page interval in seconds. Zero disables automatic advancement. Defaults to zero.</summary>
    public float AutoPlayInterval
    {
        get => _styleAutoPlayInterval.Value;
        set
        {
            VerifyTreeMutation();
            ValidateNumber(value, false);
            if (_styleAutoPlayInterval.SetLocal(value))
                NotifyPropertyChanged(AutoPlayIntervalArgs);
        }
    }

    /// <summary>Selects the next page, optionally wrapping at the end.</summary>
    /// <returns>True if selection changed; false for an empty collection or a non-looping boundary.</returns>
    public bool Next() => Step(1);

    /// <summary>Selects the previous page, optionally wrapping at the beginning.</summary>
    /// <returns>True if selection changed; false for an empty collection or a non-looping boundary.</returns>
    public bool Previous() => Step(-1);

    /// <summary>Navigates to a page index using the configured transition.</summary>
    /// <param name = "index">The destination index, or -1 to clear selection.</param>
    /// <returns>True if selection changed.</returns>
    public bool GoTo(int index)
    {
        int previous = SelectedIndex;
        SelectedIndex = index;
        return previous != SelectedIndex;
    }

    /// <summary>Navigates by one page while respecting looping and empty selection.</summary>
    /// <param name = "direction">One for forward navigation or minus one for backward navigation.</param>
    /// <returns>True when the selected index or page changed.</returns>
    private bool Step(int direction)
    {
        VerifyTreeMutation();
        if (Items.Count == 0)
            return false;
        int next = _selectedIndex < 0 ? direction > 0 ? 0 : Items.Count - 1 : _selectedIndex + direction;
        if (next < 0 || next >= Items.Count)
        {
            if (!IsLooping)
                return false;
            next = next < 0 ? Items.Count - 1 : 0;
        }

        _selectionCleared = false;
        return Select(next, direction, true);
    }

    /// <summary>Rejects page indices outside the retained collection.</summary>
    /// <param name = "index">The requested index, allowing minus one for empty selection.</param>
    private void ValidateIndex(int index)
    {
        if (index < -1 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <summary>Commits coherent selection state and schedules the corresponding page traversal.</summary>
    /// <param name = "index">The destination index, or minus one for no content.</param>
    /// <param name = "direction">The signed direction used for sliding transitions.</param>
    /// <param name = "animate">Whether a valid previous page may participate in a transition.</param>
    /// <returns>True when the selected index or page changed.</returns>
    private bool Select(int index, int direction, bool animate)
    {
        Control? page = index < 0 ? null : Items[index];
        bool changed = !ReferenceEquals(page, _selectedItem);
        int oldIndex = _selectedIndex;
        if (!changed && oldIndex == index)
            return false;
        FinishTransition();
        CarouselPageHost? previous = _incoming;
        if (Workspace?.FocusedControl is Control focused)
        {
            for (Control? node = focused; node is not null; node = node.Parent)
                if (ReferenceEquals(node, previous))
                {
                    Workspace.Focus(IsFocusable && IsAvailable() ? this : null);
                    break;
                }
        }

        _selectedIndex = index;
        _selectedItem = page;
        _incoming = page is null ? null : _hosts[page];
        _autoElapsed = 0;
        _pagesMeasured = false;
        CancelGesture();
        if (changed && animate && previous is not null && _incoming is not null && Workspace is not null && _viewport.Width > 0 && _viewport.Height > 0 && Transition != CarouselTransition.None && TransitionDuration > 0)
        {
            _outgoing = previous;
            _direction = direction;
            _progress = 0;
            NotifyPropertyChanged(TransitioningArgs);
            NotifyPropertyChanged(ProgressArgs);
        }

        SynchronizeLater();
        InvalidateMeasure();
        if (oldIndex != index)
            NotifyPropertyChanged(SelectedIndexArgs);
        if (changed)
        {
            NotifyPropertyChanged(SelectedItemArgs);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        return changed || oldIndex != index;
    }

    /// <summary>Reserves an unowned page inside its persistent private host.</summary>
    /// <param name = "page">The page to validate and reserve immediately.</param>
    internal void ReservePage(Control page)
    {
        ValidateChild(page);
        var host = new CarouselPageHost
        {
            Content = page
        };
        ReserveChild(host);
        _hosts.Add(page, host);
    }

    /// <summary>Releases a page immediately and defers removal of its committed host when necessary.</summary>
    /// <param name = "page">The retained page whose ownership must be released.</param>
    internal void ReleasePage(Control page)
    {
        CarouselPageHost host = _hosts[page];
        host.Content = null;
        base.Remove(host);
        _hosts.Remove(page);
    }

    /// <summary>Preserves selection identity after a collection mutation and invalidates navigation layout.</summary>
    internal void PagesChanged()
    {
        FinishTransition();
        int index = _selectedItem is null ? -1 : Items.IndexOf(_selectedItem);
        if (index < 0 && Items.Count > 0 && !_selectionCleared)
            index = Math.Clamp(_selectedIndex, 0, Items.Count - 1);
        Select(index, 1, false);
        _pagesMeasured = false;
        SynchronizeLater();
        NotifyPropertyChanged(ItemsArgs);
    }

    /// <summary>Runs page attachment changes immediately or at the next safe traversal boundary.</summary>
    private void SynchronizeLater()
    {
        if (Workspace is Workspace workspace)
            workspace.Mutate(SynchronizePages);
        else
            SynchronizePages();
    }

    /// <summary>Attaches only incoming and outgoing hosts while retaining inactive ownership reservations.</summary>
    private void SynchronizePages()
    {
        foreach (CarouselPageHost host in _hosts.Values)
        {
            bool active = ReferenceEquals(host, _incoming) || ReferenceEquals(host, _outgoing);
            host.IsHitTestVisible = ReferenceEquals(host, _incoming) && !IsTransitioning;
            PresentOwnedChild(host, active);
        }
    }

    /// <inheritdoc/>
    public override void Add(Control child) => Items.Add(child);

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        Items.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear() => Items.Clear();

    /// <inheritdoc/>
    internal override Bounds2 GetChildClip(Control child) => child is CarouselPageHost ? _viewport : ContentBounds;

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        SynchronizePages();
        UpdateNavigation();
    }

    /// <summary>Reuses navigation controls, updates availability and reconciles indicator count.</summary>
    private void UpdateNavigation()
    {
        bool arrows = (NavigationMode & CarouselNavigationMode.Arrows) != 0 && Items.Count > 1;
        _previous.IsVisible = _next.IsVisible = arrows;
        _previous.IsEnabled = Items.Count > 0 && (IsLooping || _selectedIndex > 0);
        _next.IsEnabled = Items.Count > 0 && (IsLooping || _selectedIndex < Items.Count - 1);
        _previous.Content = Orientation == ControlOrientation.Horizontal ? "‹" : "↑";
        _next.Content = Orientation == ControlOrientation.Horizontal ? "›" : "↓";
        _previous.Width = _previous.Height = _next.Width = _next.Height = NavigationButtonSize;
        while (_indicators.Count > Items.Count)
        {
            int last = _indicators.Count - 1;
            _indicatorPanel.Remove(_indicators[last]);
            _indicators.RemoveAt(last);
        }

        while (_indicators.Count < Items.Count)
        {
            int index = _indicators.Count;
            var button = new CarouselIndicatorButton
            {
                StyleId = "Carousel.Indicator"
            };
            button.Clicked += (_, _) => GoTo(index);
            _indicators.Add(button);
            _indicatorPanel.Add(button);
        }

        for (int i = 0; i < _indicators.Count; i++)
        {
            _indicators[i].IsChecked = i == _selectedIndex;
            _indicators[i].Width = _indicators[i].Height = IndicatorSize;
        }

        _indicatorPanel.Spacing = IndicatorSpacing;
        _indicatorPanel.IsVisible = ShowIndicators && Items.Count > 1;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _previous.Measure(context, available);
        _next.Measure(context, available);
        _indicatorPanel.Measure(context, available);
        float arrows = _previous.IsVisible ? 2 * (NavigationButtonSize + Spacing) : 0;
        float indicators = _indicatorPanel.IsVisible ? _indicatorPanel.DesiredSize.Height + Spacing : 0;
        Size2 pageSpace = new(MathF.Max(0, available.Width - (Horizontal ? arrows : 0)), MathF.Max(0, available.Height - indicators - (Horizontal ? 0 : arrows)));
        Size2 size = Size2.Zero;
        if (_incoming is not null)
        {
            _incoming.Measure(context, pageSpace);
            size = _incoming.DesiredSize;
        }

        if (_outgoing is not null)
        {
            _outgoing.Measure(context, pageSpace);
            size = new Size2(MathF.Max(size.Width, _outgoing.DesiredSize.Width), MathF.Max(size.Height, _outgoing.DesiredSize.Height));
        }

        _pagesMeasured = true;
        return new Size2(MathF.Max(size.Width + (Horizontal ? arrows : 0), _indicatorPanel.IsVisible ? _indicatorPanel.DesiredSize.Width : 0), size.Height + indicators + (Horizontal ? 0 : arrows));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float strip = _indicatorPanel.IsVisible ? MathF.Min(bounds.Height, _indicatorPanel.DesiredSize.Height) : 0;
        float gap = strip > 0 ? MathF.Min(Spacing, MathF.Max(0, bounds.Height - strip)) : 0;
        Bounds2 pageArea = new(bounds.X, bounds.Y, bounds.Width, MathF.Max(0, bounds.Height - strip - gap));
        _indicatorPanel.Arrange(new Bounds2(bounds.X + MathF.Max(0, (bounds.Width - _indicatorPanel.DesiredSize.Width) / 2), bounds.Bottom - strip, MathF.Min(bounds.Width, _indicatorPanel.DesiredSize.Width), strip), true);
        float length = Horizontal ? pageArea.Width : pageArea.Height;
        float extent = _previous.IsVisible ? MathF.Min(NavigationButtonSize, length / 2) : 0;
        float arrowGap = extent > 0 ? MathF.Min(Spacing, MathF.Max(0, (length - 2 * extent) / 2)) : 0;
        _viewport = Horizontal ? new Bounds2(pageArea.X + extent + arrowGap, pageArea.Y, MathF.Max(0, pageArea.Width - 2 * (extent + arrowGap)), pageArea.Height) : new Bounds2(pageArea.X, pageArea.Y + extent + arrowGap, pageArea.Width, MathF.Max(0, pageArea.Height - 2 * (extent + arrowGap)));
        float cross = MathF.Min(NavigationButtonSize, Horizontal ? pageArea.Height : pageArea.Width);
        _previous.Arrange(Horizontal ? new Bounds2(pageArea.X, pageArea.Y + (pageArea.Height - cross) / 2, extent, cross) : new Bounds2(pageArea.X + (pageArea.Width - cross) / 2, pageArea.Y, cross, extent), true);
        _next.Arrange(Horizontal ? new Bounds2(pageArea.Right - extent, pageArea.Y + (pageArea.Height - cross) / 2, extent, cross) : new Bounds2(pageArea.X + (pageArea.Width - cross) / 2, pageArea.Bottom - extent, cross, extent), true);
        ArrangePages();
    }

    /// <summary>Arranges measured transition hosts without remeasuring their retained page content.</summary>
    private void ArrangePages()
    {
        if (!_pagesMeasured)
            return;
        float length = Horizontal ? _viewport.Width : _viewport.Height;
        bool slide = IsTransitioning && Transition == CarouselTransition.Slide;
        float incomingOffset = slide ? _direction * (1 - _progress) * length : 0;
        float outgoingOffset = slide ? -_direction * _progress * length : 0;
        if (_outgoing is not null)
        {
            _outgoing.Opacity = Transition == CarouselTransition.Fade ? 1 - _progress : 1;
            _outgoing.Arrange(PageSlot(outgoingOffset), true);
        }

        if (_incoming is not null)
        {
            _incoming.Opacity = IsTransitioning && Transition == CarouselTransition.Fade ? _progress : 1;
            _incoming.Arrange(PageSlot(incomingOffset), true);
        }
    }

    /// <summary>Offsets the cached viewport along the selected slide axis.</summary>
    /// <param name = "offset">The signed logical displacement along the main axis.</param>
    /// <returns>The displaced page slot in workspace logical coordinates.</returns>
    private Bounds2 PageSlot(float offset) => new(_viewport.X + (Horizontal ? offset : 0), _viewport.Y + (Horizontal ? 0 : offset), _viewport.Width, _viewport.Height);

    /// <summary>Restores the destination page opacity and schedules outgoing page detachment.</summary>
    private void FinishTransition()
    {
        if (!IsTransitioning)
            return;
        _outgoing = null;
        _progress = 1;
        if (_incoming is not null)
            _incoming.Opacity = 1;
        SynchronizeLater();
        NotifyPropertyChanged(TransitioningArgs);
        NotifyPropertyChanged(ProgressArgs);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_tracking && !ScrollViewer.HasSwipeCapture(this, _gesturePointer))
            CancelGesture();
        double seconds = Math.Max(0, elapsed.TotalSeconds);
        if (IsTransitioning)
        {
            _progress = TransitionDuration <= 0 ? 1 : MathF.Min(1, _progress + (float)(seconds / TransitionDuration));
            if (_progress >= 1)
                FinishTransition();
            else
                NotifyPropertyChanged(ProgressArgs);
            ArrangePages();
            return;
        }

        if (AutoPlayInterval > 0 && SelectedIndex >= 0 && Items.Count > 1 && !_tracking && IsAvailable())
        {
            _autoElapsed += seconds;
            if (_autoElapsed >= AutoPlayInterval)
            {
                _autoElapsed = 0;
                Next();
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPreviewInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var input = args.Input;
        if (IsTransitioning && input.Type is ControlInputEventType.KeyDown or ControlInputEventType.KeyUp or ControlInputEventType.TextInput)
        {
            for (Control? node = args.Source; node is not null && node != this; node = node.Parent)
                if (node is CarouselPageHost)
                {
                    args.Handled = true;
                    return;
                }
        }

        if (input.Type == ControlInputEventType.PointerCancel && _tracking && input.Pointer == _gesturePointer)
        {
            CancelGesture();
            return;
        }

        if ((NavigationMode & CarouselNavigationMode.Swipe) == 0)
            return;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            if (_tracking && !ScrollViewer.HasSwipeCapture(this, _gesturePointer))
                CancelGesture();
            if (ScrollViewer.IsDirectManipulationTarget(args.Source, this))
                return;
            if (!Contains(_viewport, input.Position.X, input.Position.Y) || _tracking)
                return;
            for (Control? node = args.Source; node is not null && node != this; node = node.Parent)
                if (node is Carousel)
                    return;
            if (IsTransitioning)
            {
                args.Handled = true;
                return;
            }

            _tracking = true;
            _gesturePointer = input.Pointer;
            _gestureStart = input.Position;
            // Keep the original route available for cancellation, even for non-interactive content.
            context.CapturePointer(args.Source);
        }

        if (!_tracking || input.Pointer != _gesturePointer)
            return;
        Vector2 delta = input.Position - _gestureStart;
        float main = Horizontal ? delta.X : delta.Y;
        float cross = Horizontal ? delta.Y : delta.X;
        if (input.Type == ControlInputEventType.PointerMove)
        {
            if (!_swiping)
            {
                for (Control? node = args.Source; node is not null && node != this; node = node.Parent)
                    if (node is ScrollViewer viewer && (viewer.IsSwiping || viewer.CanConsumeSwipe(input.Pointer, delta)))
                        return;
            }

            if (!_swiping && MathF.Abs(cross) >= SwipeThreshold && MathF.Abs(cross) > MathF.Abs(main))
            {
                _tracking = false;
                return;
            }

            if (!_swiping && MathF.Abs(main) >= SwipeThreshold && MathF.Abs(main) > MathF.Abs(cross))
            {
                context.CapturePointer(this);
                _swiping = true;
            }

            if (_swiping)
                args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            bool swipe = _swiping;
            _tracking = _swiping = false;
            if (!swipe)
                return;
            args.Handled = true;
            context.ReleasePointer(this);
            if (IsFocusable)
                context.Focus(this);
            if (MathF.Abs(main) >= SwipeThreshold)
                Step(main < 0 ? 1 : -1);
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if ((NavigationMode & CarouselNavigationMode.Keyboard) == 0 || args.Input.Type != ControlInputEventType.KeyDown || args.Input.IsRepeat)
            return;
        Keys key = args.Input.Key;
        if (key == (Horizontal ? Keys.Right : Keys.Down))
        {
            Next();
            args.Handled = true;
        }
        else if (key == (Horizontal ? Keys.Left : Keys.Up))
        {
            Previous();
            args.Handled = true;
        }
        else if (key == Keys.Home && Items.Count > 0)
        {
            GoTo(0);
            args.Handled = true;
        }
        else if (key == Keys.End && Items.Count > 0)
        {
            GoTo(Items.Count - 1);
            args.Handled = true;
        }
    }

    /// <summary>Clears pending gesture state, releases carousel capture and resets automatic timing.</summary>
    private void CancelGesture()
    {
        _tracking = _swiping = false;
        if (Workspace?.GetCapturedControl(_gesturePointer) == this)
            Workspace.ReleasePointer(this, _gesturePointer);
        _autoElapsed = 0;
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => CancelGesture();

    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (pointer == _gesturePointer)
            _tracking = _swiping = false;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName is nameof(IsTransitioning) or nameof(TransitionProgress))
        {
            InvalidateVisual();
            return;
        }
        if (notification.PropertyName is nameof(Orientation) or nameof(Transition) or nameof(TransitionDuration))
            FinishTransition();
        if (notification.PropertyName is nameof(Orientation) or nameof(NavigationMode))
            CancelGesture();
        if (notification.PropertyName == nameof(AutoPlayInterval))
            _autoElapsed = 0;
        base.OnPropertyChanged(notification);
    }

    /// <summary>Rejects invalid logical dimensions or time intervals.</summary>
    /// <param name = "value">The numeric setting being validated.</param>
    /// <param name = "positive">Whether zero is also invalid.</param>
    private static void ValidateNumber(float value, bool positive)
    {
        if (!float.IsFinite(value) || value < 0 || (positive && value == 0))
            throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(Orientation):
                {
                    ControlOrientation value = Converters.Convert<ControlOrientation>("ControlOrientation", entry.Value);
                    if (value is not (ControlOrientation.Horizontal or ControlOrientation.Vertical))
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (_styleOrientation.Apply(value))
                        NotifyPropertyChanged(OrientationArgs);
                    break;
                }

                case nameof(NavigationMode):
                {
                    CarouselNavigationMode value = Converters.Convert<CarouselNavigationMode>("CarouselNavigationMode", entry.Value);
                    if ((value & ~CarouselNavigationMode.All) != 0)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (_styleNavigationMode.Apply(value))
                        NotifyPropertyChanged(NavigationModeArgs);
                    break;
                }

                case nameof(ShowIndicators):
                {
                    bool value = Converters.Convert<bool>("Boolean", entry.Value);
                    if (_styleShowIndicators.Apply(value))
                        NotifyPropertyChanged(ShowIndicatorsArgs);
                    break;
                }

                case nameof(IsLooping):
                {
                    bool value = Converters.Convert<bool>("Boolean", entry.Value);
                    if (_styleIsLooping.Apply(value))
                        NotifyPropertyChanged(IsLoopingArgs);
                    break;
                }

                case nameof(Transition):
                {
                    CarouselTransition value = Converters.Convert<CarouselTransition>("CarouselTransition", entry.Value);
                    if (value is < CarouselTransition.None or > CarouselTransition.Fade)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    if (_styleTransition.Apply(value))
                        NotifyPropertyChanged(TransitionArgs);
                    break;
                }

                case nameof(TransitionDuration):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, false);
                    if (_styleTransitionDuration.Apply(value))
                        NotifyPropertyChanged(TransitionDurationArgs);
                    break;
                }

                case nameof(SwipeThreshold):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, true);
                    if (_styleSwipeThreshold.Apply(value))
                        NotifyPropertyChanged(SwipeThresholdArgs);
                    break;
                }

                case nameof(NavigationButtonSize):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, true);
                    if (_styleNavigationButtonSize.Apply(value))
                        NotifyPropertyChanged(NavigationButtonSizeArgs);
                    break;
                }

                case nameof(IndicatorSize):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, true);
                    if (_styleIndicatorSize.Apply(value))
                        NotifyPropertyChanged(IndicatorSizeArgs);
                    break;
                }

                case nameof(IndicatorSpacing):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, false);
                    if (_styleIndicatorSpacing.Apply(value))
                        NotifyPropertyChanged(IndicatorSpacingArgs);
                    break;
                }

                case nameof(Spacing):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, false);
                    if (_styleSpacing.Apply(value))
                        NotifyPropertyChanged(SpacingArgs);
                    break;
                }

                case nameof(AutoPlayInterval):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateNumber(value, false);
                    if (_styleAutoPlayInterval.Apply(value))
                        NotifyPropertyChanged(AutoPlayIntervalArgs);
                    break;
                }
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(Orientation) => _styleOrientation.HasLocalValue,
        nameof(NavigationMode) => _styleNavigationMode.HasLocalValue,
        nameof(ShowIndicators) => _styleShowIndicators.HasLocalValue,
        nameof(IsLooping) => _styleIsLooping.HasLocalValue,
        nameof(Transition) => _styleTransition.HasLocalValue,
        nameof(TransitionDuration) => _styleTransitionDuration.HasLocalValue,
        nameof(SwipeThreshold) => _styleSwipeThreshold.HasLocalValue,
        nameof(NavigationButtonSize) => _styleNavigationButtonSize.HasLocalValue,
        nameof(IndicatorSize) => _styleIndicatorSize.HasLocalValue,
        nameof(IndicatorSpacing) => _styleIndicatorSpacing.HasLocalValue,
        nameof(Spacing) => _styleSpacing.HasLocalValue,
        nameof(AutoPlayInterval) => _styleAutoPlayInterval.HasLocalValue,
        _ => base.IsStyleLocal(property)};

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(Orientation):
                if (_styleOrientation.ClearLocal())
                    NotifyPropertyChanged(OrientationArgs);
                break;
            case nameof(NavigationMode):
                if (_styleNavigationMode.ClearLocal())
                    NotifyPropertyChanged(NavigationModeArgs);
                break;
            case nameof(ShowIndicators):
                if (_styleShowIndicators.ClearLocal())
                    NotifyPropertyChanged(ShowIndicatorsArgs);
                break;
            case nameof(IsLooping):
                if (_styleIsLooping.ClearLocal())
                    NotifyPropertyChanged(IsLoopingArgs);
                break;
            case nameof(Transition):
                if (_styleTransition.ClearLocal())
                    NotifyPropertyChanged(TransitionArgs);
                break;
            case nameof(TransitionDuration):
                if (_styleTransitionDuration.ClearLocal())
                    NotifyPropertyChanged(TransitionDurationArgs);
                break;
            case nameof(SwipeThreshold):
                if (_styleSwipeThreshold.ClearLocal())
                    NotifyPropertyChanged(SwipeThresholdArgs);
                break;
            case nameof(NavigationButtonSize):
                if (_styleNavigationButtonSize.ClearLocal())
                    NotifyPropertyChanged(NavigationButtonSizeArgs);
                break;
            case nameof(IndicatorSize):
                if (_styleIndicatorSize.ClearLocal())
                    NotifyPropertyChanged(IndicatorSizeArgs);
                break;
            case nameof(IndicatorSpacing):
                if (_styleIndicatorSpacing.ClearLocal())
                    NotifyPropertyChanged(IndicatorSpacingArgs);
                break;
            case nameof(Spacing):
                if (_styleSpacing.ClearLocal())
                    NotifyPropertyChanged(SpacingArgs);
                break;
            case nameof(AutoPlayInterval):
                if (_styleAutoPlayInterval.ClearLocal())
                    NotifyPropertyChanged(AutoPlayIntervalArgs);
                break;
            default:
                base.ResetStyleProperty(property);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _styleOrientation.BeginStyle(ControlOrientation.Horizontal);
        _styleNavigationMode.BeginStyle(CarouselNavigationMode.All);
        _styleShowIndicators.BeginStyle(true);
        _styleIsLooping.BeginStyle(false);
        _styleTransition.BeginStyle(CarouselTransition.Slide);
        _styleTransitionDuration.BeginStyle(0.25f);
        _styleSwipeThreshold.BeginStyle(24);
        _styleNavigationButtonSize.BeginStyle(36);
        _styleIndicatorSize.BeginStyle(16);
        _styleIndicatorSpacing.BeginStyle(6);
        _styleSpacing.BeginStyle(8);
        _styleAutoPlayInterval.BeginStyle(0);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleOrientation.CommitStyle())
            NotifyPropertyChanged(OrientationArgs);
        if (_styleNavigationMode.CommitStyle())
            NotifyPropertyChanged(NavigationModeArgs);
        if (_styleShowIndicators.CommitStyle())
            NotifyPropertyChanged(ShowIndicatorsArgs);
        if (_styleIsLooping.CommitStyle())
            NotifyPropertyChanged(IsLoopingArgs);
        if (_styleTransition.CommitStyle())
            NotifyPropertyChanged(TransitionArgs);
        if (_styleTransitionDuration.CommitStyle())
            NotifyPropertyChanged(TransitionDurationArgs);
        if (_styleSwipeThreshold.CommitStyle())
            NotifyPropertyChanged(SwipeThresholdArgs);
        if (_styleNavigationButtonSize.CommitStyle())
            NotifyPropertyChanged(NavigationButtonSizeArgs);
        if (_styleIndicatorSize.CommitStyle())
            NotifyPropertyChanged(IndicatorSizeArgs);
        if (_styleIndicatorSpacing.CommitStyle())
            NotifyPropertyChanged(IndicatorSpacingArgs);
        if (_styleSpacing.CommitStyle())
            NotifyPropertyChanged(SpacingArgs);
        if (_styleAutoPlayInterval.CommitStyle())
            NotifyPropertyChanged(AutoPlayIntervalArgs);
    }
}

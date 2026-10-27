using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// A clipped, keyboard-accessible selectable list with an independent scrollbar.
/// </summary>
public class ListBox : ContainerControl
{
    /// <summary>Caches changes to the touch-scroll policy.</summary>
    private static readonly PropertyChangedEventArgs IsSwipeEnabledArgs = new(nameof(IsSwipeEnabled));

    /// <summary>Caches changes to the active touch-scroll state.</summary>
    private static readonly PropertyChangedEventArgs IsSwipingArgs = new(nameof(IsSwiping));

    /// <summary>Caches notifications for changes to item height.</summary>
    private static readonly PropertyChangedEventArgs ItemHeightChangedArgs = new(nameof(ItemHeight));

    /// <summary>Caches notifications raised by mutations of the item collection.</summary>
    private static readonly PropertyChangedEventArgs ItemsCollectionArgs = new(nameof(Items));

    /// <summary>Caches notifications for changes to multiple selection.</summary>
    private static readonly PropertyChangedEventArgs MultiSelectChangedArgs = new(nameof(MultiSelect));

    /// <summary>Caches notifications for changes to scrollbar width.</summary>
    private static readonly PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));

    /// <summary>Caches notifications for changes to the primary selection.</summary>
    private static readonly PropertyChangedEventArgs SelectedIndexChangedArgs = new(nameof(SelectedIndex));

    /// <summary>Caches changes to the movement threshold.</summary>
    private static readonly PropertyChangedEventArgs SwipeThresholdArgs = new(nameof(SwipeThreshold));

    /// <summary>Caches the predicate used to discard removed or disabled selections.</summary>
    private readonly Predicate<int> _invalidSelection;

    /// <summary>Stores the selected item indices independently of the primary selection.</summary>
    private readonly SortedSet<int> _selected = new();

    /// <summary>Stores the arranged item area excluding the scrollbar.</summary>
    private Bounds2 _viewport;

    /// <summary>Retains the layout context used to measure the scrollbar during arrangement.</summary>
    private ControlLayoutContext? _layout;

    /// <summary>Identifies the pointer owning the pending or active gesture.</summary>
    private ControlPointer _swipePointer;

    /// <summary>Lazily stores the observable list-owned item collection.</summary>
    private ElementCollection<SelectorItem>? _ownedItems;

    /// <summary>Caches the font resolved during measurement for item rendering.</summary>
    private IControlFont? _font;

    /// <summary>Stores the styled touch-scroll policy.</summary>
    private StyleProperty<bool> _styleIsSwipeEnabled = new(true);

    /// <summary>Stores the styled multiple-selection policy and any local override.</summary>
    private StyleProperty<bool> _styleMultiSelect = new(false);

    /// <summary>Stores the styled item height and any local override.</summary>
    private StyleProperty<float> _styleItemHeight = new(30f);

    /// <summary>Stores the styled scrollbar width and any local override.</summary>
    private StyleProperty<float> _styleScrollBarThickness = new(20f);

    /// <summary>Stores the styled positive touch movement threshold.</summary>
    private StyleProperty<float> _styleSwipeThreshold = new(12);

    /// <summary>Stores the styled primary selection and any local override.</summary>
    private StyleProperty<int> _styleSelectedIndex = new(-1);

    /// <summary>Stores the latest position for incremental scrolling.</summary>
    private Vector2 _swipePrevious;

    /// <summary>Stores the touch-down position used to distinguish tapping from scrolling.</summary>
    private Vector2 _swipeStart;

    /// <summary>Requests bringing the primary selection into view after arrangement.</summary>
    private bool _ensureSelection;

    /// <summary>Tracks whether the list has captured an active scrolling gesture.</summary>
    private bool _isSwiping;

    /// <summary>Requests selection reconciliation after item or selection changes.</summary>
    private bool _selectionDirty = true;

    /// <summary>Tracks whether the arranged content exceeds its viewport.</summary>
    private bool _showBar;

    /// <summary>Prevents activation after a movement exceeding the tap threshold.</summary>
    private bool _swipeMoved;

    /// <summary>Tracks whether a touch is waiting to become a tap or scroll gesture.</summary>
    private bool _trackingSwipe;

    /// <summary>Tracks whether a descendant consumed earlier movement before boundary handoff.</summary>
    private bool _yieldedSwipe;

    /// <summary>Stores the update time of the previous item activation.</summary>
    private double _clickTime;

    /// <summary>Accumulates update time for deterministic double-click detection.</summary>
    private double _time;

    /// <summary>Stores the starting index for range selection.</summary>
    private int _anchor = -1;

    /// <summary>Stores the number of item columns in the arranged viewport.</summary>
    private int _columns = 1;

    /// <summary>Stores the previously activated item for double-click detection.</summary>
    private int _lastClick = -1;

    /// <summary>Stores the primary index last reported to selection listeners.</summary>
    private int _lastPrimary = -1;

    /// <summary>Stores the item pressed by the primary pointer, or minus one.</summary>
    private int _pressed = -1;

    /// <summary>
    /// Occurs when selection changed.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Occurs when item activated.
    /// </summary>
    public event EventHandler? ItemActivated;

    /// <summary>
    /// Occurs when item double clicked.
    /// </summary>
    public event EventHandler? ItemDoubleClicked;

    /// <summary>
    /// Initializes a new list box instance.
    /// </summary>
    public ListBox() : base()
    {
        _invalidSelection = IsInvalidSelection;
        ScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Vertical
        };
        base.Add(ScrollBar);
    }

    /// <summary>
    /// Gets the arranged item viewport in workspace coordinates.
    /// </summary>
    protected Bounds2 ItemViewport => _viewport;

    /// <summary>
    /// Gets the arranged number of item columns.
    /// </summary>
    protected int ItemColumns => _columns;

    /// <summary>
    /// Gets the spacing between rows and columns.
    /// </summary>
    protected virtual float ItemSpacing => 0;

    /// <summary>
    /// Gets the effective item height.
    /// </summary>
    protected virtual float ItemExtent => ItemHeight;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ListBox";

    /// <summary>
    /// Gets the observable collection of selectable items.
    /// </summary>
    public IList<SelectorItem> Items => ConfigItems;

    /// <summary>
    /// Gets the independent vertical scrollbar.
    /// </summary>
    public ScrollBar ScrollBar
    {
        get;
    }

    /// <summary>Gets or sets whether fingers can scroll the list vertically. Defaults to true; mouse and keyboard behavior is preserved.</summary>
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

    /// <summary>Gets whether a finger currently owns an active scroll gesture.</summary>
    public bool IsSwiping => _isSwiping;

    /// <summary>Gets or sets the positive touch movement threshold in logical UI units. Defaults to 12.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The threshold is nonfinite or not positive.</exception>
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

    /// <summary>
    /// Gets or sets the logical height of each item row.
    /// </summary>
    public float ItemHeight
    {
        get => ConfigItemHeight;
        set => ConfigItemHeight = value;
    }

    /// <summary>
    /// Gets or sets whether range and additive selection are enabled.
    /// </summary>
    public bool MultiSelect
    {
        get => ConfigMultiSelect;
        set => ConfigMultiSelect = value;
    }

    /// <summary>
    /// Gets or sets the primary selected item index, or minus one for no selection.
    /// </summary>
    public int SelectedIndex
    {
        get
        {
            int value = ConfigSelectedIndex;
            return value >= 0 && value < Items.Count && Items[value].IsEnabled ? value : -1;
        }

        set
        {
            if (value < -1 || value >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (value >= 0 && !Items[value].IsEnabled)
                return;
            _selected.Clear();
            if (value >= 0)
                _selected.Add(value);
            ConfigSelectedIndex = value;
            _anchor = value;
            _ensureSelection = true;
            Notify();
            EnsureVisible(value);
        }
    }

    /// <summary>
    /// Gets the primary selected item, or null when no enabled item is selected.
    /// </summary>
    public SelectorItem? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];

    /// <summary>
    /// Gets a newly allocated snapshot of the selected item indices.
    /// </summary>
    public IReadOnlyList<int> SelectedIndices => new ReadOnlyCollection<int>(_selected.ToList());

    /// <summary>
    /// Identifies keyboard activation during the ItemActivated callback.
    /// </summary>
    public bool IsKeyboardActivation
    {
        get;
        private set;
    }

    /// <summary>Gets or sets the resolved scrollbar width through its style storage.</summary>
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
    public float ScrollBarThickness
    {
        get => ConfigScrollBarThickness;
        set => ConfigScrollBarThickness = value;
    }

    /// <summary>Gets or sets the resolved row height through its style storage.</summary>
    internal float ConfigItemHeight
    {
        get => _styleItemHeight.Value;
        set
        {
            if (_styleItemHeight.SetLocal(value))
                NotifyPropertyChanged(ItemHeightChangedArgs);
        }
    }

    /// <summary>Gets or sets the resolved multiple-selection policy through its style storage.</summary>
    internal bool ConfigMultiSelect
    {
        get => _styleMultiSelect.Value;
        set
        {
            if (_styleMultiSelect.SetLocal(value))
                NotifyPropertyChanged(MultiSelectChangedArgs);
        }
    }

    /// <summary>Gets or sets the resolved primary index through its style storage.</summary>
    internal int ConfigSelectedIndex
    {
        get => _styleSelectedIndex.Value;
        set
        {
            if (_styleSelectedIndex.SetLocal(value))
                NotifyPropertyChanged(SelectedIndexChangedArgs);
        }
    }

    /// <summary>Gets the lazily created collection owned by this list.</summary>
    internal IList<SelectorItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    /// <summary>
    /// Computes the number of columns for an available width.
    /// </summary>
    /// <param name = "width">The available logical viewport width.</param>
    /// <returns>The positive number of columns.</returns>
    protected virtual int GetColumnCount(float width) => 1;

    /// <summary>
    /// Computes an item's width inside the viewport.
    /// </summary>
    /// <param name = "width">The available logical viewport width.</param>
    /// <returns>The logical width of an item.</returns>
    protected virtual float GetItemWidth(float width) => width;

    /// <summary>
    /// Measures the preferred list width without creating item controls.
    /// </summary>
    /// <param name = "font">The font used to measure item labels.</param>
    /// <returns>The preferred logical list width.</returns>
    protected virtual float MeasureItems(IControlFont font)
    {
        float width = 120;
        for (int i = 0; i < Items.Count; i++)
        {
            SelectorItem item = Items[i];
            ArgumentNullException.ThrowIfNull(item);
            width = MathF.Max(width, font.Measure(item.Text).Width + 32);
        }

        return width;
    }

    /// <summary>Calculates the total row extent including inter-row spacing.</summary>
    /// <param name = "columns">The positive number of item columns.</param>
    /// <returns>The total logical content height.</returns>
    private float ContentHeight(int columns) => Items.Count == 0 ? 0 : ((Items.Count - 1) / columns + 1) * (ItemExtent + ItemSpacing) - ItemSpacing;

    /// <summary>
    /// Gets the visible item interval, including partially visible rows.
    /// </summary>
    /// <param name = "first">Receives the first potentially visible item index.</param>
    /// <param name = "last">Receives the exclusive end of the visible interval.</param>
    protected void GetVisibleRange(out int first, out int last)
    {
        first = Math.Max(0, (int)(ScrollBar.Value / (ItemExtent + ItemSpacing))) * _columns;
        last = Math.Min(Items.Count, first + ((int)(_viewport.Height / (ItemExtent + ItemSpacing)) + 2) * _columns);
    }

    /// <summary>
    /// Gets an item's bounds in workspace coordinates.
    /// </summary>
    /// <param name = "index">The source item index.</param>
    /// <returns>The arranged logical workspace bounds of the item.</returns>
    public Bounds2 GetItemBounds(int index)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        float width = GetItemWidth(_viewport.Width);
        return new Bounds2(_viewport.X + index % _columns * (width + ItemSpacing), _viewport.Y + index / _columns * (ItemExtent + ItemSpacing) - ScrollBar.Value, width, ItemExtent);
    }

    /// <summary>Raises selection changes only when the primary selected index changes.</summary>
    private void Notify()
    {
        int current = SelectedIndex;
        if (_lastPrimary == current)
            return;
        _lastPrimary = current;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Brings an item into view while preserving the offset of an already visible oversized item.</summary>
    /// <param name = "index">The item to reveal, or minus one for no selection.</param>
    private void EnsureVisible(int index)
    {
        if (index < 0)
            return;
        float top = index / _columns * (ItemExtent + ItemSpacing), bottom = top + ItemExtent;
        if (ItemExtent > _viewport.Height)
        {
            // Keep an already visible oversized item stationary when it is selected by touch.
            if (bottom <= ScrollBar.Value || top >= ScrollBar.Value + _viewport.Height)
                ScrollBar.Value = top;
            return;
        }

        if (top < ScrollBar.Value)
            ScrollBar.Value = top;
        else if (bottom > ScrollBar.Value + _viewport.Height)
            ScrollBar.Value = bottom - _viewport.Height;
    }

    /// <summary>Applies single, range or additive selection using input modifiers.</summary>
    /// <param name = "index">The item under the pointer or keyboard target.</param>
    /// <param name = "shift">Whether to extend selection from the anchor.</param>
    /// <param name = "control">Whether to toggle an individual selection.</param>
    private void Choose(int index, bool shift, bool control)
    {
        if (index < 0 || index >= Items.Count || !Items[index].IsEnabled)
            return;
        if (!MultiSelect || (!shift && !control))
        {
            SelectedIndex = index;
            return;
        }

        if (shift && _anchor >= 0)
        {
            _selected.Clear();
            for (int i = Math.Min(_anchor, index); i <= Math.Max(_anchor, index); i++)
                if (Items[i].IsEnabled)
                    _selected.Add(i);
        }
        else
        {
            if (!_selected.Add(index))
                _selected.Remove(index);
            _anchor = index;
        }

        ConfigSelectedIndex = _selected.Contains(index) ? index : _selected.Count > 0 ? _selected.Max : -1;
        _lastPrimary = SelectedIndex;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        EnsureVisible(index);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _layout = context;
        if (!float.IsFinite(ItemExtent) || ItemExtent <= 0 || !float.IsFinite(ItemSpacing) || ItemSpacing < 0)
            throw new InvalidOperationException("Invalid item geometry.");
        _font = GetFont(context);
        float width = MeasureItems(_font);
        _showBar = ContentHeight(GetColumnCount(available.Width)) > available.Height;
        // Visibility belongs to the final viewport, not an unbounded measurement.
        ScrollBar.Measure(context, available);
        return new Size2(MathF.Min(available.Width, width + (_showBar ? ConfigScrollBarThickness : 0)), MathF.Min(available.Height, MathF.Max(ItemExtent, MathF.Min(8, Items.Count) * ItemExtent)));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        _columns = GetColumnCount(contentBounds.Width);
        _showBar = ContentHeight(_columns) > contentBounds.Height;
        float t = _showBar ? ConfigScrollBarThickness : 0;
        _viewport = new Bounds2(contentBounds.X, contentBounds.Y, MathF.Max(0, contentBounds.Width - t), contentBounds.Height);
        _columns = GetColumnCount(_viewport.Width);
        ScrollBar.IsVisible = _showBar;
        if (_layout is not null)
            ScrollBar.Measure(_layout, new Size2(contentBounds.Width, contentBounds.Height));
        ScrollBar.Maximum = MathF.Max(0, ContentHeight(_columns) - _viewport.Height);
        ScrollBar.ViewportSize = _viewport.Height;
        ScrollBar.SmallChange = ItemExtent + ItemSpacing;
        ScrollBar.LargeChange = _viewport.Height;
        ScrollBar.Arrange(new Bounds2(_viewport.Right, contentBounds.Y, t, contentBounds.Height), true);
        if (_ensureSelection)
        {
            EnsureVisible(SelectedIndex);
            _ensureSelection = false;
        }
    }

    /// <inheritdoc/>
    internal override Bounds2 GetChildClip(Control child) => base.ContentBounds;

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_trackingSwipe && !ScrollViewer.HasSwipeCapture(this, _swipePointer))
            CancelSwipe();
        _time += elapsed.TotalSeconds;
        if (_selectionDirty)
        {
            _selectionDirty = false;
            _selected.RemoveWhere(_invalidSelection);
            int index = SelectedIndex;
            if (!MultiSelect && (_selected.Count != (index < 0 ? 0 : 1) || index >= 0 && !_selected.Contains(index)))
            {
                _selected.Clear();
                if (index >= 0)
                    _selected.Add(index);
            }
        }

        Notify();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(IsSwiping))
        {
            InvalidateVisual();
            return;
        }

        if (notification.PropertyName is nameof(IsSwipeEnabled) or nameof(SwipeThreshold))
        {
            CancelSwipe();
            return;
        }

        if (notification.PropertyName == nameof(Items) || notification.PropertyName == nameof(IsEnabled) && !IsEnabled || notification.PropertyName == nameof(IsVisible) && !IsVisible || notification.PropertyName == nameof(IsHitTestVisible) && !IsHitTestVisible)
            CancelSwipe();
        if (notification.PropertyName is nameof(Items) or nameof(MultiSelect) or nameof(SelectedIndex))
            _selectionDirty = true;
        base.OnPropertyChanged(notification);
    }

    /// <summary>Identifies selections invalidated by item removal or disabling.</summary>
    /// <param name = "index">The previously selected source index.</param>
    /// <returns>True when the index no longer identifies an enabled item.</returns>
    private bool IsInvalidSelection(int index) => index >= Items.Count || !Items[index].IsEnabled;

    /// <summary>Finds the item under a logical workspace position, excluding spacing and the scrollbar.</summary>
    /// <param name = "point">The workspace position to test.</param>
    /// <returns>The source index, or minus one outside an item.</returns>
    protected int GetItemIndexAt(Vector2 point)
    {
        if (!Contains(_viewport, point.X, point.Y))
            return -1;
        int column = (int)((point.X - _viewport.X) / (GetItemWidth(_viewport.Width) + ItemSpacing));
        int row = (int)((point.Y - _viewport.Y + ScrollBar.Value) / (ItemExtent + ItemSpacing));
        int index = row * _columns + column;
        return column < _columns && index >= 0 && index < Items.Count && Contains(GetItemBounds(index), point.X, point.Y) ? index : -1;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (_trackingSwipe && e.Pointer != _swipePointer && e.Type is ControlInputEventType.PointerDown or ControlInputEventType.PointerMove or ControlInputEventType.PointerUp)
        {
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerWheel)
        {
            float old = ScrollBar.Value;
            ScrollBar.Value -= e.WheelDelta / 120f * ItemExtent * 3;
            args.Handled = old != ScrollBar.Value;
            return;
        }

        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            _pressed = GetItemIndexAt(e.Position);
            if (e.PointerKind != ControlPointerKind.Touch || !IsSwipeEnabled)
                Choose(_pressed, e.Shift, e.Control);
            context.CapturePointer(this);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && (_pressed >= 0 || Workspace?.GetCapturedControl(e.Pointer) == this))
        {
            int hit = GetItemIndexAt(e.Position), pressed = _pressed;
            _pressed = -1;
            if (pressed >= 0 && hit == pressed && e.PointerKind == ControlPointerKind.Touch && IsSwipeEnabled)
                Choose(hit, e.Shift, e.Control);
            context.ReleasePointer(this);
            args.Handled = true;
            if (pressed >= 0 && hit == pressed && hit == SelectedIndex && IsAvailable())
            {
                IsKeyboardActivation = false;
                ItemActivated?.Invoke(this, EventArgs.Empty);
                if (_lastClick == hit && _time - _clickTime <= .35)
                {
                    _lastClick = -1;
                    ItemDoubleClicked?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _lastClick = hit;
                    _clickTime = _time;
                }
            }

            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key == Keys.Enter)
        {
            IsKeyboardActivation = true;
            try
            {
                ItemActivated?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                IsKeyboardActivation = false;
            }

            args.Handled = true;
            return;
        }

        int direction = e.Key is Keys.Up or Keys.Left or Keys.PageUp ? -1 : 1;
        int index = SelectedIndex;
        switch (e.Key)
        {
            case Keys.Up:
            case Keys.Down:
                index = index < 0 ? (direction > 0 ? 0 : Items.Count - 1) : index + direction * _columns;
                break;
            case Keys.Left:
            case Keys.Right:
                if (_columns == 1)
                    return;
                index = index < 0 ? 0 : index + direction;
                break;
            case Keys.PageUp:
            case Keys.PageDown:
                index += direction * Math.Max(1, (int)(_viewport.Height / (ItemExtent + ItemSpacing)) * _columns);
                break;
            case Keys.Home:
                index = 0;
                direction = 1;
                break;
            case Keys.End:
                index = Items.Count - 1;
                direction = -1;
                break;
            default:
                return;
        }

        index = Math.Clamp(index, 0, Math.Max(0, Items.Count - 1));
        while (index >= 0 && index < Items.Count && !Items[index].IsEnabled)
            index += direction;
        Choose(index, e.Shift, e.Control);
        args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => CancelSwipe();

    /// <inheritdoc/>
    protected override void OnPreviewInput(ControlInputContext context, ControlInputEventArgs args)
    {
        ControlInputEvent input = args.Input;
        if (input.Type == ControlInputEventType.PointerCancel && _trackingSwipe && input.Pointer == _swipePointer)
        {
            CancelSwipe();
            return;
        }

        if (!IsSwipeEnabled || input.PointerKind != ControlPointerKind.Touch)
            return;
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
        {
            if (_trackingSwipe && !ScrollViewer.HasSwipeCapture(this, _swipePointer))
                CancelSwipe();
            if (_trackingSwipe || !Contains(_viewport, input.Position.X, input.Position.Y) || ScrollViewer.IsDirectManipulationTarget(args.Source, this))
                return;
            _trackingSwipe = true;
            _swipeMoved = _yieldedSwipe = false;
            _swipePointer = input.Pointer;
            _swipeStart = _swipePrevious = input.Position;
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
                _swipeMoved = true;
                _pressed = _lastClick = -1;
                _ensureSelection = false;
                Vector2 movement = _yieldedSwipe ? incremental : total;
                for (Control? node = args.Source; node is not null && node != this; node = node.Parent)
                {
                    if (node is ScrollViewer viewer && viewer.CanConsumeSwipe(input.Pointer, movement) || node is ListBox list && list.CanConsumeSwipe(input.Pointer, movement))
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

            Pan(incremental.Y);
            args.Handled = true;
        }
        else if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            if (_isSwiping)
            {
                Pan(input.Position.Y - _swipePrevious.Y);
                CancelSwipe();
                args.Handled = true;
            }
            else if (_swipeMoved)
            {
                context.CapturePointer(this);
                CancelSwipe();
                args.Handled = true;
            }
            else
            {
                // Preserve normal child release handling; a direct list tap chooses its item in OnInput.
                _trackingSwipe = false;
                _yieldedSwipe = false;
            }
        }
    }

    /// <summary>Reports whether this pending list gesture can consume the proposed vertical movement.</summary>
    /// <param name = "pointer">The pointer moving the content.</param>
    /// <param name = "movement">The dominant-axis displacement in logical workspace units.</param>
    /// <returns>True when the same pointer can scroll the list further in that direction.</returns>
    internal bool CanConsumeSwipe(ControlPointer pointer, Vector2 movement) => _trackingSwipe && pointer == _swipePointer && IsSwipeEnabled && CanScrollInDirection(movement);

    /// <summary>Tests whether the list can move opposite to a predominantly vertical finger displacement.</summary>
    /// <param name = "movement">The signed logical finger movement.</param>
    /// <returns>True when the scrollbar has remaining range in the proposed direction.</returns>
    private bool CanScrollInDirection(Vector2 movement) => MathF.Abs(movement.Y) >= MathF.Abs(movement.X) && (movement.Y < 0 ? ScrollBar.Value < ScrollBar.Maximum : movement.Y > 0 && ScrollBar.Value > ScrollBar.Minimum);

    /// <summary>Scrolls opposite to the finger while retaining the existing scrollbar's clamping behavior.</summary>
    /// <param name = "movement">The incremental signed vertical displacement.</param>
    private void Pan(float movement) => ScrollBar.Value = (float)Math.Clamp((double)ScrollBar.Value - movement, ScrollBar.Minimum, ScrollBar.Maximum);

    /// <summary>Clears tap/scroll state and releases capture only when it still belongs to this list.</summary>
    private void CancelSwipe()
    {
        _trackingSwipe = _swipeMoved = _yieldedSwipe = false;
        _pressed = -1;
        SetNotify(ref _isSwiping, false, IsSwipingArgs);
        if (Workspace?.GetCapturedControl(_swipePointer) == this)
            Workspace.ReleasePointer(this, _swipePointer);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (_font is null)
            return;
        var clip = new Bounds2(_viewport.X - Bounds.X, _viewport.Y - Bounds.Y, _viewport.Width, _viewport.Height);
        using (context.PushClipScope(clip))
        {
            GetVisibleRange(out int first, out int last);
            for (int i = first; i < last; i++)
            {
                var item = Items[i];
                Bounds2 world = GetItemBounds(i);
                var row = new Bounds2(world.X - Bounds.X, world.Y - Bounds.Y, world.Width, world.Height);
                bool hover = !IsSwiping && Workspace is not null && Contains(world, Workspace.PointerPosition.X, Workspace.PointerPosition.Y) && IsPointerOver;
                bool selected = _selected.Contains(i) || i == SelectedIndex;
                string state = !item.IsEnabled ? "Disabled" : selected ? (hover ? "Selected.Hover" : "Selected") : hover ? "Hover" : "Normal";
                if (!context.DrawSkinPart(this, "Item", row, state) && selected)
                    context.FillRectangle(row, new Color(33, 70, 84));
                DrawItem(context, i, row, _font);
            }
        }
    }

    /// <summary>
    /// Draws one visible item inside its supplied local bounds.
    /// </summary>
    /// <param name = "context">The active clipped drawing context.</param>
    /// <param name = "index">The source item index.</param>
    /// <param name = "row">The item bounds relative to this list.</param>
    /// <param name = "font">The resolved item font.</param>
    protected virtual void DrawItem(ControlDrawContext context, int index, Bounds2 row, IControlFont font)
    {
        SelectorItem item = Items[index];
        float x = row.X + 8;
        if (item.Icon is string icon)
        {
            context.DrawSkinRegion(icon, new Bounds2(x, row.Y + (row.Height - 20) / 2, 20, 20));
            x += 28;
        }

        Size2 text = font.Measure(item.Text);
        context.DrawText(font, item.Text, new Vector2(x, row.Y + (row.Height - text.Height) / 2), item.IsEnabled && IsAvailable() ? ConfigTextColor : ConfigDisabledTextColor);
    }

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    /// <param name = "style">The explicit style values to apply.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(IsSwipeEnabled):
                    if (_styleIsSwipeEnabled.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsSwipeEnabledArgs);
                    break;
                case nameof(SwipeThreshold):
                    float threshold = Converters.Convert<float>("Single", entry.Value);
                    if (!float.IsFinite(threshold) || threshold <= 0)
                        throw new ArgumentOutOfRangeException(nameof(style));
                    if (_styleSwipeThreshold.Apply(threshold))
                        NotifyPropertyChanged(SwipeThresholdArgs);
                    break;
                case nameof(ScrollBarThickness):
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case nameof(ItemHeight):
                    if (_styleItemHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ItemHeightChangedArgs);
                    break;
                case nameof(MultiSelect):
                    if (_styleMultiSelect.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(MultiSelectChangedArgs);
                    break;
                case nameof(SelectedIndex):
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
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
            case nameof(SwipeThreshold):
                return _styleSwipeThreshold.HasLocalValue;
            case nameof(ScrollBarThickness):
                return _styleScrollBarThickness.HasLocalValue;
            case nameof(ItemHeight):
                return _styleItemHeight.HasLocalValue;
            case nameof(MultiSelect):
                return _styleMultiSelect.HasLocalValue;
            case nameof(SelectedIndex):
                return _styleSelectedIndex.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    /// <param name = "property">The property whose local override is removed.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(IsSwipeEnabled):
                if (_styleIsSwipeEnabled.ClearLocal())
                    NotifyPropertyChanged(IsSwipeEnabledArgs);
                return;
            case nameof(SwipeThreshold):
                if (_styleSwipeThreshold.ClearLocal())
                    NotifyPropertyChanged(SwipeThresholdArgs);
                return;
            case nameof(ScrollBarThickness):
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case nameof(ItemHeight):
                if (_styleItemHeight.ClearLocal())
                    NotifyPropertyChanged(ItemHeightChangedArgs);
                return;
            case nameof(MultiSelect):
                if (_styleMultiSelect.ClearLocal())
                    NotifyPropertyChanged(MultiSelectChangedArgs);
                return;
            case nameof(SelectedIndex):
                if (_styleSelectedIndex.ClearLocal())
                    NotifyPropertyChanged(SelectedIndexChangedArgs);
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
        if (_styleIsSwipeEnabled.BeginStyle(true))
            NotifyPropertyChanged(IsSwipeEnabledArgs);
        if (_styleSwipeThreshold.BeginStyle(12))
            NotifyPropertyChanged(SwipeThresholdArgs);
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleItemHeight.BeginStyle(30f))
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleMultiSelect.BeginStyle(false))
            NotifyPropertyChanged(MultiSelectChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsSwipeEnabled.CommitStyle())
            NotifyPropertyChanged(IsSwipeEnabledArgs);
        if (_styleSwipeThreshold.CommitStyle())
            NotifyPropertyChanged(SwipeThresholdArgs);
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleItemHeight.CommitStyle())
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleMultiSelect.CommitStyle())
            NotifyPropertyChanged(MultiSelectChangedArgs);
        if (_styleSelectedIndex.CommitStyle())
            NotifyPropertyChanged(SelectedIndexChangedArgs);
    }
}

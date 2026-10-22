using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>A clipped, keyboard-accessible selectable list with an independent scrollbar.</summary>
public class ListBox : ContainerControl
{
    private readonly SortedSet<int> _selected = new();
    private readonly Predicate<int> _invalidSelection;
    private bool _selectionDirty = true;
    private int _lastPrimary = -1, _anchor = -1, _pressed = -1, _lastClick = -1;
    private double _time, _clickTime;
    private IControlFont? _font;
    private Bounds2 _viewport;
    private bool _showBar, _ensureSelection;
    private int _columns = 1;
    /// <summary>Gets the arranged item viewport in workspace coordinates.</summary>
    protected Bounds2 ItemViewport => _viewport;
    /// <summary>Gets the arranged number of item columns.</summary>
    protected int ItemColumns => _columns;
    /// <summary>Gets the spacing between rows and columns.</summary>
    protected virtual float ItemSpacing => 0;
    /// <summary>Gets the effective item height.</summary>
    protected virtual float ItemExtent => ItemHeight;

    /// <summary>Computes the number of columns for an available width.</summary>
    protected virtual int GetColumnCount(float width) => 1;
    /// <summary>Computes an item's width inside the viewport.</summary>
    protected virtual float GetItemWidth(float width) => width;
    /// <summary>Measures the preferred list width without creating item controls.</summary>
    protected virtual float MeasureItems(IControlFont font)
    {
        float width = 120;
        foreach (SelectorItem item in Items)
        {
            ArgumentNullException.ThrowIfNull(item);
            width = MathF.Max(width, font.Measure(item.Text).Width + 32);
        }

        return width;
    }

    private float ContentHeight(int columns) => Items.Count == 0 ? 0 : ((Items.Count - 1) / columns + 1) * (ItemExtent + ItemSpacing) - ItemSpacing;
    /// <summary>Gets the visible item interval, including partially visible rows.</summary>
    protected void GetVisibleRange(out int first, out int last)
    {
        first = Math.Max(0, (int)(ScrollBar.Value / (ItemExtent + ItemSpacing))) * _columns;
        last = Math.Min(Items.Count, first + ((int)(_viewport.Height / (ItemExtent + ItemSpacing)) + 2) * _columns);
    }

    /// <summary>Gets an item's bounds in workspace coordinates.</summary>
    public Bounds2 GetItemBounds(int index)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        float width = GetItemWidth(_viewport.Width);
        return new Bounds2(_viewport.X + index % _columns * (width + ItemSpacing), _viewport.Y + index / _columns * (ItemExtent + ItemSpacing) - ScrollBar.Value, width, ItemExtent);
    }

    /// <summary>Initializes a new list box instance.</summary>
    public ListBox() : base()
    {
        _invalidSelection = IsInvalidSelection;
        ScrollBar = new ScrollBar
        {
            Orientation = ControlOrientation.Vertical
        };
        base.Add(ScrollBar);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ListBox";
    /// <summary>Gets items.</summary>
    public IList<SelectorItem> Items => ConfigItems;
    /// <summary>Gets scroll bar.</summary>
    public ScrollBar ScrollBar { get; }
    /// <summary>Gets or sets item height.</summary>
    public float ItemHeight { get => ConfigItemHeight; set => ConfigItemHeight = value; }
    /// <summary>Gets or sets multi select.</summary>
    public bool MultiSelect { get => ConfigMultiSelect; set => ConfigMultiSelect = value; }

    /// <summary>Gets or sets selected index.</summary>
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

    /// <summary>Gets selected item.</summary>
    public SelectorItem? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];
    /// <summary>Gets selected indices.</summary>
    public IReadOnlyList<int> SelectedIndices => new ReadOnlyCollection<int>(_selected.ToList());

    /// <summary>Occurs when selection changed.</summary>
    public event EventHandler? SelectionChanged;
    /// <summary>Occurs when item activated.</summary>
    public event EventHandler? ItemActivated;
    /// <summary>Identifies keyboard activation during the ItemActivated callback.</summary>
    public bool IsKeyboardActivation { get; private set; }

    /// <summary>Occurs when item double clicked.</summary>
    public event EventHandler? ItemDoubleClicked;
    private void Notify()
    {
        int current = SelectedIndex;
        if (_lastPrimary == current)
            return;
        _lastPrimary = current;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureVisible(int index)
    {
        if (index < 0)
            return;
        float top = index / _columns * (ItemExtent + ItemSpacing), bottom = top + ItemExtent;
        if (top < ScrollBar.Value)
            ScrollBar.Value = top;
        else if (bottom > ScrollBar.Value + _viewport.Height)
            ScrollBar.Value = bottom - _viewport.Height;
    }

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

    private ControlLayoutContext? _layout;
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

    internal override Bounds2 GetChildClip(Control child) => base.ContentBounds;
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
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
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName is "Items" or nameof(MultiSelect) or nameof(SelectedIndex))
            _selectionDirty = true;
        base.OnPropertyChanged(notification);
    }

    private bool IsInvalidSelection(int index) => index >= Items.Count || !Items[index].IsEnabled;
    private int IndexAt(Vector2 point)
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
        if (e.Type == ControlInputEventType.PointerWheel)
        {
            float old = ScrollBar.Value;
            ScrollBar.Value -= e.WheelDelta / 120f * ItemExtent * 3;
            args.Handled = old != ScrollBar.Value;
            return;
        }

        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            _pressed = IndexAt(e.Position);
            Choose(_pressed, e.Shift, e.Control);
            context.CapturePointer(this);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && _pressed >= 0)
        {
            int hit = IndexAt(e.Position), pressed = _pressed;
            _pressed = -1;
            context.ReleasePointer(this);
            args.Handled = true;
            if (hit == pressed && hit == SelectedIndex)
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
    protected override void OnInteractionCancelled() => _pressed = -1;
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
                bool hover = Workspace is not null && Contains(world, Workspace.PointerPosition.X, Workspace.PointerPosition.Y) && IsPointerOver;
                bool selected = _selected.Contains(i) || i == SelectedIndex;
                string state = !item.IsEnabled ? "Disabled" : selected ? (hover ? "Selected.Hover" : "Selected") : hover ? "Hover" : "Normal";
                if (!context.DrawSkinPart(this, "Item", row, state) && selected)
                    context.FillRectangle(row, new Color(33, 70, 84));
                DrawItem(context, i, row, _font);
            }
        }
    }

    /// <summary>Draws one visible item inside its supplied local bounds.</summary>
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

    private StyleProperty<float> _styleScrollBarThickness = new(20f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));
    internal float ConfigScrollBarThickness
    {
        get => _styleScrollBarThickness.Value;
        set
        {
            if (_styleScrollBarThickness.SetLocal(value))
                NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        }
    }

    /// <summary>Gets or sets ScrollBarThickness; a local assignment overrides the style.</summary>
    public float ScrollBarThickness { get => ConfigScrollBarThickness; set => ConfigScrollBarThickness = value; }

    private StyleProperty<float> _styleItemHeight = new(30f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemHeightChangedArgs = new(nameof(ItemHeight));
    internal float ConfigItemHeight
    {
        get => _styleItemHeight.Value;
        set
        {
            if (_styleItemHeight.SetLocal(value))
                NotifyPropertyChanged(ItemHeightChangedArgs);
        }
    }

    private StyleProperty<bool> _styleMultiSelect = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MultiSelectChangedArgs = new(nameof(MultiSelect));
    internal bool ConfigMultiSelect
    {
        get => _styleMultiSelect.Value;
        set
        {
            if (_styleMultiSelect.SetLocal(value))
                NotifyPropertyChanged(MultiSelectChangedArgs);
        }
    }

    private StyleProperty<int> _styleSelectedIndex = new(-1);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedIndexChangedArgs = new(nameof(SelectedIndex));
    internal int ConfigSelectedIndex
    {
        get => _styleSelectedIndex.Value;
        set
        {
            if (_styleSelectedIndex.SetLocal(value))
                NotifyPropertyChanged(SelectedIndexChangedArgs);
        }
    }

    private ElementCollection<SelectorItem>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<SelectorItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "ScrollBarThickness":
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case "ItemHeight":
                    if (_styleItemHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ItemHeightChangedArgs);
                    break;
                case "MultiSelect":
                    if (_styleMultiSelect.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(MultiSelectChangedArgs);
                    break;
                case "SelectedIndex":
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ScrollBarThickness":
                return _styleScrollBarThickness.HasLocalValue;
            case "ItemHeight":
                return _styleItemHeight.HasLocalValue;
            case "MultiSelect":
                return _styleMultiSelect.HasLocalValue;
            case "SelectedIndex":
                return _styleSelectedIndex.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "ScrollBarThickness":
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case "ItemHeight":
                if (_styleItemHeight.ClearLocal())
                    NotifyPropertyChanged(ItemHeightChangedArgs);
                return;
            case "MultiSelect":
                if (_styleMultiSelect.ClearLocal())
                    NotifyPropertyChanged(MultiSelectChangedArgs);
                return;
            case "SelectedIndex":
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
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleItemHeight.BeginStyle(30f))
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleMultiSelect.BeginStyle(false))
            NotifyPropertyChanged(MultiSelectChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
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

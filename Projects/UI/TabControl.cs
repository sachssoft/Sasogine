using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>Arbitrary page/header content, keyboard navigation and horizontally scrolling headers.</summary>
public class TabControl : ContainerControl
{
    private readonly StackPanel _headers = new()
    {
        Orientation = StackOrientation.Horizontal,
        Spacing = 4,
        Background = Color.Transparent
    };
    private readonly ScrollViewer _headerView = new()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
    };
    private readonly List<TabItem> _known = new();
    private readonly List<ToggleButton> _buttons = new();
    private readonly List<TextBlock> _captions = new();
    private TabItem? _active;
    private ChangeTracker<int> _selection;
    private ChangeTracker<float> _headerHeight;
    private float _actualHeaderHeight;
    private Bounds2 _body;
    /// <summary>Creates a tab host.</summary>
    public TabControl() : base()
    {
        _headerView.Content = _headers;
        base.Add(_headerView);
    }

    /// <summary>Gets the retained pages.</summary>
    public IList<TabItem> Items => ConfigItems;

    /// <summary>Gets or sets the selected source index; -1 leaves the content empty.</summary>
    public int SelectedIndex
    {
        get
        {
            int index = ConfigSelectedIndex;
            return index >= 0 && index < Items.Count ? index : -1;
        }

        set
        {
            if (value < -1 || value >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (value >= 0 && !Items[value].IsEnabled)
                return;
            ConfigSelectedIndex = value;
        }
    }

    /// <summary>Gets the selected page.</summary>
    public TabItem? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];
    /// <summary>Gets the independent header scroll viewer.</summary>
    public ScrollViewer HeaderScrollViewer => _headerView;

    /// <summary>Occurs after selected content has changed at a layout boundary.</summary>
    public event EventHandler? SelectionChanged;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "TabControl";

    internal override Bounds2 GetChildClip(Control child) => child == _headerView ? ContentBounds : _body;
    internal override void Prepare()
    {
        base.Prepare();
        bool rebuild = _known.Count != Items.Count;
        if (!rebuild)
            for (int i = 0; i < Items.Count; i++)
                if (!ReferenceEquals(_known[i], Items[i]))
                {
                    rebuild = true;
                    break;
                }

        if (rebuild)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                TabItem item = Items[i] ?? throw new InvalidOperationException("Null tab item.");
                if (item.TabOwner is not null && item.TabOwner != this)
                    throw new InvalidOperationException("Tab page belongs to another tab control.");
                if (item.Parent is not null && item != _active || item.Workspace is not null && item != _active)
                    throw new InvalidOperationException("Tab page already belongs to another tree.");
                for (int n = 0; n < i; n++)
                    if (ReferenceEquals(Items[n], item))
                        throw new InvalidOperationException("A tab page cannot occur twice.");
                for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
                    if (ReferenceEquals(ancestor, item))
                        throw new InvalidOperationException("Cyclic tab content.");
            }

            foreach (var old in _known)
                if (!Items.Contains(old))
                    old.TabOwner = null;
            foreach (var button in _buttons)
            {
                button.Content = null;
                button.Prepare();
                _headers.ReplaceOwnedChild(button, null);
            }

            _known.Clear();
            _buttons.Clear();
            _captions.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                int index = i;
                var item = Items[i];
                item.TabOwner = this;
                _known.Add(item);
                _captions.Add(new TextBlock());
                var header = new ToggleButton()
                {
                    StyleId = "TabControl.Header"
                };
                header.Clicked += (_, _) => SelectedIndex = index;
                _buttons.Add(header);
                _headers.ReplaceOwnedChild(null, header);
            }
        }

        foreach (var item in Items)
        {
            item.FactoryRegistry = Workspace?.Registry ?? FactoryRegistry;
            item.EnsureStyle();
        }

        int selected = ConfigSelectedIndex;
        if (selected < -1)
            throw new InvalidOperationException("Invalid tab selection.");
        if (selected >= Items.Count)
            selected = Items.Count - 1;
        if (selected >= 0 && !Items[selected].IsEnabled)
        {
            selected = -1;
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].IsEnabled)
                {
                    selected = i;
                    break;
                }
        }

        ConfigSelectedIndex = selected;
        TabItem? next = selected >= 0 ? Items[selected] : null;
        bool changed = !ReferenceEquals(next, _active);
        if (changed)
        {
            ReplaceOwnedChild(_active, next);
            _active = next;
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            var header = _buttons[i];
            var item = Items[i];
            _captions[i].Text = item.Header;
            header.Content = item.HeaderContent ?? _captions[i];
            header.IsChecked = i == selected;
            header.IsEnabled = item.IsEnabled;
            header.Height = ConfigHeaderHeight;
        }

        _selection.EnsureChange(selected);
        if (changed)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _headerHeight.EnsureChange(ConfigHeaderHeight);
        if (!float.IsFinite(_headerHeight.NewValue) || _headerHeight.NewValue < 1)
            throw new InvalidOperationException("Invalid tab header height.");
        _headers.Measure(context, new Size2(float.PositiveInfinity, _headerHeight.NewValue));
        bool overflow = _headers.DesiredSize.Width > available.Width;
        _actualHeaderHeight = _headerHeight.NewValue + (overflow ? ConfigScrollBarThickness : 0);
        _headerView.Measure(context, new Size2(available.Width, _actualHeaderHeight));
        _active?.Measure(context, new Size2(available.Width, MathF.Max(0, available.Height - _actualHeaderHeight)));
        return new Size2(MathF.Max(_headerView.DesiredSize.Width, _active?.DesiredSize.Width ?? 0), _actualHeaderHeight + (_active?.DesiredSize.Height ?? 0));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float h = MathF.Min(bounds.Height, _actualHeaderHeight);
        _headerView.Arrange(new Bounds2(bounds.X, bounds.Y, bounds.Width, h), true);
        _body = new Bounds2(bounds.X, bounds.Y + h, bounds.Width, MathF.Max(0, bounds.Height - h));
        _active?.Arrange(_body, true);
        int selected = SelectedIndex;
        if (selected >= 0 && selected < _buttons.Count)
        {
            Control header = _buttons[selected];
            float logical = header.Bounds.X - _headers.Bounds.X;
            if (logical < _headerView.HorizontalScrollBar.Value)
                _headerView.HorizontalScrollBar.Value = logical;
            else if (logical + header.Bounds.Width > _headerView.HorizontalScrollBar.Value + _headerView.ViewportBounds.Width)
                _headerView.HorizontalScrollBar.Value = logical + header.Bounds.Width - _headerView.ViewportBounds.Width;
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type != ControlInputEventType.KeyDown || Items.Count == 0)
            return;
        bool inHeader = Workspace?.FocusedControl is Control focus && _buttons.Contains(focus as ToggleButton ?? null!);
        int direction = e.Control && e.Key == Keys.Tab ? (e.Shift ? -1 : 1) : inHeader && e.Key == Keys.Left ? -1 : inHeader && e.Key == Keys.Right ? 1 : 0;
        if (direction == 0 && !(inHeader && e.Key is Keys.Home or Keys.End))
            return;
        int next = e.Key == Keys.Home ? 0 : e.Key == Keys.End ? Items.Count - 1 : SelectedIndex;
        for (int i = 0; i < Items.Count; i++)
        {
            if (direction != 0)
                next = (next + direction + Items.Count) % Items.Count;
            if (Items[next].IsEnabled)
                break;
        }

        if (next >= 0 && next < Items.Count && Items[next].IsEnabled)
        {
            SelectedIndex = next;
            if (inHeader)
                Workspace?.Focus(_buttons[next]);
        }

        args.Handled = true;
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

    private StyleProperty<float> _styleHeaderHeight = new(38f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderHeightChangedArgs = new(nameof(HeaderHeight));
    internal float ConfigHeaderHeight
    {
        get => _styleHeaderHeight.Value;
        set
        {
            if (_styleHeaderHeight.SetLocal(value))
                NotifyPropertyChanged(HeaderHeightChangedArgs);
        }
    }

    /// <summary>Gets or sets HeaderHeight; a local assignment overrides the style.</summary>
    public float HeaderHeight { get => ConfigHeaderHeight; set => ConfigHeaderHeight = value; }

    private ElementCollection<TabItem>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<TabItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

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
                case "SelectedIndex":
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
                    break;
                case "HeaderHeight":
                    if (_styleHeaderHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(HeaderHeightChangedArgs);
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
            case "SelectedIndex":
                return _styleSelectedIndex.HasLocalValue;
            case "HeaderHeight":
                return _styleHeaderHeight.HasLocalValue;
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
            case "SelectedIndex":
                if (_styleSelectedIndex.ClearLocal())
                    NotifyPropertyChanged(SelectedIndexChangedArgs);
                return;
            case "HeaderHeight":
                if (_styleHeaderHeight.ClearLocal())
                    NotifyPropertyChanged(HeaderHeightChangedArgs);
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
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleHeaderHeight.BeginStyle(38f))
            NotifyPropertyChanged(HeaderHeightChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleSelectedIndex.CommitStyle())
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleHeaderHeight.CommitStyle())
            NotifyPropertyChanged(HeaderHeightChangedArgs);
    }
}

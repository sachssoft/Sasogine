using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TabPlacement = Sachssoft.Engine.Controls.TabStripPlacement;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Presents arbitrary pages with a scrollable header strip on any edge and keyboard navigation.
/// </summary>
public class TabControl : ContainerControl
{
    /// <summary>Caches notifications for changes to natural header height.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderHeightChangedArgs = new(nameof(HeaderHeight));

    /// <summary>Caches notifications for changes to side header width.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderWidthArgs = new(nameof(HeaderWidth));

    /// <summary>Caches notifications for changes to header scrollbar thickness.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ScrollBarThicknessChangedArgs = new(nameof(ScrollBarThickness));

    /// <summary>Caches notifications for changes to the selected page index.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedIndexChangedArgs = new(nameof(SelectedIndex));

    /// <summary>Caches notifications when tab headers switch between natural and equal sizing.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs StretchHeadersChangedArgs = new(nameof(StretchHeaders));

    /// <summary>Caches notifications for changes to the header strip edge.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs TabStripPlacementArgs = new(nameof(TabStripPlacement));

    /// <summary>Caches page identities to rebuild header ownership only after item changes.</summary>
    private readonly List<TabItem> _known = new();

    /// <summary>Retains the generated captions used when pages have no custom header content.</summary>
    private readonly List<TextBlock> _captions = new();

    /// <summary>Retains the toggle buttons associated with the known pages.</summary>
    private readonly List<ToggleButton> _buttons = new();

    /// <summary>Clips and scrolls the retained header buttons independently of page content.</summary>
    private readonly ScrollViewer _headerView = new()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
    };

    /// <summary>Retains the natural or equally sized header stack.</summary>
    private readonly TabHeaderPanel _headers = new()
    {
        Orientation = StackOrientation.Horizontal,
        Spacing = 4,
        Background = Color.Transparent
    };

    /// <summary>Stores the page clipping bounds beside the header strip.</summary>
    private Bounds2 _body;

    /// <summary>Tracks the effective configured header height used by layout.</summary>
    private ChangeTracker<float> _headerHeight;

    /// <summary>Tracks the selected source index across preparation boundaries.</summary>
    private ChangeTracker<int> _selection;

    /// <summary>Retains the latest context for constrained child measurement during arrangement.</summary>
    private ControlLayoutContext? _layoutContext;

    /// <summary>Stores the styled header placement and its local override.</summary>
    private StyleProperty<TabPlacement> _styleTabStripPlacement = new(TabPlacement.Top);

    /// <summary>Stores the style value and local override for equal header sizing.</summary>
    private StyleProperty<bool> _styleStretchHeaders = new(false);

    /// <summary>Stores the styled height of horizontal strips and natural side headers.</summary>
    private StyleProperty<float> _styleHeaderHeight = new(38f);

    /// <summary>Stores the styled width of side header strips and its local override.</summary>
    private StyleProperty<float> _styleHeaderWidth = new(160f);

    /// <summary>Stores the styled thickness of natural-layout header scrollbars.</summary>
    private StyleProperty<float> _styleScrollBarThickness = new(20f);

    /// <summary>Stores the selected page index and its local override.</summary>
    private StyleProperty<int> _styleSelectedIndex = new(-1);

    /// <summary>Tracks the page currently attached to layout and input traversal.</summary>
    private TabItem? _active;

    /// <summary>Lazily creates the collection that reserves page ownership.</summary>
    private TabItemCollection? _ownedItems;

    /// <summary>Stores the cross-axis strip size, including natural-layout scrollbar space.</summary>
    private float _actualHeaderExtent;

    /// <summary>
    /// Occurs after selected content has changed at a layout boundary.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a tab host.
    /// </summary>
    public TabControl() : base()
    {
        _headerView.Content = _headers;
        base.Add(_headerView);
    }

    /// <summary>
    /// Gets the retained pages.
    /// </summary>
    public IList<TabItem> Items => ConfigItems;

    /// <summary>
    /// Gets or sets the selected source index; -1 leaves the content empty.
    /// </summary>
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

    /// <summary>
    /// Gets the selected page.
    /// </summary>
    public TabItem? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];

    /// <summary>
    /// Gets the independent header scroll viewer.
    /// </summary>
    public ScrollViewer HeaderScrollViewer => _headerView;

    /// <summary>Gets or sets whether visible header buttons share the available strip length equally. Defaults to false.</summary>
    /// <remarks>Top and Bottom share width; Left and Right share height. False preserves natural sizing and overflow scrolling.
    /// Stretched captions are clipped when necessary. Unbounded measurement remains natural until a finite arrangement is available.</remarks>
    public bool StretchHeaders
    {
        get => _styleStretchHeaders.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleStretchHeaders.SetLocal(value))
                NotifyPropertyChanged(StretchHeadersChangedArgs);
        }
    }

    /// <summary>Gets or sets the edge containing the header strip. The default is Top; changing it preserves the selected page.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The value is not a supported edge.</exception>
    public TabPlacement TabStripPlacement
    {
        get => _styleTabStripPlacement.Value;
        set
        {
            VerifyTreeMutation();
            if (value is not (TabPlacement.Top or TabPlacement.Left or TabPlacement.Bottom or TabPlacement.Right))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleTabStripPlacement.SetLocal(value))
                NotifyPropertyChanged(TabStripPlacementArgs);
        }
    }

    /// <summary>Gets or sets the logical header width for Left and Right placement, excluding the scrollbar. Defaults to 160.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The width is not finite or is less than one.</exception>
    public float HeaderWidth
    {
        get => _styleHeaderWidth.Value;
        set
        {
            VerifyTreeMutation();
            if (!float.IsFinite(value) || value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleHeaderWidth.SetLocal(value))
                NotifyPropertyChanged(HeaderWidthArgs);
        }
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(TabControl);

    /// <summary>Gets whether the strip uses its vertical axis for header placement.</summary>
    private bool VerticalHeaders => TabStripPlacement is TabPlacement.Left or TabPlacement.Right;

    /// <summary>Gets or sets the effective natural-layout scrollbar thickness.</summary>
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

    /// <summary>Gets or sets the selected source index using the existing style storage.</summary>
    internal int ConfigSelectedIndex
    {
        get => _styleSelectedIndex.Value;
        set
        {
            if (_styleSelectedIndex.SetLocal(value))
                NotifyPropertyChanged(SelectedIndexChangedArgs);
        }
    }

    /// <summary>Gets or sets the natural header and horizontal-strip height.</summary>
    internal float ConfigHeaderHeight
    {
        get => _styleHeaderHeight.Value;
        set
        {
            if (_styleHeaderHeight.SetLocal(value))
                NotifyPropertyChanged(HeaderHeightChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the horizontal strip and natural side-header height. StretchHeaders overrides individual side-header heights.
    /// </summary>
    public float HeaderHeight
    {
        get => ConfigHeaderHeight;
        set => ConfigHeaderHeight = value;
    }

    /// <summary>Gets the lazily created ownership-aware page collection.</summary>
    internal IList<TabItem> ConfigItems => _ownedItems ??= new(this);

    /// <summary>Releases a removed page and its retained header from this host.</summary>
    /// <param name = "item">The page being removed.</param>
    internal void ReleaseTab(TabItem item)
    {
        _headers.Remove(item.HeaderButton);
        base.Remove(item);
    }

    /// <inheritdoc/>
    public override void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is not TabItem item)
            throw new ArgumentException("TabControl children must be TabItem pages.", nameof(child));
        Items.Add(item);
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child is TabItem item)
            Items.Remove(item);
    }

    /// <inheritdoc/>
    public override void Clear() => Items.Clear();

    /// <inheritdoc/>
    internal override Bounds2 GetChildClip(Control child) => child == _headerView ? ContentBounds : _body;

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        if (_headers.StretchHeaders != StretchHeaders)
        {
            _headers.StretchHeaders = StretchHeaders;
            _headerView.HorizontalScrollBar.Value = 0;
            _headerView.VerticalScrollBar.Value = 0;
        }

        StackOrientation orientation = VerticalHeaders ? StackOrientation.Vertical : StackOrientation.Horizontal;
        if (_headers.Orientation != orientation)
        {
            _headers.Orientation = orientation;
            _headerView.HorizontalScrollBar.Value = 0;
            _headerView.VerticalScrollBar.Value = 0;
        }

        _headerView.HorizontalScrollBarVisibility = VerticalHeaders || StretchHeaders ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
        _headerView.VerticalScrollBarVisibility = VerticalHeaders && !StretchHeaders ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        _headerView.ScrollBarThickness = ConfigScrollBarThickness;
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
            foreach (var button in _buttons)
                _headers.ReplaceOwnedChild(button, null);
            _known.Clear();
            _buttons.Clear();
            _captions.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                _known.Add(item);
                _captions.Add(item.HeaderCaption);
                var header = item.HeaderButton;
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
        ReplaceOwnedChild(_active, next, retainOwnership: true);
        _active = next;
        for (int i = 0; i < _buttons.Count; i++)
        {
            var header = _buttons[i];
            var item = Items[i];
            _captions[i].Text = item.Header;
            header.Content = item.HeaderContent ?? _captions[i];
            header.IsChecked = i == selected;
            header.IsEnabled = item.IsEnabled;
            header.Height = StretchHeaders && VerticalHeaders ? null : ConfigHeaderHeight;
        }

        _selection.EnsureChange(selected);
        if (changed)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _layoutContext = context;
        if (StretchHeaders)
            _headers.StretchExtent = VerticalHeaders ? available.Height : available.Width;
        _headerHeight.EnsureChange(ConfigHeaderHeight);
        if (!float.IsFinite(_headerHeight.NewValue) || _headerHeight.NewValue < 1)
            throw new InvalidOperationException("Invalid tab header height.");
        if (!float.IsFinite(HeaderWidth) || HeaderWidth < 1 || !float.IsFinite(ConfigScrollBarThickness) || ConfigScrollBarThickness <= 0 || TabStripPlacement is not (TabPlacement.Top or TabPlacement.Left or TabPlacement.Bottom or TabPlacement.Right))
            throw new InvalidOperationException("Invalid tab strip placement, width or scrollbar thickness.");
        if (VerticalHeaders)
        {
            _headers.Measure(context, new Size2(HeaderWidth, StretchHeaders ? available.Height : float.PositiveInfinity));
            bool verticalOverflow = _headers.DesiredSize.Height > available.Height;
            _actualHeaderExtent = HeaderWidth + (verticalOverflow ? ConfigScrollBarThickness : 0);
            _headerView.Measure(context, new Size2(_actualHeaderExtent, available.Height));
            _active?.Measure(context, new Size2(MathF.Max(0, available.Width - _actualHeaderExtent), available.Height));
            return new Size2(_actualHeaderExtent + (_active?.DesiredSize.Width ?? 0), MathF.Max(_headerView.DesiredSize.Height, _active?.DesiredSize.Height ?? 0));
        }

        _headers.Measure(context, new Size2(StretchHeaders ? available.Width : float.PositiveInfinity, _headerHeight.NewValue));
        bool overflow = _headers.DesiredSize.Width > available.Width;
        _actualHeaderExtent = _headerHeight.NewValue + (overflow ? ConfigScrollBarThickness : 0);
        _headerView.Measure(context, new Size2(available.Width, _actualHeaderExtent));
        _active?.Measure(context, new Size2(available.Width, MathF.Max(0, available.Height - _actualHeaderExtent)));
        return new Size2(MathF.Max(_headerView.DesiredSize.Width, _active?.DesiredSize.Width ?? 0), _actualHeaderExtent + (_active?.DesiredSize.Height ?? 0));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        if (StretchHeaders && _headers.SetArrangeExtent(VerticalHeaders ? bounds.Height : bounds.Width) && _layoutContext is not null)
            _headers.Measure(_layoutContext, VerticalHeaders ? new Size2(HeaderWidth, bounds.Height) : new Size2(bounds.Width, ConfigHeaderHeight));
        if (VerticalHeaders)
        {
            _actualHeaderExtent = HeaderWidth + (_headers.DesiredSize.Height > bounds.Height ? ConfigScrollBarThickness : 0);
            float width = MathF.Min(bounds.Width, _actualHeaderExtent);
            bool right = TabStripPlacement == TabPlacement.Right;
            if (_layoutContext is not null)
                _headerView.Measure(_layoutContext, new Size2(width, bounds.Height));
            _headerView.Arrange(new Bounds2(right ? bounds.Right - width : bounds.X, bounds.Y, width, bounds.Height), true);
            _body = new Bounds2(right ? bounds.X : bounds.X + width, bounds.Y, MathF.Max(0, bounds.Width - width), bounds.Height);
        }
        else
        {
            _actualHeaderExtent = ConfigHeaderHeight + (_headers.DesiredSize.Width > bounds.Width ? ConfigScrollBarThickness : 0);
            float height = MathF.Min(bounds.Height, _actualHeaderExtent);
            bool bottom = TabStripPlacement == TabPlacement.Bottom;
            if (_layoutContext is not null)
                _headerView.Measure(_layoutContext, new Size2(bounds.Width, height));
            _headerView.Arrange(new Bounds2(bounds.X, bottom ? bounds.Bottom - height : bounds.Y, bounds.Width, height), true);
            _body = new Bounds2(bounds.X, bottom ? bounds.Y : bounds.Y + height, bounds.Width, MathF.Max(0, bounds.Height - height));
        }

        if (_layoutContext is not null)
            _active?.Measure(_layoutContext, new Size2(_body.Width, _body.Height));
        _active?.Arrange(_body, true);
        int selected = SelectedIndex;
        if (selected >= 0 && selected < _buttons.Count)
        {
            Control header = _buttons[selected];
            ScrollBar bar = VerticalHeaders ? _headerView.VerticalScrollBar : _headerView.HorizontalScrollBar;
            float logical = VerticalHeaders ? header.Bounds.Y - _headers.Bounds.Y : header.Bounds.X - _headers.Bounds.X;
            float extent = VerticalHeaders ? header.Bounds.Height : header.Bounds.Width;
            float viewport = VerticalHeaders ? _headerView.ViewportBounds.Height : _headerView.ViewportBounds.Width;
            if (logical < bar.Value)
                bar.Value = logical;
            else if (logical + extent > bar.Value + viewport)
                bar.Value = logical + extent - viewport;
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type != ControlInputEventType.KeyDown || Items.Count == 0)
            return;
        bool inHeader = Workspace?.FocusedControl is Control focus && _buttons.Contains(focus as ToggleButton ?? null!);
        Keys previousKey = VerticalHeaders ? Keys.Up : Keys.Left;
        Keys nextKey = VerticalHeaders ? Keys.Down : Keys.Right;
        int direction = e.Control && e.Key == Keys.Tab ? (e.Shift ? -1 : 1) : inHeader && e.Key == previousKey ? -1 : inHeader && e.Key == nextKey ? 1 : 0;
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

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    /// <param name = "style">The resolved style values to apply without replacing local overrides.</param>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(StretchHeaders):
                    if (_styleStretchHeaders.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(StretchHeadersChangedArgs);
                    break;
                case nameof(TabStripPlacement):
                    if (_styleTabStripPlacement.Apply(Converters.Convert<TabPlacement>("TabStripPlacement", entry.Value)))
                        NotifyPropertyChanged(TabStripPlacementArgs);
                    break;
                case nameof(HeaderWidth):
                    if (_styleHeaderWidth.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(HeaderWidthArgs);
                    break;
                case nameof(ScrollBarThickness):
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case nameof(SelectedIndex):
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
                    break;
                case nameof(HeaderHeight):
                    if (_styleHeaderHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(HeaderHeightChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case nameof(StretchHeaders):
                return _styleStretchHeaders.HasLocalValue;
            case nameof(TabStripPlacement):
                return _styleTabStripPlacement.HasLocalValue;
            case nameof(HeaderWidth):
                return _styleHeaderWidth.HasLocalValue;
            case nameof(ScrollBarThickness):
                return _styleScrollBarThickness.HasLocalValue;
            case nameof(SelectedIndex):
                return _styleSelectedIndex.HasLocalValue;
            case nameof(HeaderHeight):
                return _styleHeaderHeight.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    /// <param name = "property">The style property whose local override is removed.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(StretchHeaders):
                if (_styleStretchHeaders.ClearLocal())
                    NotifyPropertyChanged(StretchHeadersChangedArgs);
                return;
            case nameof(TabStripPlacement):
                if (_styleTabStripPlacement.ClearLocal())
                    NotifyPropertyChanged(TabStripPlacementArgs);
                return;
            case nameof(HeaderWidth):
                if (_styleHeaderWidth.ClearLocal())
                    NotifyPropertyChanged(HeaderWidthArgs);
                return;
            case nameof(ScrollBarThickness):
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case nameof(SelectedIndex):
                if (_styleSelectedIndex.ClearLocal())
                    NotifyPropertyChanged(SelectedIndexChangedArgs);
                return;
            case nameof(HeaderHeight):
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
        if (_styleStretchHeaders.BeginStyle(false))
            NotifyPropertyChanged(StretchHeadersChangedArgs);
        if (_styleTabStripPlacement.BeginStyle(TabPlacement.Top))
            NotifyPropertyChanged(TabStripPlacementArgs);
        if (_styleHeaderWidth.BeginStyle(160f))
            NotifyPropertyChanged(HeaderWidthArgs);
        if (_styleScrollBarThickness.BeginStyle(20f))
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleHeaderHeight.BeginStyle(38f))
            NotifyPropertyChanged(HeaderHeightChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleStretchHeaders.CommitStyle())
            NotifyPropertyChanged(StretchHeadersChangedArgs);
        if (_styleTabStripPlacement.CommitStyle())
            NotifyPropertyChanged(TabStripPlacementArgs);
        if (_styleHeaderWidth.CommitStyle())
            NotifyPropertyChanged(HeaderWidthArgs);
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleSelectedIndex.CommitStyle())
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleHeaderHeight.CommitStyle())
            NotifyPropertyChanged(HeaderHeightChangedArgs);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Expandable tree with explicit item identities, two scrollbars, keyboard navigation and row activation.
/// </summary>
public class TreeView : ContainerControl
{
    private readonly List<TreeViewRow> _rows = new();
    private readonly HashSet<TreeViewItem> _seen = new();
    private readonly Stack<(TreeViewItem Item, TreeViewItem? Parent, int Depth, bool Visible, bool Enabled)> _walk = new();
    private ChangeTracker<TreeViewItem?> _selected;
    private ChangeTracker<float> _height, _indent;
    private Bounds2 _viewport;
    private float _extent;
    private bool _ensureVisible;
    private IControlFont? _font;
    /// <summary>
    /// Creates a tree.
    /// </summary>
    public TreeView() : base()
    {
        HorizontalScrollBar = new()
        {
            Axis = ControlOrientation.Horizontal
        };
        VerticalScrollBar = new()
        {
            Axis = ControlOrientation.Vertical
        };
        base.Add(HorizontalScrollBar);
        base.Add(VerticalScrollBar);
    }

    /// <summary>
    /// Gets root items.
    /// </summary>
    public IList<TreeViewItem> Items => ConfigItems;

    /// <summary>
    /// Gets or sets selection by item identity.
    /// </summary>
    public TreeViewItem? SelectedItem
    {
        get => ConfigSelectedItem;
        set
        {
            ConfigSelectedItem = value;
            _ensureVisible = true;
            Notify();
        }
    }

    /// <summary>
    /// Gets the count of visible rows.
    /// </summary>
    public int VisibleCount => _rows.Count;
    /// <summary>
    /// Gets the independent horizontal scrollbar.
    /// </summary>
    public ScrollBar HorizontalScrollBar { get; }
    /// <summary>
    /// Gets the independent vertical scrollbar.
    /// </summary>
    public ScrollBar VerticalScrollBar { get; }
    /// <summary>
    /// Gets the row viewport.
    /// </summary>
    public Bounds2 ViewportBounds => _viewport;

    /// <summary>
    /// Gets one currently visible item.
    /// </summary>
    public TreeViewItem GetVisibleItem(int index) => _rows[index].Item;
    /// <summary>
    /// Occurs after selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;
    /// <summary>
    /// Occurs after expansion changes through this control.
    /// </summary>
    public event EventHandler? ExpansionChanged;
    /// <summary>
    /// Occurs after an enabled row is activated.
    /// </summary>
    public event EventHandler? ItemActivated;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "TreeView";
    private float RowHeight => _height.NewValue;
    private float Indent => _indent.NewValue;

    private void Notify()
    {
        bool ready = _selected.WasEnsured;
        if (_selected.EnsureChange(SelectedItem))
        {
            _ensureVisible = true;
            if (ready)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Changes expansion and requests selection visibility.
    /// </summary>
    public void SetExpanded(TreeViewItem item, bool expanded)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item.IsExpanded == expanded)
            return;
        item.IsExpanded = expanded;
        _ensureVisible = true;
        ExpansionChanged?.Invoke(this, EventArgs.Empty);
    }

    internal override void Prepare()
    {
        base.Prepare();
        _height.EnsureChange(ConfigItemHeight);
        _indent.EnsureChange(ConfigIndent);
        if (!float.IsFinite(RowHeight) || RowHeight <= 0 || !float.IsFinite(Indent) || Indent < 0)
            throw new InvalidOperationException("Invalid tree row size.");
        _rows.Clear();
        _seen.Clear();
        _walk.Clear();
        for (int i = Items.Count - 1; i >= 0; i--)
            _walk.Push((Items[i], null, 0, true, true));
        while (_walk.Count > 0)
        {
            var entry = _walk.Pop();
            var item = entry.Item ?? throw new InvalidOperationException("Null tree item.");
            if (!_seen.Add(item))
                throw new InvalidOperationException("Tree items must have one owner and cannot form cycles.");
            bool enabled = entry.Enabled && item.IsEnabled;
            if (entry.Visible)
                _rows.Add(new TreeViewRow(item, entry.Parent, entry.Depth, enabled));
            for (int i = item.Items.Count - 1; i >= 0; i--)
                _walk.Push((item.Items[i], item, checked(entry.Depth + 1), entry.Visible && item.IsExpanded, enabled));
        }

        if (SelectedItem is not null)
        {
            int index = _rows.FindIndex(row => ReferenceEquals(row.Item, SelectedItem));
            if (index < 0)
            {
                TreeViewItem? current = SelectedItem;
                while (current is not null && !_rows.Exists(row => ReferenceEquals(row.Item, current)))
                {
                    TreeViewItem? parent = null;
                    foreach (var node in _seen)
                        if (node.Items.Contains(current))
                        {
                            parent = node;
                            break;
                        }

                    current = parent;
                }

                ConfigSelectedItem = current;
            }
            else if (!_rows[index].IsEnabled)
                ConfigSelectedItem = null;
            if (ConfigSelectedItem is TreeViewItem chosen && _rows.Exists(row => ReferenceEquals(row.Item, chosen) && !row.IsEnabled))
                ConfigSelectedItem = null;
        }

        Notify();
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _font = GetFont(context);
        _extent = 0;
        foreach (var row in _rows)
            _extent = MathF.Max(_extent, row.Depth * Indent + 28 + (row.Item.Icon is null ? 0 : 24) + _font.Measure(row.Item.Text).Width);
        HorizontalScrollBar.Measure(context, available);
        VerticalScrollBar.Measure(context, available);
        return new Size2(MathF.Min(available.Width, MathF.Max(160, _extent)), MathF.Min(available.Height, MathF.Max(RowHeight, Math.Min(8, _rows.Count) * RowHeight)));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float t = ConfigScrollBarThickness;
        if (!float.IsFinite(t) || t <= 0)
            throw new InvalidOperationException("Invalid scrollbar thickness.");
        bool horizontal = false, vertical = false;
        for (int i = 0; i < 3; i++)
        {
            horizontal = _extent > MathF.Max(0, b.Width - (vertical ? t : 0));
            vertical = _rows.Count * (double)RowHeight > MathF.Max(0, b.Height - (horizontal ? t : 0));
        }

        HorizontalScrollBar.IsVisible = horizontal;
        VerticalScrollBar.IsVisible = vertical;
        _viewport = new Bounds2(b.X, b.Y, MathF.Max(0, b.Width - (vertical ? t : 0)), MathF.Max(0, b.Height - (horizontal ? t : 0)));
        HorizontalScrollBar.Maximum = MathF.Max(0, _extent - _viewport.Width);
        HorizontalScrollBar.ViewportSize = _viewport.Width;
        HorizontalScrollBar.SmallChange = 24;
        HorizontalScrollBar.LargeChange = _viewport.Width;
        VerticalScrollBar.Maximum = (float)Math.Max(0, _rows.Count * (double)RowHeight - _viewport.Height);
        VerticalScrollBar.ViewportSize = _viewport.Height;
        VerticalScrollBar.SmallChange = RowHeight;
        VerticalScrollBar.LargeChange = _viewport.Height;
        HorizontalScrollBar.Arrange(new Bounds2(b.X, _viewport.Bottom, _viewport.Width, t), true);
        VerticalScrollBar.Arrange(new Bounds2(_viewport.Right, b.Y, t, _viewport.Height), true);
        if (_ensureVisible)
        {
            _ensureVisible = false;
            int row = _rows.FindIndex(r => ReferenceEquals(r.Item, SelectedItem));
            if (row >= 0)
            {
                float top = row * RowHeight;
                if (top < VerticalScrollBar.Value)
                    VerticalScrollBar.Value = top;
                else if (top + RowHeight > VerticalScrollBar.Value + _viewport.Height)
                    VerticalScrollBar.Value = top + RowHeight - _viewport.Height;
            }
        }
    }

    private void Activate()
    {
        if (SelectedItem is null)
            return;
        var command = ConfigCommand;
        var parameter = ConfigCommandParameter ?? SelectedItem;
        if (command is not null && !command.CanExecute(parameter))
            return;
        command?.Execute(parameter);
        ItemActivated?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerWheel)
        {
            var bar = e.Shift ? HorizontalScrollBar : VerticalScrollBar;
            float old = bar.Value;
            bar.Value -= e.WheelDelta / 120f * bar.SmallChange * 3;
            args.Handled = old != bar.Value;
            return;
        }

        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left && Contains(_viewport, e.Position.X, e.Position.Y))
        {
            int index = (int)((e.Position.Y - _viewport.Y + VerticalScrollBar.Value) / RowHeight);
            if (index >= 0 && index < _rows.Count && _rows[index].IsEnabled)
            {
                var row = _rows[index];
                float x = e.Position.X - _viewport.X + HorizontalScrollBar.Value - row.Depth * Indent;
                if (x < 22 && row.Item.Items.Count > 0)
                    SetExpanded(row.Item, !row.Item.IsExpanded);
                else
                    SelectedItem = row.Item;
            }

            args.Handled = true;
            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        int current = _rows.FindIndex(r => ReferenceEquals(r.Item, SelectedItem));
        if (e.Key is Keys.Enter or Keys.Space)
        {
            Activate();
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.Right && current >= 0)
        {
            var item = _rows[current].Item;
            if (item.Items.Count > 0 && !item.IsExpanded)
                SetExpanded(item, true);
            else if (current + 1 < _rows.Count && ReferenceEquals(_rows[current + 1].Parent, item))
                SelectedItem = _rows[current + 1].Item;
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.Left && current >= 0)
        {
            var row = _rows[current];
            if (row.Item.IsExpanded)
                SetExpanded(row.Item, false);
            else if (row.Parent is not null)
                SelectedItem = row.Parent;
            args.Handled = true;
            return;
        }

        int direction = e.Key is Keys.Up or Keys.Home or Keys.PageUp ? -1 : 1;
        int next = e.Key switch
        {
            Keys.Up => current - 1,
            Keys.Down => current + 1,
            Keys.Home => 0,
            Keys.End => _rows.Count - 1,
            Keys.PageUp => current - Math.Max(1, (int)(_viewport.Height / RowHeight)),
            Keys.PageDown => current + Math.Max(1, (int)(_viewport.Height / RowHeight)),
            _ => int.MinValue
        };
        if (next == int.MinValue || _rows.Count == 0)
            return;
        next = Math.Clamp(next, 0, _rows.Count - 1);
        if (e.Key == Keys.Home)
            direction = 1;
        if (e.Key == Keys.End)
            direction = -1;
        while (next >= 0 && next < _rows.Count && !_rows[next].IsEnabled)
            next += direction;
        if (next >= 0 && next < _rows.Count)
            SelectedItem = _rows[next].Item;
        args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        var font = GetFont(context);
        var clip = new Bounds2(_viewport.X - Bounds.X, _viewport.Y - Bounds.Y, _viewport.Width, _viewport.Height);
        using (context.PushClipScope(clip))
        {
            int start = Math.Max(0, (int)(VerticalScrollBar.Value / RowHeight)), end = Math.Min(_rows.Count, start + (int)(_viewport.Height / RowHeight) + 2);
            for (int i = start; i < end; i++)
            {
                var row = _rows[i];
                float y = clip.Y + i * RowHeight - VerticalScrollBar.Value;
                var box = new Bounds2(clip.X, y, clip.Width, RowHeight);
                bool selected = ReferenceEquals(row.Item, SelectedItem);
                if (selected)
                {
                    context.FillRectangle(box, new Color(33, 70, 84));
                    context.DrawSkinStylePart("ListBox", "Item", box, "Selected");
                }

                Color color = IsAvailable() && row.IsEnabled ? ConfigTextColor : ConfigDisabledTextColor;
                float x = clip.X + row.Depth * Indent - HorizontalScrollBar.Value;
                if (row.Item.Items.Count > 0)
                {
                    Vector2 center = new(x + 10, y + RowHeight / 2);
                    float radius = 4;
                    if (row.Item.IsExpanded)
                    {
                        context.DrawLine(center + new Vector2(-radius, -2), center + new Vector2(0, 3), color, 1.5f);
                        context.DrawLine(center + new Vector2(0, 3), center + new Vector2(radius, -2), color, 1.5f);
                    }
                    else
                    {
                        context.DrawLine(center + new Vector2(-2, -radius), center + new Vector2(3, 0), color, 1.5f);
                        context.DrawLine(center + new Vector2(3, 0), center + new Vector2(-2, radius), color, 1.5f);
                    }
                }

                x += 24;
                if (row.Item.Icon is string icon)
                {
                    context.DrawSkinRegion(icon, new Bounds2(x, y + (RowHeight - 20) / 2, 20, 20));
                    x += 24;
                }

                context.DrawText(font, row.Item.Text, new Vector2(x, y + (RowHeight - font.Measure(row.Item.Text).Height) / 2), color);
            }
        }
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

    /// <summary>
    /// Gets or sets ScrollBarThickness; a local assignment overrides the style.
    /// </summary>
    public  float ScrollBarThickness
    {
        get => ConfigScrollBarThickness;
        set => ConfigScrollBarThickness = value;
    }

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

    /// <summary>
    /// Gets or sets ItemHeight; a local assignment overrides the style.
    /// </summary>
    public float ItemHeight { get => ConfigItemHeight; set => ConfigItemHeight = value; }

    private StyleProperty<System.Windows.Input.ICommand?> _styleCommand = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandChangedArgs = new(nameof(Command));
    internal System.Windows.Input.ICommand? ConfigCommand
    {
        get => _styleCommand.Value;
        set
        {
            if (_styleCommand.SetLocal(value))
                NotifyPropertyChanged(CommandChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets Command; a local assignment overrides the style.
    /// </summary>
    public System.Windows.Input.ICommand? Command { get => ConfigCommand; set => ConfigCommand = value; }

    private StyleProperty<object?> _styleCommandParameter = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandParameterChangedArgs = new(nameof(CommandParameter));
    internal object? ConfigCommandParameter
    {
        get => _styleCommandParameter.Value;
        set
        {
            if (_styleCommandParameter.SetLocal(value))
                NotifyPropertyChanged(CommandParameterChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets CommandParameter; a local assignment overrides the style.
    /// </summary>
    public  object? CommandParameter
    {
        get => ConfigCommandParameter;
        set => ConfigCommandParameter = value;
    }

    private StyleProperty<float> _styleIndent = new(20f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IndentChangedArgs = new(nameof(Indent));
    internal float ConfigIndent
    {
        get => _styleIndent.Value;
        set
        {
            if (_styleIndent.SetLocal(value))
                NotifyPropertyChanged(IndentChangedArgs);
        }
    }

    private ElementCollection<TreeViewItem>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<TreeViewItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    private TreeViewItem? _storedSelectedItem = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedItemStorageArgs = new("SelectedItem");
    internal  TreeViewItem? ConfigSelectedItem
    {
        get => _storedSelectedItem;
        set => SetNotify(ref _storedSelectedItem, value, SelectedItemStorageArgs);
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
                case "ScrollBarThickness":
                    if (_styleScrollBarThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                    break;
                case "ItemHeight":
                    if (_styleItemHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ItemHeightChangedArgs);
                    break;
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<System.Windows.Input.ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;
                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
                    break;
                case "Indent":
                    if (_styleIndent.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(IndentChangedArgs);
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
            case "Command":
                return _styleCommand.HasLocalValue;
            case "CommandParameter":
                return _styleCommandParameter.HasLocalValue;
            case "Indent":
                return _styleIndent.HasLocalValue;
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
            case "ScrollBarThickness":
                if (_styleScrollBarThickness.ClearLocal())
                    NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
                return;
            case "ItemHeight":
                if (_styleItemHeight.ClearLocal())
                    NotifyPropertyChanged(ItemHeightChangedArgs);
                return;
            case "Command":
                if (_styleCommand.ClearLocal())
                    NotifyPropertyChanged(CommandChangedArgs);
                return;
            case "CommandParameter":
                if (_styleCommandParameter.ClearLocal())
                    NotifyPropertyChanged(CommandParameterChangedArgs);
                return;
            case "Indent":
                if (_styleIndent.ClearLocal())
                    NotifyPropertyChanged(IndentChangedArgs);
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
        if (_styleCommand.BeginStyle(null))
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.BeginStyle(null))
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleIndent.BeginStyle(20f))
            NotifyPropertyChanged(IndentChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleScrollBarThickness.CommitStyle())
            NotifyPropertyChanged(ScrollBarThicknessChangedArgs);
        if (_styleItemHeight.CommitStyle())
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleIndent.CommitStyle())
            NotifyPropertyChanged(IndentChangedArgs);
    }
}

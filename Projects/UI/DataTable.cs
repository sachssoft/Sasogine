using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;

namespace Sachssoft.Engine.Controls;
/// <summary>Virtualized text cells, stable explicit sorting, column resizing, row activation and optional inline editing.</summary>
public class DataTable : ContainerControl
{
    private readonly List<DataTableColumnLayout> _columns = new();
    private readonly List<object> _source = new();
    private readonly List<int> _order = new();
    private readonly SortedSet<int> _selectedRows = new();
    private readonly List<object> _selectedObjects = new();
    private ChangeTracker<int> _primarySelection;
    private int _selectionVersion, _notifiedSelectionVersion, _anchor = -1;
    /// <summary>Gets or sets whether Ctrl/Shift and Ctrl+A select multiple full rows.</summary>
    public bool MultiSelect { get => ConfigMultiSelect; set => ConfigMultiSelect = value; }
    /// <summary>Gets selected source row indices in source order.</summary>
    public IReadOnlyList<int> SelectedIndices => Array.AsReadOnly(_selectedRows.ToArray());
    /// <summary>Gets selected source objects in source order.</summary>
    public IReadOnlyList<object> SelectedItems => Array.AsReadOnly(_selectedRows.Where(i => i >= 0 && i < Items.Count).Select(i => Items[i]).ToArray());

    /// <summary>Selects all rows in multi-selection mode.</summary>
    public void SelectAll()
    {
        if (!MultiSelect)
            return;
        var indices = Enumerable.Range(0, Items.Count);
        SetRows(indices, SelectedIndex >= 0 ? SelectedIndex : Items.Count > 0 ? 0 : -1);
    }

    /// <summary>Formats selected full rows as tab-separated text, in display order.</summary>
    public string GetSelectedText()
    {
        var result = new StringBuilder();
        foreach (int source in _order)
        {
            if (!_selectedRows.Contains(source))
                continue;
            if (result.Length > 0)
                result.Append('\n');
            bool first = true;
            foreach (var column in Columns)
            {
                if (!column.IsVisible)
                    continue;
                if (!first)
                    result.Append('\t');
                first = false;
                string text = column.GetText(Items[source]);
                if (text.Contains('\t') || text.Contains('\n') || text.Contains('\r') || text.Contains('"'))
                    result.Append('"').Append(text.Replace("\"", "\"\"", StringComparison.Ordinal)).Append('"');
                else
                    result.Append(text);
            }
        }

        return result.ToString();
    }

    /// <summary>Clears the full-row selection.</summary>
    public void ClearSelection() => SetRows(Array.Empty<int>(), -1);
    private void SetRows(IEnumerable<int> rows, int primary)
    {
        var next = new SortedSet<int>(rows);
        if (!_selectedRows.SetEquals(next))
        {
            _selectedRows.Clear();
            _selectedRows.UnionWith(next);
            unchecked
            {
                _selectionVersion++;
            }
        }

        ConfigSelectedIndex = primary;
        _primarySelection.Set(primary);
        _ensureVisible = true;
        NotifySelection();
        EnsureSelectionVisible();
    }

    private void ChooseRow(int source, bool shift, bool control)
    {
        if (!MultiSelect || !shift && !control)
        {
            SelectedIndex = source;
            return;
        }

        if (shift && _anchor >= 0)
        {
            int from = _order.IndexOf(_anchor), to = _order.IndexOf(source);
            if (from < 0)
                from = to;
            var range = new List<int>();
            if (control)
                range.AddRange(_selectedRows);
            for (int i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                range.Add(_order[i]);
            SetRows(range, source);
        }
        else
        {
            var next = new SortedSet<int>(_selectedRows);
            if (!next.Add(source))
                next.Remove(source);
            _anchor = source;
            SetRows(next, next.Contains(source) ? source : next.Count > 0 ? next.Max : -1);
        }
    }

    private readonly Comparison<int> _comparison;
    private readonly TextBox _editor = new()
    {
        IsVisible = false,
        Padding = new Insets2(5, 3)
    };
    private ChangeTracker<int> _refresh;
    private ChangeTracker<float> _rowHeight, _headerHeight;
    private DataTableColumn? _sortColumn, _resizing, _editingColumn;
    private DataTableSortDirection _direction;
    private float _extentWidth, _resizeStartWidth, _resizeStartX;
    private Bounds2 _body, _header;
    private int _selectedColumn, _editRow = -1, _notifiedIndex = -1, _lastClick = -1, _revision;
    private bool _ensureVisible;
    private object? _notifiedItem;
    private double _time, _lastClickTime;
    private IControlFont? _font;
    /// <summary>Creates a table.</summary>
    public DataTable() : base()
    {
        _comparison = CompareRows;
        HorizontalScrollBar = new ScrollBar()
        {
            Axis = ControlOrientation.Horizontal
        };
        VerticalScrollBar = new ScrollBar()
        {
            Axis = ControlOrientation.Vertical
        };
        base.Add(HorizontalScrollBar);
        base.Add(VerticalScrollBar);
        base.Add(_editor);
        _editor.Submitted += (_, _) => CommitEdit();
    }

    /// <summary>Gets source rows.</summary>
    public IList<object> Items => ConfigItems;
    /// <summary>Gets explicit column propertiess.</summary>
    public IList<DataTableColumn> Columns => ConfigColumnDefinitions;
    /// <summary>Gets the independent horizontal scrollbar.</summary>
    public ScrollBar HorizontalScrollBar { get; }
    /// <summary>Gets the independent vertical scrollbar.</summary>
    public ScrollBar VerticalScrollBar { get; }
    /// <summary>Gets visible row viewport bounds.</summary>
    public Bounds2 ViewportBounds => _body;

    /// <summary>Gets or sets source row selection; sorting does not change source indices.</summary>
    public int SelectedIndex
    {
        get => ConfigSelectedIndex >= 0 && ConfigSelectedIndex < Items.Count ? ConfigSelectedIndex : -1;
        set
        {
            if (value < -1 || value >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(value));
            _anchor = value;
            SetRows(value >= 0 ? new[] { value } : Array.Empty<int>(), value);
        }
    }

    /// <summary>Gets the selected source item.</summary>
    public object? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];
    /// <summary>Gets the current sort column.</summary>
    public DataTableColumn? SortColumn => _sortColumn;
    /// <summary>Gets current sorting.</summary>
    public DataTableSortDirection SortDirection => _direction;
    /// <summary>Gets whether an inline editor is active.</summary>
    public bool IsEditing => _editRow >= 0;
    /// <summary>Gets the inline editor for validation and customization.</summary>
    public TextBox CellEditor => _editor;
    /// <summary>Gets the last expected conversion/validation error.</summary>
    public string EditError { get; private set; } = string.Empty;
    /// <summary>Gets or sets the command executed by Enter or double-click. Null parameter defaults to the selected row.</summary>
    public System.Windows.Input.ICommand? Command { get => ConfigCommand; set => ConfigCommand = value; }
    /// <summary>Gets or sets an explicit row activation parameter.</summary>
    public object? CommandParameter { get => ConfigCommandParameter; set => ConfigCommandParameter = value; }

    /// <summary>Occurs when selection changes.</summary>
    public event EventHandler? SelectionChanged;
    /// <summary>Occurs after an allowed row activation.</summary>
    public event EventHandler? RowActivated;
    /// <summary>Occurs after a cell is committed.</summary>
    public event EventHandler? CellEdited;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "DataTable";

    /// <summary>Invalidates sorted order after application row values have changed.</summary>
    public void Refresh()
    {
        unchecked
        {
            _revision++;
        }
    }

    /// <summary>Sorts through an explicit column comparison; equal values preserve source order.</summary>
    public void SortBy(DataTableColumn? column, DataTableSortDirection direction = DataTableSortDirection.Ascending)
    {
        if (direction is not (DataTableSortDirection.None or DataTableSortDirection.Ascending or DataTableSortDirection.Descending))
            throw new ArgumentOutOfRangeException(nameof(direction));
        if (column is not null && !Columns.Contains(column))
            throw new ArgumentException("Column is not part of this table.", nameof(column));
        if (IsEditing && !CommitEdit())
            return;
        _sortColumn = column;
        _direction = column is null ? DataTableSortDirection.None : direction;
        Refresh();
    }

    /// <summary>Gets an item in displayed, sorted order.</summary>
    public object GetDisplayItem(int displayIndex) => Items[_order[displayIndex]];
    private int CompareRows(int a, int b)
    {
        int result = _sortColumn!.Compare(_source[a], _source[b]);
        if (result == 0)
            return a.CompareTo(b);
        return _direction == DataTableSortDirection.Descending ? (result > 0 ? -1 : 1) : result;
    }

    private void ReconcileRows()
    {
        bool changed = _source.Count != Items.Count;
        if (!changed)
            for (int i = 0; i < Items.Count; i++)
                if (!ReferenceEquals(Items[i], _source[i]))
                {
                    changed = true;
                    break;
                }

        if (_sortColumn is not null && !Columns.Contains(_sortColumn))
        {
            _sortColumn = null;
            _direction = DataTableSortDirection.None;
            changed = true;
        }

        bool invalidated = _refresh.EnsureChange(_revision);
        if (!changed && !invalidated)
            return;
        if (changed && IsEditing)
            CancelEdit();
        bool remap = changed && _source.Count > 0;
        object? primaryObject = remap && SelectedIndex >= 0 && SelectedIndex < _source.Count ? _source[SelectedIndex] : null;
        object? anchorObject = remap && _anchor >= 0 && _anchor < _source.Count ? _source[_anchor] : null;
        _selectedObjects.Clear();
        if (remap)
            foreach (int index in _selectedRows)
                if (index < _source.Count)
                    _selectedObjects.Add(_source[index]);
        _source.Clear();
        _order.Clear();
        for (int i = 0; i < Items.Count; i++)
        {
            _source.Add(Items[i] ?? throw new InvalidOperationException("A table row cannot be null."));
            _order.Add(i);
        }

        if (_sortColumn is not null && _direction != DataTableSortDirection.None)
            _order.Sort(_comparison);
        if (remap)
        {
            var mapped = new List<int>();
            for (int i = 0; i < _source.Count; i++)
                if (_selectedObjects.Any(o => ReferenceEquals(o, _source[i])))
                    mapped.Add(i);
            int primary = primaryObject is null ? -1 : _source.FindIndex(o => ReferenceEquals(o, primaryObject));
            _anchor = anchorObject is null ? -1 : _source.FindIndex(o => ReferenceEquals(o, anchorObject));
            SetRows(mapped, primary >= 0 ? primary : mapped.Count > 0 ? mapped[0] : -1);
        }

        if (ConfigSelectedIndex >= Items.Count)
            ConfigSelectedIndex = Items.Count - 1;
        NotifySelection();
    }

    private float RowHeight
    {
        get
        {
            _rowHeight.EnsureChange(ConfigItemHeight);
            float value = _rowHeight.NewValue;
            if (!float.IsFinite(value) || value <= 0)
                throw new InvalidOperationException("Invalid table row height.");
            return value;
        }
    }

    private float HeaderHeight
    {
        get
        {
            _headerHeight.EnsureChange(ConfigHeaderHeight);
            float value = _headerHeight.NewValue;
            if (!float.IsFinite(value) || value < 0)
                throw new InvalidOperationException("Invalid table header height.");
            return ConfigShowHeaders ? value : 0;
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _font = ConfigFont ?? context.DefaultFont;
        ReconcileRows();
        NotifySelection();
        _columns.Clear();
        _extentWidth = 0;
        foreach (DataTableColumn column in Columns)
        {
            ArgumentNullException.ThrowIfNull(column);
            if (!column.IsVisible)
                continue;
            if (!float.IsFinite(column.Width) || column.Width < 0 || !float.IsFinite(column.MinWidth) || column.MinWidth < 0 || float.IsNaN(column.MaxWidth) || column.MaxWidth < column.MinWidth)
                throw new InvalidOperationException("Invalid table column width.");
            float width = Math.Clamp(column.Width, column.MinWidth, column.MaxWidth);
            _columns.Add(new DataTableColumnLayout(column, _extentWidth, width));
            _extentWidth += width;
        }

        HorizontalScrollBar.Measure(context, available);
        VerticalScrollBar.Measure(context, available);
        _editor.Measure(context, available);
        return new Size2(MathF.Min(available.Width, _extentWidth), MathF.Min(available.Height, HeaderHeight + MathF.Max(1, Math.Min(8, Items.Count)) * RowHeight));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float t = ConfigScrollBarThickness;
        if (!float.IsFinite(t) || t <= 0)
            throw new InvalidOperationException("Invalid scrollbar size.");
        bool horizontal = false, vertical = false;
        for (int i = 0; i < 3; i++)
        {
            horizontal = _extentWidth > MathF.Max(0, b.Width - (vertical ? t : 0));
            vertical = Items.Count * (double)RowHeight > MathF.Max(0, b.Height - HeaderHeight - (horizontal ? t : 0));
        }

        HorizontalScrollBar.IsVisible = horizontal;
        VerticalScrollBar.IsVisible = vertical;
        float w = MathF.Max(0, b.Width - (vertical ? t : 0)), h = MathF.Max(0, b.Height - HeaderHeight - (horizontal ? t : 0));
        _header = new Bounds2(b.X, b.Y, w, MathF.Min(b.Height, HeaderHeight));
        _body = new Bounds2(b.X, _header.Bottom, w, h);
        HorizontalScrollBar.Maximum = MathF.Max(0, _extentWidth - w);
        HorizontalScrollBar.ViewportSize = w;
        HorizontalScrollBar.SmallChange = 32;
        HorizontalScrollBar.LargeChange = w;
        VerticalScrollBar.Maximum = (float)Math.Max(0, Items.Count * (double)RowHeight - h);
        VerticalScrollBar.ViewportSize = h;
        VerticalScrollBar.SmallChange = RowHeight;
        VerticalScrollBar.LargeChange = h;
        HorizontalScrollBar.Arrange(new Bounds2(b.X, _body.Bottom, w, t), true);
        VerticalScrollBar.Arrange(new Bounds2(_body.Right, _body.Y, t, h), true);
        if (_ensureVisible)
        {
            _ensureVisible = false;
            EnsureSelectionVisible();
        }

        if (IsEditing)
        {
            int display = _order.IndexOf(_editRow);
            int column = _columns.FindIndex(c => ReferenceEquals(c.Column, _editingColumn));
            if (display < 0 || column < 0)
            {
                CancelEdit();
                return;
            }

            var c = _columns[column];
            _editor.Arrange(new Bounds2(_body.X + c.X - HorizontalScrollBar.Value, _body.Y + display * RowHeight - VerticalScrollBar.Value, c.Width, RowHeight), true);
        }
    }

    internal override Bounds2 GetChildClip(Control child) => child == _editor ? _body : ContentBounds;
    private void NotifySelection()
    {
        int index = SelectedIndex;
        if (_primarySelection.EnsureChange(index))
        {
            bool same = _selectedRows.Count == (index >= 0 ? 1 : 0) && (index < 0 || _selectedRows.Contains(index));
            _selectedRows.Clear();
            if (index >= 0)
                _selectedRows.Add(index);
            if (!same)
                unchecked
                {
                    _selectionVersion++;
                }

            _anchor = index;
        }

        if (!MultiSelect && (_selectedRows.Count > 1 || index >= 0 && !_selectedRows.Contains(index)))
        {
            _selectedRows.Clear();
            if (index >= 0)
                _selectedRows.Add(index);
            unchecked
            {
                _selectionVersion++;
            }
        }

        int removed = _selectedRows.RemoveWhere(i => i < 0 || i >= Items.Count);
        if (removed > 0)
            unchecked
            {
                _selectionVersion++;
            }

        object? item = SelectedItem;
        if (_notifiedIndex == index && ReferenceEquals(item, _notifiedItem) && _notifiedSelectionVersion == _selectionVersion)
            return;
        _notifiedIndex = index;
        _notifiedItem = item;
        _notifiedSelectionVersion = _selectionVersion;
        _ensureVisible = true;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureSelectionVisible()
    {
        int display = _order.IndexOf(SelectedIndex);
        if (display < 0)
            return;
        float top = display * RowHeight, bottom = top + RowHeight;
        if (top < VerticalScrollBar.Value)
            VerticalScrollBar.Value = top;
        else if (bottom > VerticalScrollBar.Value + _body.Height)
            VerticalScrollBar.Value = bottom - _body.Height;
        if (_selectedColumn >= 0 && _selectedColumn < _columns.Count)
        {
            var c = _columns[_selectedColumn];
            if (c.X < HorizontalScrollBar.Value)
                HorizontalScrollBar.Value = c.X;
            else if (c.X + c.Width > HorizontalScrollBar.Value + _body.Width)
                HorizontalScrollBar.Value = c.X + c.Width - _body.Width;
        }
    }

    /// <summary>Starts an explicit editable cell. Source index remains stable across sorting.</summary>
    public bool BeginEdit(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= Items.Count || columnIndex < 0 || columnIndex >= Columns.Count)
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        DataTableColumn column = Columns[columnIndex];
        if (!column.IsVisible || !column.CanEdit(Items[rowIndex]))
            return false;
        if (IsEditing && !CommitEdit())
            return false;
        _editRow = rowIndex;
        _editingColumn = column;
        SelectedIndex = rowIndex;
        _selectedColumn = _columns.FindIndex(c => ReferenceEquals(c.Column, column));
        EnsureSelectionVisible();
        _editor.Text = column.GetText(Items[rowIndex]);
        _editor.HasError = false;
        EditError = string.Empty;
        _editor.IsVisible = true;
        _editor.SelectAll();
        Workspace?.Focus(_editor);
        return true;
    }

    /// <summary>Commits through the column setter, retaining invalid text for correction.</summary>
    public bool CommitEdit()
    {
        if (!IsEditing)
            return true;
        try
        {
            _editingColumn!.SetText(Items[_editRow], _editor.Text);
        }
        catch (Exception error)when (error is FormatException or ArgumentException or OverflowException)
        {
            _editor.HasError = true;
            EditError = error.Message;
            return false;
        }

        CancelEdit();
        Refresh();
        CellEdited?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Discards the inline editor.</summary>
    public void CancelEdit()
    {
        bool focused = _editor.IsFocused;
        _editRow = -1;
        _editingColumn = null;
        _editor.IsVisible = false;
        _editor.HasError = false;
        EditError = string.Empty;
        if (focused && Workspace is not null)
            Workspace.Focus(this);
    }

    private void Activate()
    {
        if (SelectedItem is not object row)
            return;
        var command = Command;
        object? parameter = CommandParameter ?? row;
        if (command is not null && !command.CanExecute(parameter))
            return;
        command?.Execute(parameter);
        RowActivated?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        _time += elapsed.TotalSeconds;
        NotifySelection();
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => _resizing = null;
    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerMove && _resizing is DataTableColumn resize)
        {
            resize.Width = Math.Clamp(_resizeStartWidth + e.Position.X - _resizeStartX, resize.MinWidth, resize.MaxWidth);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerUp && _resizing is not null)
        {
            context.ReleasePointer(this);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            if (IsEditing && !CommitEdit())
            {
                args.Handled = true;
                return;
            }

            float local = e.Position.X - _body.X + HorizontalScrollBar.Value;
            if (Contains(_header, e.Position.X, e.Position.Y))
            {
                foreach (var c in _columns)
                {
                    if (ConfigCanUserResizeColumns && c.Column.CanResize && MathF.Abs(local - c.X - c.Width) <= 4)
                    {
                        _resizing = c.Column;
                        _resizeStartWidth = c.Width;
                        _resizeStartX = e.Position.X;
                        context.CapturePointer(this);
                        args.Handled = true;
                        return;
                    }

                    if (local >= c.X && local < c.X + c.Width && ConfigCanUserSortColumns && c.Column.CanSort)
                    {
                        var next = ReferenceEquals(_sortColumn, c.Column) && _direction == DataTableSortDirection.Ascending ? DataTableSortDirection.Descending : ReferenceEquals(_sortColumn, c.Column) && _direction == DataTableSortDirection.Descending ? DataTableSortDirection.None : DataTableSortDirection.Ascending;
                        SortBy(c.Column, next);
                        args.Handled = true;
                        return;
                    }
                }
            }
            else if (Contains(_body, e.Position.X, e.Position.Y))
            {
                int display = (int)((e.Position.Y - _body.Y + VerticalScrollBar.Value) / RowHeight);
                if (display >= 0 && display < _order.Count)
                {
                    ChooseRow(_order[display], e.Shift, e.Control);
                    for (int i = 0; i < _columns.Count; i++)
                        if (local >= _columns[i].X && local < _columns[i].X + _columns[i].Width)
                        {
                            _selectedColumn = i;
                            break;
                        }

                    if (SelectedIndex >= 0 && !e.Control && !e.Shift && _lastClick == SelectedIndex && _time - _lastClickTime <= .35)
                    {
                        int column = _selectedColumn < _columns.Count ? Columns.IndexOf(_columns[_selectedColumn].Column) : -1;
                        if (column < 0 || !BeginEdit(SelectedIndex, column))
                            Activate();
                        _lastClick = -1;
                    }
                    else
                    {
                        _lastClick = SelectedIndex;
                        _lastClickTime = _time;
                    }
                }

                args.Handled = true;
                return;
            }
        }

        if (e.Type == ControlInputEventType.PointerWheel)
        {
            if (IsEditing && !CommitEdit())
            {
                args.Handled = true;
                return;
            }

            var bar = e.Shift ? HorizontalScrollBar : VerticalScrollBar;
            float old = bar.Value;
            bar.Value -= e.WheelDelta / 120f * bar.SmallChange;
            args.Handled = old != bar.Value;
            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key == Keys.Escape && IsEditing)
        {
            CancelEdit();
            args.Handled = true;
            return;
        }

        if (IsEditing)
            return;
        if (e.Control && e.Key == Keys.A && MultiSelect)
        {
            SelectAll();
            args.Handled = true;
            return;
        }

        if (e.Control && e.Key == Keys.Space && SelectedIndex >= 0 && MultiSelect)
        {
            ChooseRow(SelectedIndex, false, true);
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.Enter)
        {
            Activate();
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.F2 && SelectedIndex >= 0 && _columns.Count > 0)
        {
            BeginEdit(SelectedIndex, Columns.IndexOf(_columns[Math.Clamp(_selectedColumn, 0, _columns.Count - 1)].Column));
            args.Handled = true;
            return;
        }

        if (e.Control && e.Key == Keys.C && SelectedItem is object selected && _columns.Count > 0 && Workspace?.Registry.Clipboard is { } clipboard)
        {
            clipboard.SetData("text/plain", MultiSelect ? GetSelectedText() : _columns[Math.Clamp(_selectedColumn, 0, _columns.Count - 1)].Column.GetText(selected));
            args.Handled = true;
            return;
        }

        int current = _order.IndexOf(SelectedIndex), page = Math.Max(1, (int)(_body.Height / RowHeight));
        int nextIndex = e.Key switch
        {
            Keys.Up => current - 1,
            Keys.Down => current + 1,
            Keys.Home => 0,
            Keys.End => _order.Count - 1,
            Keys.PageUp => current - page,
            Keys.PageDown => current + page,
            _ => int.MinValue
        };
        if (nextIndex != int.MinValue && _order.Count > 0)
        {
            ChooseRow(_order[Math.Clamp(nextIndex, 0, _order.Count - 1)], e.Shift, e.Control);
            args.Handled = true;
            return;
        }

        if (e.Key is Keys.Left or Keys.Right && _columns.Count > 0)
        {
            _selectedColumn = Math.Clamp(_selectedColumn + (e.Key == Keys.Left ? -1 : 1), 0, _columns.Count - 1);
            EnsureSelectionVisible();
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        IControlFont font = _font ?? context.DefaultFont;
        float rowHeight = RowHeight;
        Bounds2 Local(Bounds2 b) => new(b.X - Bounds.X, b.Y - Bounds.Y, b.Width, b.Height);
        if (_header.Height > 0)
        {
            using (context.PushClipScope(Local(_header)))
            {
                context.DrawSkinStylePart("MenuBar", "Background", Local(_header));
                foreach (var c in _columns)
                {
                    var b = new Bounds2(_header.X + c.X - HorizontalScrollBar.Value, _header.Y, c.Width, _header.Height);
                    if (b.Right < _header.X || b.X > _header.Right)
                        continue;
                    using (context.PushClipScope(Local(b)))
                    {
                        string caption = c.Column.Header + (ReferenceEquals(_sortColumn, c.Column) ? _direction == DataTableSortDirection.Ascending ? "  ▲" : _direction == DataTableSortDirection.Descending ? "  ▼" : "" : "");
                        context.DrawText(font, caption, new Vector2(b.X - Bounds.X + 7, b.Y - Bounds.Y + (b.Height - font.Measure(caption).Height) / 2), ConfigTextColor);
                    }

                    if (ConfigShowGridLines)
                        context.FillRectangle(new Bounds2(b.Right - Bounds.X - 1, b.Y - Bounds.Y, 1, b.Height), BorderColor);
                }
            }
        }

        using (context.PushClipScope(Local(_body)))
        {
            int start = Math.Max(0, (int)(VerticalScrollBar.Value / rowHeight)), end = Math.Min(_order.Count, start + (int)(_body.Height / rowHeight) + 2);
            for (int display = start; display < end; display++)
            {
                int source = _order[display];
                var row = new Bounds2(_body.X, _body.Y + display * rowHeight - VerticalScrollBar.Value, _body.Width, rowHeight);
                if (_selectedRows.Contains(source))
                {
                    context.FillRectangle(Local(row), new Color(35, 73, 91));
                    context.DrawSkinStylePart("ListBox", "Item", Local(row), IsFocused ? "Selected" : "Selected.Inactive");
                }

                if (source == SelectedIndex && IsFocused)
                    context.StrokeRectangle(Local(row), ConfigFocusColor, 1);
                foreach (var c in _columns)
                {
                    var b = new Bounds2(_body.X + c.X - HorizontalScrollBar.Value, row.Y, c.Width, rowHeight);
                    if (b.Right < _body.X || b.X > _body.Right)
                        continue;
                    string text = c.Column.GetText(Items[source]);
                    Size2 measured = font.Measure(text);
                    float offset = c.Column.TextAlignment == ControlAlignment.End ? MathF.Max(7, c.Width - measured.Width - 7) : c.Column.TextAlignment == ControlAlignment.Center ? MathF.Max(7, (c.Width - measured.Width) / 2) : 7;
                    using (context.PushClipScope(Local(b)))
                        context.DrawText(font, text, new Vector2(b.X - Bounds.X + offset, b.Y - Bounds.Y + (rowHeight - measured.Height) / 2), IsAvailable() ? ConfigTextColor : ConfigDisabledTextColor);
                    if (ConfigShowGridLines)
                        context.FillRectangle(new Bounds2(b.Right - Bounds.X - 1, b.Y - Bounds.Y, 1, b.Height), BorderColor);
                }

                if (ConfigShowGridLines)
                    context.FillRectangle(new Bounds2(row.X - Bounds.X, row.Bottom - Bounds.Y - 1, row.Width, 1), BorderColor);
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

    /// <summary>Gets or sets ItemHeight; a local assignment overrides the style.</summary>
    public float ItemHeight { get => ConfigItemHeight; set => ConfigItemHeight = value; }

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

    private ElementCollection<object>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<object> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);
    internal IList<object> ConfigRows => ConfigItems;
    /// <summary>Gets the owned collection for Rows.</summary>
    public IList<object> Rows { get => ConfigRows; }

    private ElementCollection<DataTableColumn>? _ownedColumnDefinitions;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ColumnDefinitionsCollectionArgs = new("Tracks");
    internal IList<DataTableColumn> ConfigColumnDefinitions => _ownedColumnDefinitions ??= new(this, ColumnDefinitionsCollectionArgs);
    /// <summary>Gets the owned collection for Tracks.</summary>
    public IList<DataTableColumn> Tracks { get => ConfigColumnDefinitions; }

    private bool _storedShowHeaders = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowHeadersStorageArgs = new("ShowHeaders");
    internal bool ConfigShowHeaders { get => _storedShowHeaders; set => SetNotify(ref _storedShowHeaders, value, ShowHeadersStorageArgs); }
    /// <summary>Gets or sets the value for ShowHeaders.</summary>
    public bool ShowHeaders { get => ConfigShowHeaders; set => ConfigShowHeaders = value; }

    private bool _storedCanUserResizeColumns = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanUserResizeColumnsStorageArgs = new("CanUserResizeColumns");
    internal bool ConfigCanUserResizeColumns { get => _storedCanUserResizeColumns; set => SetNotify(ref _storedCanUserResizeColumns, value, CanUserResizeColumnsStorageArgs); }
    /// <summary>Gets or sets the value for CanUserResizeColumns.</summary>
    public bool CanUserResizeColumns { get => ConfigCanUserResizeColumns; set => ConfigCanUserResizeColumns = value; }

    private bool _storedCanUserSortColumns = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanUserSortColumnsStorageArgs = new("CanUserSortColumns");
    internal bool ConfigCanUserSortColumns { get => _storedCanUserSortColumns; set => SetNotify(ref _storedCanUserSortColumns, value, CanUserSortColumnsStorageArgs); }
    /// <summary>Gets or sets the value for CanUserSortColumns.</summary>
    public bool CanUserSortColumns { get => ConfigCanUserSortColumns; set => ConfigCanUserSortColumns = value; }

    private bool _storedShowGridLines = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowGridLinesStorageArgs = new("ShowGridLines");
    internal bool ConfigShowGridLines { get => _storedShowGridLines; set => SetNotify(ref _storedShowGridLines, value, ShowGridLinesStorageArgs); }
    /// <summary>Gets or sets the value for ShowGridLines.</summary>
    public bool ShowGridLines { get => ConfigShowGridLines; set => ConfigShowGridLines = value; }

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
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<System.Windows.Input.ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;
                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
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
            case "ItemHeight":
                return _styleItemHeight.HasLocalValue;
            case "MultiSelect":
                return _styleMultiSelect.HasLocalValue;
            case "SelectedIndex":
                return _styleSelectedIndex.HasLocalValue;
            case "Command":
                return _styleCommand.HasLocalValue;
            case "CommandParameter":
                return _styleCommandParameter.HasLocalValue;
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
            case "Command":
                if (_styleCommand.ClearLocal())
                    NotifyPropertyChanged(CommandChangedArgs);
                return;
            case "CommandParameter":
                if (_styleCommandParameter.ClearLocal())
                    NotifyPropertyChanged(CommandParameterChangedArgs);
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
        if (_styleItemHeight.BeginStyle(30f))
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleMultiSelect.BeginStyle(false))
            NotifyPropertyChanged(MultiSelectChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleCommand.BeginStyle(null))
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.BeginStyle(null))
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleHeaderHeight.BeginStyle(38f))
            NotifyPropertyChanged(HeaderHeightChangedArgs);
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
        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleHeaderHeight.CommitStyle())
            NotifyPropertyChanged(HeaderHeightChangedArgs);
    }
}

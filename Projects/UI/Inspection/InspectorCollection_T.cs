using System.Collections.Specialized;
using System.ComponentModel;
using Sachssoft.Engine.Input;
using Sachssoft.Engine.Collections;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>
/// Edits a tracked collection through its own Add, RemoveAt and Move operations, with selected-item properties below the list.
/// </summary>
/// <typeparam name = "T">The reference type stored in the tracked collection.</typeparam>
public sealed class InspectorCollection<T> : ContentControl where T : class
{
    private readonly TrackableCollection<T> _source;
    private readonly Func<T> _create;
    private readonly Func<T, string> _label;
    private readonly Action<InspectorView, T> _describe;
    private readonly List<T> _subscribed = new();
    private readonly ListBox _list = new()
    {
        Height = 104
    };
    private readonly ContentControl _details = new();
    private bool _dirty = true, _syncing;
    private T? _selected, _displayed;
    /// <summary>
    /// Creates a collection editor using an explicit item factory, label getter and property description.
    /// </summary>
    public InspectorCollection(TrackableCollection<T> source, Func<T> create, Func<T, string> label, Action<InspectorView, T> describe)
    {
        _source = source;
        _create = create;
        _label = label;
        _describe = describe;
        var body = new StackPanel
        {
            Spacing = 8
        };
        var toolbar = new StackPanel
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 6
        };
        AddAction(toolbar, "+", AddItem, () => true);
        AddAction(toolbar, "−", RemoveSelected, () => SelectedIndex >= 0);
        AddAction(toolbar, "↑", () => MoveSelected(-1), () => SelectedIndex > 0);
        AddAction(toolbar, "↓", () => MoveSelected(1), () => SelectedIndex >= 0 && SelectedIndex + 1 < _source.Count);
        body.Add(_list);
        body.Add(toolbar);
        body.Add(_details);
        Content = body;
        _list.SelectionChanged += (_, _) =>
        {
            if (!_syncing)
            {
                _selected = _list.SelectedItem?.Value as T;
                RefreshDetails();
            }
        };
    }

    /// <summary>
    /// Gets the selectable item list.
    /// </summary>
    public ListBox List => _list;
    /// <summary>
    /// Gets the selected item or null.
    /// </summary>
    public T? SelectedItem => _selected;
    /// <summary>
    /// Gets the selected item's current collection index.
    /// </summary>
    public int SelectedIndex => _selected is null ? -1 : _source.IndexOf(_selected);

    /// <summary>
    /// Adds an item through the tracked collection and selects it.
    /// </summary>
    public void AddItem()
    {
        _selected = _create();
        _source.Add(_selected);
        _dirty = true;
    }

    /// <summary>
    /// Removes the selected item through the tracked collection.
    /// </summary>
    public void RemoveSelected()
    {
        int index = SelectedIndex;
        if (index < 0)
            return;
        _source.RemoveAt(index);
        _selected = _source.Count == 0 ? null : _source[Math.Min(index, _source.Count - 1)];
        _dirty = true;
    }

    /// <summary>
    /// Moves the selected item by an offset while retaining object identity.
    /// </summary>
    public void MoveSelected(int offset)
    {
        int index = SelectedIndex;
        int next = index + offset;
        if (index >= 0 && next >= 0 && next < _source.Count)
            _source.Move(index, next);
    }

    private static void AddAction(StackPanel toolbar, string text, Action action, Func<bool> enabled)
    {
        Control content = text switch
        {
            "↑" => new ControlGlyph(ControlGlyphKind.Up),
            "↓" => new ControlGlyph(ControlGlyphKind.Down),
            _ => new TextBlock
            {
                Text = text
            }
        };
        toolbar.Add(new Button { Width = 42, Height = 30, Content = content, Command = new Command(_ => action(), _ => enabled()) });
    }

    private void Changed(object? sender, NotifyCollectionChangedEventArgs args) => _dirty = true;
    private void ItemChanged(object? sender, PropertyChangedEventArgs args) => _dirty = true;
    private void UnsubscribeItems()
    {
        foreach (T item in _subscribed)
            if (item is INotifyPropertyChanged observable)
                observable.PropertyChanged -= ItemChanged;
        _subscribed.Clear();
    }

    private void RefreshDetails()
    {
        if (ReferenceEquals(_selected, _displayed))
            return;
        _displayed = _selected;
        if (_selected is null)
        {
            _details.Content = null;
            return;
        }

        var fields = new InspectorView();
        _describe(fields, _selected);
        _details.Content = fields;
    }

    private void Refresh()
    {
        _dirty = false;
        _syncing = true;
        try
        {
            UnsubscribeItems();
            _list.Items.Clear();
            foreach (T item in _source)
            {
                _list.Items.Add(new SelectorItem(_label(item), item));
                if (Workspace is not null && item is INotifyPropertyChanged observable)
                {
                    observable.PropertyChanged += ItemChanged;
                    _subscribed.Add(item);
                }
            }

            if (SelectedIndex < 0)
                _selected = _source.Count == 0 ? null : _source[0];
            _list.SelectedIndex = SelectedIndex;
            RefreshDetails();
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        _source.CollectionChanged += Changed;
        _dirty = true;
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        _source.CollectionChanged -= Changed;
        UnsubscribeItems();
        base.OnDetached();
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_dirty)
            Refresh();
        base.OnUpdate(elapsed);
    }
}

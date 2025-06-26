using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

// Developed by Tobias Sachs.
public abstract class ItemsWidget<TCollection> : Widget, IItemsWidgetProvider where TCollection : WidgetCollection, new()
{
    private readonly TCollection _widgets;
    private IEnumerable<Widget>? _widgets_source;

    protected ItemsWidget()
    {
        _widgets = new TCollection();
        _widgets.CollectionChanged += OnWidgetsCollectionChanged;
    }

    private void OnWidgetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_widgets_source != null && _widgets.Count > 0)
        {
            throw new InvalidOperationException("Cannot use both 'Widgets' and 'WidgetsSource'. Use only one.");
        }
    }

    public TCollection Widgets => _widgets;

    IList<Widget> IItemsWidgetProvider.Widgets => _widgets;

    public IEnumerable<Widget>? WidgetsSource
    {
        get => _widgets_source;
        set
        {
            if (_widgets_source == value)
                return;

            if (_widgets_source != null)
                _widgets.RemoveRange(_widgets_source);

            if (value != null && _widgets.Count > 0)
                throw new InvalidOperationException("Cannot set 'WidgetsSource' while 'Widgets' has items.");

            _widgets_source = value;

            if (_widgets_source != null)
                _widgets.AddRange(_widgets_source);
        }
    }
}

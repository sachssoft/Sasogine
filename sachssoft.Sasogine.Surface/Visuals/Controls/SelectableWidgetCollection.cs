using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

public class SelectableWidgetCollection : WidgetCollection
{
    private Widget? _selected_item;
    private int _selected_index = -1;

    public event EventHandler? SelectionChanged;

    public Widget? SelectedItem
    {
        get => _selected_item;
        set
        {
            if (_selected_item != value)
            {
                if (value != null && !Contains(value))
                    throw new ArgumentException("Selected item must be in the collection.");

                _selected_item = value;
                _selected_index = (value != null) ? IndexOf(value) : -1;
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }

    public int SelectedIndex
    {
        get => _selected_index;
        set
        {
            if (_selected_index != value)
            {
                if (value < -1 || value >= Count)
                    throw new ArgumentOutOfRangeException(nameof(value), "SelectedIndex must be between -1 and Count-1.");

                _selected_index = value;
                _selected_item = (value != -1) ? this[value] : null;
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }

    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    public bool Select(Widget widget)
    {
        if (!Contains(widget))
            return false;

        SelectedItem = widget;
        return true;
    }

    public void Deselect()
    {
        SelectedIndex = -1;
    }

    protected override void RemoveItem(int index)
    {
        var item = this[index];
        base.RemoveItem(index);

        if (SelectedItem == item)
            SelectedItem = null;
        else if (_selected_index > index)
            _selected_index--; // Index verschieben wenn ein Element vor Auswahl entfernt wurde
    }
}

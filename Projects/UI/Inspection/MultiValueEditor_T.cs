using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Edits a composite value as one horizontal row of equally sized, named numeric fields.</summary>
/// <typeparam name = "T">The composite value type, such as a vector, size or point.</typeparam>
public sealed class MultiValueEditor<T> : Grid where T : struct
{
    private readonly ValueComponent<T>[] _components;
    private readonly ValueTextBox[] _editors;
    private readonly IReadOnlyList<ValueTextBox> _editorView;
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    private T _value;
    private bool _updating;
    /// <summary>Creates a row from a value and explicitly described components.</summary>
    public MultiValueEditor(T value, params ValueComponent<T>[] components)
    {
        ArgumentNullException.ThrowIfNull(components);
        if (components.Length == 0)
            throw new ArgumentException("At least one component is required.", nameof(components));
        _components = (ValueComponent<T>[])components.Clone();
        _editors = new ValueTextBox[components.Length];
        _editorView = Array.AsReadOnly(_editors);
        StyleId = "Inspector.MultiValue";
        for (int index = 0; index < _components.Length; index++)
        {
            ValueComponent<T> component = _components[index];
            if (component.Read is null || component.WithValue is null)
                throw new ArgumentException("Default component descriptions are not valid.", nameof(components));
            Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
            var cell = new Grid
            {
                StyleId = "Inspector.Component"
            };
            cell.Tracks.Add(new ColumnTrack { Width = new GridLength(14) });
            cell.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
            cell.Add(new TextBlock { Text = component.Name, VerticalAlignment = ControlAlignment.Center }, column: 0);
            var editor = new ValueTextBox
            {
                Minimum = component.Minimum,
                Maximum = component.Maximum,
                Step = component.Step,
                ButtonWidth = 12,
                Height = 32,
                ToolTip = component.Name
            };
            _editors[index] = editor;
            editor.ValueChanged += (_, _) =>
            {
                if (!_updating)
                {
                    // Read the latest composite when editing so pending sibling changes are preserved.
                    T current = ReadCurrent is null ? _value : ReadCurrent();
                    Value = component.WithValue(current, editor.Value);
                }
            };
            cell.Add(editor, column: 1);
            Add(cell, column: index);
        }

        _value = value;
        DisplayValue();
    }

    /// <summary>Gets the individual fields in component order.</summary>
    public IReadOnlyList<ValueTextBox> Editors => _editorView;

    /// <summary>Gets or sets the composite value without replacing existing component controls.</summary>
    public T Value
    {
        get => _value;
        set
        {
            if (SetNotify(ref _value, value, ValueArgs))
            {
                DisplayValue();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>Occurs when the composite value changes.</summary>
    public event EventHandler? ValueChanged;
    internal Func<T>? ReadCurrent { get; set; }

    private void DisplayValue()
    {
        _updating = true;
        try
        {
            for (int index = 0; index < _editors.Length; index++)
                _editors[index].Value = _components[index].Read(_value);
        }
        finally
        {
            _updating = false;
        }
    }
}

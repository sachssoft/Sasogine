using System.ComponentModel;
using System.Numerics;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Edits complete structs using their actual numeric type and explicitly grouped component rows.</summary>
/// <typeparam name = "T">The composite struct.</typeparam>
/// <typeparam name = "TNumber">The actual component number type.</typeparam>
public sealed class TypedMultiValueEditor<T, TNumber> : Grid where T : struct where TNumber : struct, INumber<TNumber>
{
    private readonly NumericValueComponent<T, TNumber>[] _components;
    private readonly TypedValueTextBox<TNumber>[] _editors;
    private readonly IReadOnlyList<TypedValueTextBox<TNumber>> _editorView;
    private readonly NumericCompositeComparer<T, TNumber> _comparer;
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    private T _value;
    private bool _updating;
    /// <summary>Creates a value editor from explicit typed component descriptions.</summary>
    public TypedMultiValueEditor(T value, params NumericValueComponent<T, TNumber>[] components)
    {
        ArgumentNullException.ThrowIfNull(components);
        if (components.Length == 0)
            throw new ArgumentException("At least one component is required.", nameof(components));
        _components = (NumericValueComponent<T, TNumber>[])components.Clone();
        _editors = new TypedValueTextBox<TNumber>[components.Length];
        _editorView = Array.AsReadOnly(_editors);
        _comparer = new(_components);
        _value = value;
        StyleId = "Inspector.MultiValue";
        var groups = new List<string>();
        var counts = new List<int>();
        for (int i = 0; i < _components.Length; i++)
        {
            var component = _components[i];
            if (component.Read is null || component.WithValue is null)
                throw new ArgumentException("Default component descriptions are not valid.", nameof(components));
            int group = groups.IndexOf(component.Group);
            if (group < 0)
            {
                groups.Add(component.Group);
                counts.Add(1);
            }
            else
                counts[group]++;
        }

        bool headings = groups.Any(static name => name.Length > 0);
        if (headings)
            Tracks.Add(new ColumnTrack { Width = GridLength.Auto });
        int columns = counts.Max();
        for (int i = 0; i < columns; i++)
            Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        for (int row = 0; row < groups.Count; row++)
        {
            Rows.Add(new RowTrack { Height = GridLength.Auto });
            if (headings)
                Add(new TextBlock { Text = groups[row], VerticalAlignment = ControlAlignment.Center }, row, 0);
        }

        var offsets = new int[groups.Count];
        for (int i = 0; i < _components.Length; i++)
        {
            var component = _components[i];
            int row = groups.IndexOf(component.Group);
            int column = offsets[row]++ + (headings ? 1 : 0);
            var cell = new Grid
            {
                StyleId = "Inspector.Component"
            };
            cell.Tracks.Add(new ColumnTrack { Width = GridLength.Auto });
            cell.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
            cell.Add(new TextBlock { Text = component.Name, VerticalAlignment = ControlAlignment.Center }, column: 0);
            var editor = InspectorNumber.Create(component.Read(value));
            _editors[i] = editor;
            editor.ValueChanged += (_, _) =>
            {
                if (!_updating)
                    Value = component.WithValue(ReadCurrent is null ? _value : ReadCurrent(), editor.Value);
            };
            cell.Add(editor, column: 1);
            Add(cell, row, column);
        }
    }

    /// <summary>Gets fields in descriptor order.</summary>
    public IReadOnlyList<TypedValueTextBox<TNumber>> Editors => _editorView;

    /// <summary>Gets or sets the complete value without recreating component controls.</summary>
    public T Value
    {
        get => _value;
        set
        {
            if (_comparer.Equals(_value, value))
            {
                _value = value;
                return;
            }

            T previous = _value;
            _value = value;
            NotifyPropertyChanged(ValueArgs);
            _updating = true;
            try
            {
                for (int i = 0; i < _editors.Length; i++)
                    _editors[i].Value = _components[i].Read(value);
            }
            finally
            {
                _updating = false;
            }

            try
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                _value = previous;
                _updating = true;
                try
                {
                    for (int i = 0; i < _editors.Length; i++)
                        _editors[i].Value = _components[i].Read(previous);
                }
                finally
                {
                    _updating = false;
                }

                NotifyPropertyChanged(ValueArgs);
                throw;
            }
        }
    }

    /// <summary>Occurs after a complete value changes.</summary>
    public event EventHandler? ValueChanged;
    internal Func<T>? ReadCurrent { get; set; }
    internal IEqualityComparer<T> ValueComparer => _comparer;
}

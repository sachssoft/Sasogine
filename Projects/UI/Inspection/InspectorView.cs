using System.ComponentModel;
using Sachssoft.Engine.Components.Definitions.Inspectors;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Builds grouped property editors from typed delegates, with notification-driven refresh or optional polling.</summary>
public sealed class InspectorView : ContentControl, IInspectorView
{
    private readonly Dictionary<string, InspectorGroup> _namedGroups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Control> _namedEditors = new(StringComparer.Ordinal);
    private readonly List<IInspectorBinding> _bindings = new();
    private readonly Dictionary<Control, InspectorPropertyMenu> _propertyMenus = new();
    private readonly InspectorEditorRegistry? _editors;
    private readonly StackPanel _fields = new()
    {
        Spacing = 8
    };
    private bool? _localShowPropertyMenus;
    private bool _showPropertyMenus = true;
    private static readonly PropertyChangedEventArgs ShowPropertyMenusArgs = new(nameof(ShowPropertyMenus));
    /// <summary>Gets or sets trailing menu visibility. Nested inspectors inherit it unless explicitly overridden.</summary>
    public bool ShowPropertyMenus
    {
        get => _showPropertyMenus;
        set
        {
            if (_localShowPropertyMenus == value) return;
            _localShowPropertyMenus = value;
            RefreshPropertyMenuVisibility();
        }
    }
    private void RefreshPropertyMenuVisibility()
    {
        bool inherited = true;
        for (Control? parent = Parent; parent is not null; parent = parent.Parent)
            if (parent is InspectorView inspector) { inherited = inspector.ShowPropertyMenus; break; }
        bool visible = _localShowPropertyMenus ?? inherited;
        if (SetNotify(ref _showPropertyMenus, visible, ShowPropertyMenusArgs))
            foreach (InspectorPropertyMenu menu in _propertyMenus.Values) menu.SetMenuVisible(visible);
        RefreshNestedVisibility(_fields);
    }
    private static void RefreshNestedVisibility(Control control)
    {
        if (control is InspectorView inspector) { inspector.RefreshPropertyMenuVisibility(); return; }
        if (control is ContainerControl container)
            foreach (Control child in container.Children) RefreshNestedVisibility(child);
    }

    /// <summary>Creates an inspector. Place it in a ScrollViewer when its content may exceed the available height.</summary>
    public InspectorView() : this(null)
    {
    }

    /// <summary>Creates an inspector with an optional independent typed editor registry.</summary>
    public InspectorView(InspectorEditorRegistry? editors)
    {
        _editors = editors;
        Content = _fields;
    }

    /// <summary>Gets the explicit value editor factories available when fields are created.</summary>
    public InspectorEditorRegistry Editors => _editors ?? Workspace?.Registry.InspectorEditors ?? InspectorEditorRegistry.Default;

    /// <summary>Adds the registered editor for an explicitly bound value without discovering its members.</summary>
    public Control Add<T>(string title, Func<T> read, Action<T> write, INotifyPropertyChanged? source = null, string? propertyName = null) => Editors.Add(this, title, read, write, source, propertyName);
    /// <summary>Adds a named property through the UI-independent inspector contract.</summary>
    /// <remarks>A missing setter disables editing and mutating property-menu actions while retaining live display and copy support.</remarks>
    public Control Add<TValue>(string name, string title, Func<TValue> read, Action<TValue>? write = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(read);
        if (_namedEditors.ContainsKey(name))
            throw new ArgumentException("A field with this name already exists in this inspector group.", nameof(name));

        Control editor = Add(title, read, write ?? (static _ => { }), propertyName: name);
        if (write is null)
        {
            editor.IsEnabled = false;
            if (editor is TextBox text) text.IsReadOnly = true;
            GetPropertyMenu(editor).SetReadOnly();
        }
        _namedEditors.Add(name, editor);
        return editor;
    }

    void IInspectorView.Add<TValue>(string name, string title, Func<TValue> read, Action<TValue>? write) =>
        Add(name, title, read, write);

    /// <summary>Gets the editor of a named field in this group.</summary>
    public Control GetFieldEditor(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _namedEditors[name];
    }

    /// <summary>Gets or creates a group by its stable name, updating its display title when reused.</summary>
    public InspectorView Group(string name, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(title);
        if (!_namedGroups.TryGetValue(name, out InspectorGroup? group))
        {
            group = new InspectorGroup(title, Editors);
            _namedGroups.Add(name, group);
            Add(group);
        }
        else if (group.HeaderContent is TextBlock heading)
            heading.Text = title;
        return group.Fields;
    }

    IInspectorView IInspectorView.Group(string name, string title) => Group(name, title);

    /// <inheritdoc/>
    public override void Add(Control child) => _fields.Add(child);
    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        _fields.Remove(child);
        string? name = null;
        foreach (var entry in _namedGroups)
            if (ReferenceEquals(entry.Value, child)) { name = entry.Key; break; }
        if (name is not null) _namedGroups.Remove(name);
    }
    /// <summary>Removes fields and groups and releases their source subscriptions.</summary>
    public override void Clear()
    {
        foreach (IInspectorBinding binding in _bindings)
        {
            Workspace?.Bindings.Remove(binding);
            binding.Disconnect();
        }

        _bindings.Clear();
        _propertyMenus.Clear();
        _namedGroups.Clear();
        _namedEditors.Clear();
        _fields.Clear();
    }

    /// <summary>Adds a persistent, collapsible category or nested definition.</summary>
    public InspectorGroup AddGroup(string title, bool expanded = true)
    {
        var group = new InspectorGroup(title, _editors)
        {
            IsExpanded = expanded
        };
        Add(group);
        return group;
    }

    /// <summary>Adds a label and an arbitrary existing editor.</summary>
    public void AddField(string title, Control editor) => AddFieldCore(title, editor, null);

    private void AddFieldCore(string title, Control editor, IInspectorPropertyActions? actions)
    {
        var row = new Grid
        {
            Spacing = 10
        };
        row.Tracks.Add(new ColumnTrack { Width = new GridLength(112) });
        row.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        row.Tracks.Add(new ColumnTrack { Width = new GridLength(26) });
        row.Add(new TextBlock { Text = title, VerticalAlignment = ControlAlignment.Center }, column: 0);
        row.Add(editor, column: 1);
        var menu = new InspectorPropertyMenu(this, editor, actions);
        row.Add(menu, column: 2);
        menu.SetMenuVisible(ShowPropertyMenus);
        _propertyMenus.Add(editor, menu);
        Add(row);
    }

    private void AddBoundField<T>(string title, Control editor, InspectorValueBinding<T> binding, Func<T> read, Func<T, T>? snapshot = null) =>
        AddFieldCore(title, editor, new InspectorPropertyActions<T>(read, binding.SetValue, snapshot));

    /// <summary>Gets the trailing menu for a field created by this inspector.</summary>
    public InspectorPropertyMenu GetPropertyMenu(Control editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        return _propertyMenus.TryGetValue(editor, out InspectorPropertyMenu? menu) ? menu
            : throw new ArgumentException("The editor does not belong to this inspector.", nameof(editor));
    }
    /// <summary>Sets an explicit reset value. Otherwise Reset restores the snapshot captured when the field was created.</summary>
    public void SetDefault<T>(Control editor, T value) => GetPropertyMenu(editor).SetDefault(value);

    internal bool OwnsMenu(Control editor, InspectorPropertyMenu menu) =>
        _propertyMenus.TryGetValue(editor, out InspectorPropertyMenu? current) && ReferenceEquals(current, menu);

    private InspectorValueBinding<T> Bind<T>(Func<T> read, Action<T> write, Action<T> display, INotifyPropertyChanged? source, string? name, IEqualityComparer<T>? comparer = null)
    {
        var binding = new InspectorValueBinding<T>(read, write, display, source, name, comparer);
        _bindings.Add(binding);
        if (Workspace is not null)
        {
            binding.Connect();
            Workspace.Bindings.Add(binding);
        }

        binding.Update();
        return binding;
    }

    /// <summary>Adds a text editor. Supply source and propertyName for notification-driven refresh.</summary>
    public TextBox AddText(string title, Func<string> read, Action<string> write, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        var editor = new TextBox
        {
            Height = 32
        };
        var binding = Bind(read, write, value => editor.Text = value, source, propertyName);
        editor.TextChanged += (_, _) => binding.SetValue(editor.Text);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds a finite numeric editor using the shared value bar and text input control.</summary>
    public ValueTextBox AddSingle(string title, Func<float> read, Action<float> write, float minimum, float maximum, float step = 1, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        var editor = new ValueTextBox
        {
            Minimum = minimum,
            Maximum = maximum,
            Step = step,
            Height = 32
        };
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName);
        editor.ValueChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds one property row with explicitly described components of a composite value.</summary>
    public TypedMultiValueEditor<T, TNumber> AddNumericValues<T, TNumber>(string title, Func<T> read, Action<T> write, NumericValueComponent<T, TNumber>[] components, INotifyPropertyChanged? source = null, string? propertyName = null)
        where T : struct where TNumber : struct, System.Numerics.INumber<TNumber>
    {
        var editor = new TypedMultiValueEditor<T, TNumber>(read(), components)
        {
            ReadCurrent = read
        };
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName, editor.ValueComparer);
        editor.ValueChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds numeric input in its actual number type, preserving large integers and floating-point precision.</summary>
    public TypedValueTextBox<TNumber> AddNumber<TNumber>(string title, Func<TNumber> read, Action<TNumber> write, INotifyPropertyChanged? source = null, string? propertyName = null)
        where TNumber : struct, System.Numerics.INumber<TNumber>
    {
        var editor = InspectorNumber.Create(read());
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName);
        editor.ValueChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds a typed text editor with explicit formatting and parsing.</summary>
    public TypedValueTextBox<T> AddParsed<T>(string title, Func<T> read, Action<T> write, Func<T, string> format, TryParseInspectorValue<T> parse, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        var editor = new TypedValueTextBox<T>(read(), format, parse);
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName);
        editor.ValueChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds choices explicitly, including enum choices, without enumerating runtime members.</summary>
    public ComboBox AddChoice<T>(string title, Func<T> read, Action<T> write, IReadOnlyList<KeyValuePair<string, T>> choices, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(choices);
        var values = choices.ToArray();
        var editor = new ComboBox
        {
            Height = 32
        };
        for (int i = 0; i < values.Length; i++)
            editor.Items.Add(new SelectorItem(values[i].Key));
        var binding = Bind(read, write, value =>
        {
            int index = -1;
            for (int i = 0; i < values.Length; i++)
                if (EqualityComparer<T>.Default.Equals(values[i].Value, value))
                {
                    index = i;
                    break;
                }

            editor.SelectedIndex = index;
        }, source, propertyName);
        editor.SelectionChanged += (_, _) =>
        {
            if (editor.SelectedIndex >= 0)
                binding.SetValue(values[editor.SelectedIndex].Value);
        };
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds one property row with explicitly described components of a composite value.</summary>
    public MultiValueEditor<T> AddValues<T>(string title, Func<T> read, Action<T> write, ValueComponent<T>[] components, INotifyPropertyChanged? source = null, string? propertyName = null)
        where T : struct
    {
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        var editor = new MultiValueEditor<T>(read(), components)
        {
            ReadCurrent = read
        };
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName);
        editor.ValueChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds one X/Y row bound to a complete two-dimensional vector.</summary>
    public MultiValueEditor<Vector2> AddVector2(string title, Func<Vector2> read, Action<Vector2> write, float minimum, float maximum, float step = 1, INotifyPropertyChanged? source = null, string? propertyName = null) => AddValues(title, read, write, new[] { new ValueComponent<Vector2>("X", static value => value.X, static (value, x) => new(x, value.Y), minimum, maximum, step), new ValueComponent<Vector2>("Y", static value => value.Y, static (value, y) => new(value.X, y), minimum, maximum, step) }, source, propertyName);
    /// <summary>Adds one X/Y/Z row bound to a complete three-dimensional vector.</summary>
    public MultiValueEditor<Vector3> AddVector3(string title, Func<Vector3> read, Action<Vector3> write, float minimum, float maximum, float step = 1, INotifyPropertyChanged? source = null, string? propertyName = null) => AddValues(title, read, write, new[] { new ValueComponent<Vector3>("X", static value => value.X, static (value, x) => new(x, value.Y, value.Z), minimum, maximum, step), new ValueComponent<Vector3>("Y", static value => value.Y, static (value, y) => new(value.X, y, value.Z), minimum, maximum, step), new ValueComponent<Vector3>("Z", static value => value.Z, static (value, z) => new(value.X, value.Y, z), minimum, maximum, step) }, source, propertyName);
    /// <summary>Adds one X/Y/Z/W row bound to a complete four-dimensional vector.</summary>
    public MultiValueEditor<Vector4> AddVector4(string title, Func<Vector4> read, Action<Vector4> write, float minimum, float maximum, float step = 1, INotifyPropertyChanged? source = null, string? propertyName = null) => AddValues(title, read, write, new[] { new ValueComponent<Vector4>("X", static value => value.X, static (value, x) => new(x, value.Y, value.Z, value.W), minimum, maximum, step), new ValueComponent<Vector4>("Y", static value => value.Y, static (value, y) => new(value.X, y, value.Z, value.W), minimum, maximum, step), new ValueComponent<Vector4>("Z", static value => value.Z, static (value, z) => new(value.X, value.Y, z, value.W), minimum, maximum, step), new ValueComponent<Vector4>("W", static value => value.W, static (value, w) => new(value.X, value.Y, value.Z, w), minimum, maximum, step) }, source, propertyName);
    /// <summary>Adds a boolean editor with an explicit setter.</summary>
    public CheckBox AddBoolean(string title, Func<bool> read, Action<bool> write, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        var editor = new CheckBox
        {
            Height = 30
        };
        var binding = Bind(read, write, value => editor.IsChecked = value, source, propertyName);
        editor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CheckBox.IsChecked))
                binding.SetValue(editor.IsChecked == true);
        };
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds a color swatch opening the existing multi-model color picker; cancellation preserves the source.</summary>
    public Button AddColor(string title, Func<Color> read, Action<Color> write, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        var swatch = new Panel
        {
            Width = 24,
            Height = 16
        };
        var label = new TextBlock();
        var content = new StackPanel
        {
            Orientation = StackOrientation.Horizontal,
            Spacing = 8
        };
        content.Add(swatch);
        content.Add(label);
        var editor = new Button
        {
            Height = 32,
            Content = content
        };
        var binding = Bind(read, write, value =>
        {
            swatch.Background = value;
            label.Text = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }, source, propertyName);
        editor.Clicked += (_, _) =>
        {
            Workspace? owner = Workspace;
            if (owner is null)
                return;
            var dialog = new ColorPickerDialog
            {
                SelectedColor = read()
            };
            dialog.Closed += (_, _) =>
            {
                if (dialog.Result == DialogResult.OK && Workspace == owner && editor.Workspace == owner)
                    binding.SetValue(dialog.SelectedColor);
            };
            _ = owner.ShowDialog(dialog);
        };
        AddBoundField(title, editor, binding, read);
        return editor;
    }

    /// <summary>Adds a type-filtered reference field using the shared asset picker.</summary>
    public ReferencePicker<T> AddReference<T>(string title, Func<Reference<T>?> read, Action<Reference<T>?> write,
        IReadOnlyList<AssetItem> assets, INotifyPropertyChanged? source = null, string? propertyName = null)
        where T : class, IEngineReferenceable
    {
        ArgumentNullException.ThrowIfNull(assets);
        var editor = new ReferencePicker<T> { Source = assets };
        var binding = Bind(read, write, value => editor.Value = value, source, propertyName);
        editor.ReferenceChanged += (_, _) => binding.SetValue(editor.Value);
        AddBoundField(title, editor, binding, read, static value => value is null ? null : new Reference<T>(value.Id));
        return editor;
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        RefreshPropertyMenuVisibility();
        foreach (IInspectorBinding binding in _bindings)
        {
            binding.Connect();
            Workspace!.Bindings.Add(binding);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        foreach (IInspectorBinding binding in _bindings)
        {
            Workspace!.Bindings.Remove(binding);
            binding.Disconnect();
        }

        base.OnDetached();
    }
}

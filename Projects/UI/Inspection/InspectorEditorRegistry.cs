using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Maps explicit generic value identities to typed editor factories, with a cached automatic fallback for enums.</summary>
public sealed class InspectorEditorRegistry
{
    private readonly Dictionary<object, Delegate> _factories;
    private readonly List<string> _names;
    private readonly IReadOnlyList<string> _nameView;
    private readonly bool _readOnly;
    /// <summary>Gets the immutable built-in registrations used by detached inspectors.</summary>
    public static InspectorEditorRegistry Default { get; } = new(true);

    private InspectorEditorRegistry(bool builtins)
    {
        _factories = new();
        _names = new();
        _nameView = _names.AsReadOnly();
        InspectorBuiltInEditors.Register(this);
        _readOnly = builtins;
    }

    /// <summary>Creates an independent registry initialized with the built-in factories.</summary>
    public InspectorEditorRegistry()
    {
        _factories = new(Default._factories);
        _names = new(Default._names);
        _nameView = _names.AsReadOnly();
    }

    /// <summary>Gets the number of explicit registrations; automatically supported enums are not counted.</summary>
    public int Count => _factories.Count;
    /// <summary>Gets the explicit human-readable registration names.</summary>
    public IReadOnlyList<string> Names => _nameView;

    /// <summary>Registers or replaces an editor for an explicit value type.</summary>
    public void Register<T>(string name, InspectorEditorFactory<T> factory)
    {
        if (_readOnly)
            throw new InvalidOperationException("The built-in registry is immutable. Create an independent registry for custom editors.");
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(factory);
        object key = InspectorEditorKey<T>.Value;
        if (!_factories.ContainsKey(key))
            _names.Add(name);
        _factories[key] = factory;
    }

    /// <summary>Checks whether a typed registration or the automatic enum editor is available.</summary>
    public bool Contains<T>() => _factories.ContainsKey(InspectorEditorKey<T>.Value) || InspectorEnumEditor<T>.Factory is not null;
    /// <summary>Registers explicit choices, including enum values, without runtime member enumeration.</summary>
    public void RegisterChoices<T>(string name, IReadOnlyList<KeyValuePair<string, T>> choices)
    {
        ArgumentNullException.ThrowIfNull(choices);
        var copy = choices.ToArray();
        Register<T>(name, (view, title, read, write, source, propertyName) => view.AddChoice(title, read, write, copy, source, propertyName));
    }

    /// <summary>Registers low/high tiered scores for a custom supported numeric type.</summary>
    public void RegisterScores<TNumber>(string numberName)
        where TNumber : struct, System.Numerics.INumber<TNumber>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(numberName);
        InspectorBuiltInEditors.RegisterScores<TNumber>(this, numberName);
    }

    /// <summary>Adds the registered editor for an explicitly bound value.</summary>
    public Control Add<T>(InspectorView view, string title, Func<T> read, Action<T> write, INotifyPropertyChanged? source = null, string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(write);
        InspectorEditorFactory<T>? editor = _factories.TryGetValue(InspectorEditorKey<T>.Value, out Delegate? factory)
            ? (InspectorEditorFactory<T>)factory
            : InspectorEnumEditor<T>.Factory;
        if (editor is null)
            throw new NotSupportedException("No inspector editor is registered for this value. Register a typed factory explicitly.");
        return editor(view, title, read, write, source, propertyName);
    }
}

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Retains a typed value while its editor is selected or detached.</summary>
/// <typeparam name = "T">The explicitly registered source type.</typeparam>
internal sealed class InspectorValueExample<T> : Element, IInspectorValueExample
{
    private T _value;
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    internal InspectorValueExample(string name, T value)
    {
        Name = name;
        _value = value;
    }

    public string Name { get; }
    public T Value { get => _value; set => SetNotify(ref _value, value, ValueArgs); }

    public Control Build(InspectorView inspector) => inspector.Add("Value", () => Value, value => Value = value, this, nameof(Value));
}

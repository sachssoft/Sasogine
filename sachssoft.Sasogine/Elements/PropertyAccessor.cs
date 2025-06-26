using System;

namespace sachssoft.Sasogine.Elements;

public class PropertyAccessor<TObject, TProp> : IPropertyAccessor<TObject>
{
    private readonly Func<TObject, TProp> _getter;
    private readonly Action<TObject, TProp> _setter;
    public string PropertyName { get; }
    public PropertyMetadata? Metadata { get; }

    public PropertyAccessor(string propertyName, Func<TObject, TProp> getter, Action<TObject, TProp> setter, PropertyMetadata? metadata = null)
    {
        PropertyName = propertyName;
        _getter = getter;
        _setter = setter;
        Metadata = metadata;
    }

    public object? GetValue(TObject obj) => _getter(obj);

    public void SetValue(TObject obj, object? value)
    {
        if (value is TProp typedValue)
            _setter(obj, typedValue);
        else if (value == null && !typeof(TProp).IsValueType)
            _setter(obj, default!);
        else
            throw new InvalidCastException($"Cannot cast value to type {typeof(TProp).Name}");
    }
}
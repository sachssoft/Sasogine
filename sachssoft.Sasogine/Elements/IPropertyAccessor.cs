namespace Sachssoft.Sasogine.Elements;

public interface IPropertyAccessor<TObject>
{
    object? GetValue(TObject obj);
    void SetValue(TObject obj, object? value);
    string PropertyName { get; }
    PropertyMetadata? Metadata { get; }
}

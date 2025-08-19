

using System.Collections.Specialized;

namespace Sachssoft.Sasogine.Surface;

public interface IPropertyEditor
{
    // obj = Element
    void Initialize(INotifyCollectionChanged obj, string property_name, object property_value);

    // Apply Property Value
    object Apply();
}
using System;
using System.Reflection;
using Sachssoft.Sasogine.Surface.Utility;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

internal class PropertyRecord : Record
{
    private readonly PropertyInfo _propertyInfo;

    public override string Name
    {
        get { return _propertyInfo.Name; }
    }

    public override Type Type
    {
        get { return _propertyInfo.PropertyType; }
    }

    public PropertyRecord(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }

    public override object GetValue(object obj)
    {
        return _propertyInfo.GetValue(obj, new object[0]);
    }

    public override void SetValue(object obj, object value)
    {
        _propertyInfo.SetValue(obj, value);
    }

    public override T FindAttribute<T>()
    {
        return _propertyInfo.FindAttribute<T>();
    }
}

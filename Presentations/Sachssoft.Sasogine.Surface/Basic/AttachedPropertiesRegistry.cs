using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Basic;


public abstract class BaseAttachedPropertyInfo
{
    public Type OwnerType { get; private set; }
    public int Id { get; private set; }
    public string Name { get; private set; }
    public AttachedPropertyOption Option { get; private set; }
    public abstract Type PropertyType { get; }
    public abstract object DefaultValueObject { get; }

    protected BaseAttachedPropertyInfo(int id, string name, Type ownerType, AttachedPropertyOption option)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        OwnerType = ownerType ?? throw new ArgumentNullException(nameof(ownerType));
        Name = name;
        Id = id;
        Option = option;
    }

    public abstract object GetValueObject(ElementBase obj);
    public abstract void SetValueObject(ElementBase obj, object value);
}

public class AttachedPropertyInfo<T> : BaseAttachedPropertyInfo
{
    public T DefaultValue { get; private set; }
    public override Type PropertyType => typeof(T);
    public override object DefaultValueObject => DefaultValue;

    public AttachedPropertyInfo(int id, string name, Type ownerType, T defaultValue, AttachedPropertyOption option) :
        base(id, name, ownerType, option)
    {
        DefaultValue = defaultValue;
    }

    public T GetValue(ElementBase obj)
    {
        if (obj.AttachedPropertiesValues.TryGetValue(Id, out var value))
        {
            return (T)value;
        }

        return DefaultValue;
    }

    public void SetValue(ElementBase obj, T value)
    {
        if (GetValue(obj).Equals(value))
        {
            return;
        }

        obj.AttachedPropertiesValues[Id] = value;

        var asWidget = obj as Widget;
        if (asWidget != null)
        {
            switch (Option)
            {
                case AttachedPropertyOption.None:
                    break;
                case AttachedPropertyOption.AffectsArrange:
                    asWidget.InvalidateArrange();
                    break;
                case AttachedPropertyOption.AffectsMeasure:
                    asWidget.InvalidateMeasure();
                    break;
            }
        }

        obj.OnAttachedPropertyChanged(this);
    }

    public override object GetValueObject(ElementBase widget) => GetValue(widget);
    public override void SetValueObject(ElementBase widget, object value) => SetValue(widget, (T)value);
}


public static class AttachedPropertiesRegistry
{
    private static readonly Dictionary<int, BaseAttachedPropertyInfo> _properties = new Dictionary<int, BaseAttachedPropertyInfo>();
    private static readonly Dictionary<Type, BaseAttachedPropertyInfo[]> _propertiesByType = new Dictionary<Type, BaseAttachedPropertyInfo[]>();

    public static AttachedPropertyInfo<T> Create<T>(Type type, string name, T defaultValue, AttachedPropertyOption option)
    {
        var result = new AttachedPropertyInfo<T>(_properties.Count, name, type, defaultValue, option);
        _properties[result.Id] = result;

        return result;
    }

    public static BaseAttachedPropertyInfo[] GetPropertiesOfType(Type type)
    {
        BaseAttachedPropertyInfo[] result;
        if (_propertiesByType.TryGetValue(type, out result))
        {
            return result;
        }

        var propertiesList = new List<BaseAttachedPropertyInfo>();

        // Build list of all attached properties
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            // Make sure all static fields of type are initialized
            RuntimeHelpers.RunClassConstructor(currentType.TypeHandle);
            foreach (var pair in _properties)
            {
                if (pair.Value.OwnerType == currentType)
                {
                    propertiesList.Add(pair.Value);
                }
            }

            currentType = currentType.BaseType;
        }

        result = propertiesList.ToArray();
        _propertiesByType[type] = result;

        return result;
    }
}
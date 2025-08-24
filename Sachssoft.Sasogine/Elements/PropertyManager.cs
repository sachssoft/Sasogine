//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Sachssoft.Sasogine.Elements;

//public static class PropertyManager
//{
//    private static readonly Dictionary<Type, List<object>> _properties = new();

//    public static void Register<TObject, TProp>(string propertyName, Func<TObject, TProp> getter, Action<TObject, TProp> setter, PropertyMetadata? metadata = null)
//        where TObject : GameObject
//    {
//        var type = typeof(TObject);

//        // Prüfen, ob die Property schon irgendwo in der Hierarchie registriert ist
//        if (ExistsPropertyInHierarchy(type, propertyName))
//        {
//            throw new InvalidOperationException($"PropertyDescriptor '{propertyName}' ist in der Typ-Hierarchie von {type.Name} bereits registriert.");
//        }

//        if (!_properties.TryGetValue(type, out var list))
//        {
//            list = new List<object>();
//            _properties[type] = list;
//        }

//        var accessor = new PropertyAccessor<TObject, TProp>(propertyName, getter, setter, metadata);
//        list.Add(accessor);
//    }

//    private static bool ExistsPropertyInHierarchy(Type type, string propertyName)
//    {
//        Type? find_type = type;

//        while (find_type != null && typeof(GameObject).IsAssignableFrom(find_type))
//        {
//            if (_properties.TryGetValue(find_type, out var list))
//            {
//                if (list.Any(p =>
//                {
//                    dynamic d = p; // dynamischer Cast auf PropertyAccessor<T, TProp> (zur Laufzeit)
//                    return d.PropertyName == propertyName;
//                }))
//                {
//                    return true;
//                }
//            }
//            find_type = find_type.BaseType;
//        }
//        return false;
//    }

//    public static IReadOnlyList<IPropertyAccessor<TObject>> GetProperties<TObject>() where TObject : GameObject
//    {
//        var result = new List<IPropertyAccessor<TObject>>();
//        var type = typeof(TObject);

//        while (type != null && typeof(GameObject).IsAssignableFrom(type))
//        {
//            if (_properties.TryGetValue(type, out var list))
//            {
//                foreach (var item in list)
//                {
//                    if (item is IPropertyAccessor<TObject> accessor)
//                    {
//                        // Optional: Prüfen ob schon enthalten (bei Namenskonflikt)
//                        if (!result.Any(p => p.PropertyName == accessor.PropertyName))
//                            result.Add(accessor);
//                    }
//                }
//            }

//            type = type.BaseType;
//        }

//        return result;
//    }
//}
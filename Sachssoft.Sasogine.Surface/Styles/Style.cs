using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Drawing;
using static System.Collections.Specialized.BitVector32;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public sealed class Style : StyleMap
    {
        private readonly Stylesheet _sheet;
        private readonly Type? _targetType;
        private readonly Type? _applyForType;
        private readonly string? _targetId;
        private readonly Dictionary<string, string> _propertyMap;

        public Style(Stylesheet sheet, Type? targetType, Type? applyForType, string? targetId, Dictionary<string, string> propertyMap)
        {
            _sheet = sheet;
            _targetType = targetType;
            _applyForType = applyForType;
            _targetId = targetId;
            _propertyMap = propertyMap;
        }

        public string? TargetId => _targetId;

        public Type? TargetType => _targetType;

        public Type? ApplyForType => _applyForType;

        public IEnumerable<string> Properties => _propertyMap.Keys;

        public Stylesheet Stylesheet => _sheet;

        public PropertyValue? FindProperty(string name)
        {
            foreach (var propertyEntry in _propertyMap)
            {
                if (propertyEntry.Key == name)
                {
                    return new PropertyValue(propertyEntry.Value);
                }
            }

            return null;
        }

        public Style? ResolveApplyFor()
        {
            if (_applyForType == null)
                return null;
            return _sheet.FindStyle(_applyForType, null);
        }

        public void Apply<T>(T target, StyleApplyDelegate<T> action)
            where T : Widget
        {
            foreach (var propertyEntry in _propertyMap)
            {
                action.Invoke(
                    target,
                    GetStylesheet(),
                    propertyEntry.Key,
                    new PropertyValue(propertyEntry.Value)
                );
            }
        }

        public T BuildFor<T>() where T : class, IStyleFactory<T>
        {
            var first = T.PrefixName + ".";
            Dictionary<string, string> map = new();

            foreach (var propertyEntry in _propertyMap)
            {
                if (propertyEntry.Key.StartsWith(first))
                    map.Add(propertyEntry.Key, propertyEntry.Value);
            }

            return T.Create(new StyleFactoryContext<T>(this, map));
        }

        public bool HasPropertiesFor<T>() where T : class, IStyleFactory<T>
        {
            var first = T.PrefixName + ".";

            foreach (var propertyEntry in _propertyMap)
            {
                if (propertyEntry.Key.StartsWith(first))
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            // Kombination von TargetId und TargetType
            int hash = 17;
            hash = hash * 31 + (_targetId?.GetHashCode() ?? 0);
            hash = hash * 31 + (_targetType?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is not Style other) return false;

            return _targetId == other._targetId && _targetType == other._targetType;
        }

        private protected override Stylesheet GetStylesheet()
        {
            return _sheet;
        }

        public override string ToString()
        {
            return $"TargetType: '{TargetType}'; TargetId: '{TargetId}'";
        }
    }
}

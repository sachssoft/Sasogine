using Sachssoft.Sasogine.Surface.Basic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public abstract class StyleMap
    {
        private readonly HashSet<Style> _styles = new();

        internal StyleMap()
        {
        }

        public IEnumerable<Style> Styles => _styles;

        public Style? FindStyle(Type targetType)
        {
            return _styles.Where(s => s.TargetType == targetType).LastOrDefault();
        }

        public Style? FindStyle(Type targetType, string? targetId)
        {
            targetId ??= string.Empty;
            return _styles.Where(s => s.TargetType == targetType &&
                                      s.TargetId == targetId).LastOrDefault();
        }

        // Underlying Styles
        public Style Add(string targetId, Dictionary<string, string> propertyMap)
        {
            var style = new Style(GetStylesheet(), targetType: null, applyForType: null, targetId: targetId, propertyMap);
            _styles.Add(style);
            return style;
        }

        public Style Add(Type targetType, Dictionary<string, string> propertyMap)
        {
            var style = new Style(GetStylesheet(), targetType: targetType, applyForType: null, targetId: null, propertyMap);
            _styles.Add(style);
            return style;
        }

        public Style Add(Type targetType, string targetId, Type? baseType, Dictionary<string, string> propertyMap)
        {
            var style = new Style(GetStylesheet(), targetType: targetType, applyForType: baseType, targetId: targetId, propertyMap);
            _styles.Add(style);
            return style;
        }

        private protected abstract Stylesheet GetStylesheet();
    }
}

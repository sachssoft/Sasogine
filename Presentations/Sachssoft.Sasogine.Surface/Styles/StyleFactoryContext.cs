using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public class StyleFactoryContext<T> where T : class
    {
        private readonly Dictionary<string, string> _map = new();

        internal StyleFactoryContext(Style style, Dictionary<string, string> map)
        {
            Style = style;
            _map = map;
        }

        public Style Style { get; }

        public IEnumerable<string> Properties => _map.Keys;

        public PropertyValue GetValue(string key)
        {
            return new PropertyValue(_map[key]);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out PropertyValue value)
        {
            if (_map.TryGetValue(key, out var str))
            {
                value = new PropertyValue(str);
                return true;
            }

            value = null;
            return false;
        }

    }
}

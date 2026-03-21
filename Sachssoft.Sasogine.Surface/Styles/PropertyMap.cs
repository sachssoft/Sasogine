using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public sealed class PropertyMap
    {
        private Dictionary<string, string?> _properties = new();

        public PropertyMap() { }

        public PropertyMap(Dictionary<string, string?> properties)
        {
            _properties = properties;
        }

        public void Add(string name, string? value)
        {
            _properties[name] = value;
        }

        public PropertyValue Get(string name, string? fallback = null)
        {
            if (_properties.TryGetValue(name, out var value))
            {
                return new PropertyValue(_properties[name]);
            }

            return new PropertyValue(fallback);
        }
    }
}

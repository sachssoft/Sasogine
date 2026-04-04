using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public sealed class PropertySet : IEnumerable<(string Name, object? Value)>
    {
        private readonly IReadOnlyDictionary<string, object?> _properties;

        public PropertySet()
        {
            _properties = Array.Empty<KeyValuePair<string, object?>>()
                               .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public PropertySet(Dictionary<string, object?> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            // Kopiert und als readonly gespeichert
            _properties = new Dictionary<string, object?>(properties);
        }

        public PropertySet(IEnumerable<(string Name, object? Value)> properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            _properties = properties.ToDictionary(p => p.Name, p => p.Value);
        }

        /// <summary>
        /// Liefert den Wert eines Properties, optional mit Fallback.
        /// </summary>
        public T? Get<T>(string name, T? fallback = default)
        {
            if (string.IsNullOrEmpty(name))
                return fallback;

            return _properties.TryGetValue(name, out var value) && value is T typedValue
                ? typedValue
                : fallback;
        }

        /// <summary>
        /// Versucht einen Wert abzurufen, ohne Exception zu werfen.
        /// </summary>
        public bool TryGet<T>(string name, out T? value)
        {
            value = default;
            if (string.IsNullOrEmpty(name))
                return false;

            if (_properties.TryGetValue(name, out var obj) && obj is T typed)
            {
                value = typed;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Liefert alle Keys.
        /// </summary>
        public IEnumerable<string> Keys => _properties.Keys;

        /// <summary>
        /// Liefert alle Values.
        /// </summary>
        public IEnumerable<object?> Values => _properties.Values;

        /// <summary>
        /// Generischer Enumerator für foreach.
        /// </summary>
        public IEnumerator<(string Name, object? Value)> GetEnumerator()
        {
            foreach (var kvp in _properties)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Nicht-generischer Enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
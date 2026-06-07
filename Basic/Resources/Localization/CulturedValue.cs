using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Resources.Localization
{
    public sealed class CulturedValue<T>
    {
        private readonly Dictionary<string, T> _values;
        private readonly T? _fallback;

        public CulturedValue(
            IDictionary<CultureInfo, T>? values = null,
            T? fallback = default)
        {
            _values = values != null
                ? values.ToDictionary(k => k.Key.Name, v => v.Value)
                : new Dictionary<string, T>();

            _fallback = fallback;
        }

        // IMMUTABLE "MODIFIER" METHOD
        public CulturedValue<T> With(CultureInfo culture, T value)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));

            var copy = new Dictionary<string, T>(_values)
            {
                [culture.Name] = value
            };

            return new CulturedValue<T>(
                copy.ToDictionary(k => new CultureInfo(k.Key), v => v.Value),
                _fallback
            );
        }

        public CulturedValue<T> Without(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));

            var copy = new Dictionary<string, T>(_values);
            copy.Remove(culture.Name);

            return new CulturedValue<T>(
                copy.ToDictionary(k => new CultureInfo(k.Key), v => v.Value),
                _fallback
            );
        }

        public T? Get(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));

            if (_values.TryGetValue(culture.Name, out var value))
                return value;

            return _fallback;
        }

        public bool TryGet(CultureInfo culture, out T? value)
        {
            value = Get(culture);
            return value != null;
        }

        public IEnumerable<CultureInfo> Cultures
            => _values.Keys.Select(k => new CultureInfo(k));
    }
}
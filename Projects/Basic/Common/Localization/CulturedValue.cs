using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Common.Localization
{
    /// <summary>
    /// Represents an immutable value that can provide different values
    /// depending on the requested culture.
    ///
    /// A fallback value is returned when no value exists for the requested culture.
    /// Modified instances can be created using <see cref="With"/> and
    /// <see cref="Without"/> without changing the original instance.
    /// </summary>
    /// <typeparam name="T">Type of the localized value.</typeparam>
    public sealed class CulturedValue<T>
    {
        private readonly Dictionary<string, T> _values;
        private readonly T? _fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CulturedValue{T}"/> class.
        /// </summary>
        /// <param name="values">
        /// Initial culture-specific values.
        /// </param>
        /// <param name="fallback">
        /// Value returned when no value exists for the requested culture.
        /// </param>
        public CulturedValue(
            IDictionary<CultureInfo, T>? values = null,
            T? fallback = default)
        {
            _values = values != null
                ? values.ToDictionary(
                    static pair => pair.Key.Name,
                    static pair => pair.Value,
                    StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            _fallback = fallback;
        }

        private CulturedValue(
            Dictionary<string, T> values,
            T? fallback)
        {
            _values = values;
            _fallback = fallback;
        }

        /// <summary>
        /// Gets the fallback value used when no culture-specific value exists.
        /// </summary>
        public T? Fallback => _fallback;

        /// <summary>
        /// Gets the cultures for which values are explicitly defined.
        /// </summary>
        public IEnumerable<CultureInfo> Cultures =>
            _values.Keys.Select(static name => new CultureInfo(name));

        /// <summary>
        /// Creates a new instance containing the specified value for a culture.
        /// The current instance remains unchanged.
        /// </summary>
        /// <param name="culture">Culture to associate with the value.</param>
        /// <param name="value">Value associated with the culture.</param>
        /// <returns>A new instance containing the specified culture and value.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="culture"/> is null.
        /// </exception>
        public CulturedValue<T> With(CultureInfo culture, T value)
        {
            ArgumentNullException.ThrowIfNull(culture);

            var copy = new Dictionary<string, T>(
                _values,
                StringComparer.OrdinalIgnoreCase)
            {
                [culture.Name] = value
            };

            return new CulturedValue<T>(copy, _fallback);
        }

        /// <summary>
        /// Creates a new instance without the value associated with
        /// the specified culture.
        /// The current instance remains unchanged.
        /// </summary>
        /// <param name="culture">Culture to remove.</param>
        /// <returns>A new instance without the specified culture.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="culture"/> is null.
        /// </exception>
        public CulturedValue<T> Without(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            var copy = new Dictionary<string, T>(
                _values,
                StringComparer.OrdinalIgnoreCase);

            copy.Remove(culture.Name);

            return new CulturedValue<T>(copy, _fallback);
        }

        /// <summary>
        /// Gets the value associated with the specified culture.
        /// Returns the fallback value when no culture-specific value exists.
        /// </summary>
        /// <param name="culture">Culture whose value should be retrieved.</param>
        /// <returns>
        /// The culture-specific value when available; otherwise the fallback value.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="culture"/> is null.
        /// </exception>
        public T? Get(CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(culture);

            return _values.TryGetValue(culture.Name, out var value)
                ? value
                : _fallback;
        }

        /// <summary>
        /// Attempts to get a value explicitly associated with the specified culture.
        /// </summary>
        /// <param name="culture">Culture whose value should be retrieved.</param>
        /// <param name="value">
        /// When this method returns, contains the culture-specific value when found;
        /// otherwise the fallback value.
        /// </param>
        /// <returns>
        /// True if a culture-specific value exists; otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="culture"/> is null.
        /// </exception>
        public bool TryGet(CultureInfo culture, out T? value)
        {
            ArgumentNullException.ThrowIfNull(culture);

            if (_values.TryGetValue(culture.Name, out var culturedValue))
            {
                value = culturedValue;
                return true;
            }

            value = _fallback;
            return false;
        }
    }
}
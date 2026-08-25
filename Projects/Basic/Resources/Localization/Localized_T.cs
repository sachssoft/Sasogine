using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;


namespace Sachssoft.Sasogine.Resources.Localization
{
    public record LocalizedValue<T> : ILocalizedValue
    {
        public LocalizedValue() { }

        [SetsRequiredMembers]
        public LocalizedValue(string key, T? fallback = default)
        {
            Key = key;
            Fallback = fallback;
        }

        public required string Key { get; init; }

        public T? Fallback { get; init; }

        object? ILocalizedValue.Fallback => Fallback;

        public override string ToString()
        {
            if (Fallback == null)
                return $"{Key} => null";

            // Wenn der Wert formattable ist (Zahlen, Datum, etc.), invariant formatieren
            if (Fallback is IFormattable formattable)
                return $"{Key} => {formattable.ToString(null, CultureInfo.InvariantCulture)}";

            // Sonst einfach ToString()
            return $"{Key} => {Fallback}";
        }
    }
}
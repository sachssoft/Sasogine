using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Resources.Localization
{
    public class LocalizedString : ILocalizedEntry
    {
        private string? _singleValue;
        private IReadOnlyDictionary<LocalizationPluralCase, string?>? _pluralCases;
        private bool _isLoaded = false;

        public bool IsLoaded => _isLoaded;

        public void Load(CultureInfo culture, GameResourceManager resourceManager, LocalizedEntryData data)
        {
            data.Attributes.TryGetValue("Value", out _singleValue);

            _pluralCases = data.PluralCases;
            _isLoaded = true;
        }

        public object? GetValue(int count)
        {
            if (_pluralCases != null)
            {
                // PluralCase auswählen
                LocalizationPluralCase caseToUse = count switch
                {
                    0 when _pluralCases.ContainsKey(LocalizationPluralCase.Zero) => LocalizationPluralCase.Zero,
                    1 when _pluralCases.ContainsKey(LocalizationPluralCase.One) => LocalizationPluralCase.One,
                    2 when _pluralCases.ContainsKey(LocalizationPluralCase.Two) => LocalizationPluralCase.Two,
                    >= 3 and <= 4 when _pluralCases.ContainsKey(LocalizationPluralCase.Few) => LocalizationPluralCase.Few,
                    >= 5 when _pluralCases.ContainsKey(LocalizationPluralCase.Many) => LocalizationPluralCase.Many,
                    _ => LocalizationPluralCase.Default
                };

                if (_pluralCases.TryGetValue(caseToUse, out var val))
                    return val;
            }

            return _singleValue;
        }
    }

}

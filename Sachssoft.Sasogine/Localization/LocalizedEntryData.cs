using System.Collections.Generic;

namespace Sachssoft.Sasogine.Localization
{
    public record LocalizedEntryData
    {
        public required string Id { get; init; }

        public string? Content { get; init; }

        public required Dictionary<string, string?> Attributes { get; init; }

        public required Dictionary<LocalizationPluralCase, string?> PluralCases { get; init; }

    }
}

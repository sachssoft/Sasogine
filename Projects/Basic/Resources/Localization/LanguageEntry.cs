using Sachssoft.Engine.Assets;
using System;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Contains localization data for a language.
/// </summary>
public sealed class LanguageEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageEntry"/> class.
    /// </summary>
    /// <param name="language">
    /// The language associated with this entry.
    /// </param>
    /// <param name="dictionary">
    /// The localized string dictionary associated with the language.
    /// </param>
    /// <param name="assets">
    /// The localized asset store associated with the language.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="language"/>, <paramref name="dictionary"/>,
    /// or <paramref name="assets"/> is <see langword="null"/>.
    /// </exception>
    public LanguageEntry(
        Language language,
        LocalizedDictionary dictionary,
        AssetStore assets)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(assets);

        Language = language;
        Dictionary = dictionary;
        Assets = assets;
    }

    /// <summary>
    /// Gets the language associated with this entry.
    /// </summary>
    public Language Language { get; }

    /// <summary>
    /// Gets the localized string dictionary associated with the language.
    /// </summary>
    public LocalizedDictionary Dictionary { get; }

    /// <summary>
    /// Gets the localized asset store associated with the language.
    /// </summary>
    public AssetStore Assets { get; }

    /// <summary>
    /// Gets the plural case for the specified quantity.
    /// </summary>
    /// <param name="quantity">
    /// The quantity for which to determine the plural case.
    /// </param>
    /// <returns>
    /// The plural case corresponding to the specified quantity according
    /// to the language-specific pluralization rule.
    /// </returns>
    public LocalizationPluralCase GetPluralCase(int quantity)
    {
        return Language.GetPluralCase(quantity);
    }
}
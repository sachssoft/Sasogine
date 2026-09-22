using System;
using System.Globalization;

namespace Sachssoft.Engine.Resources.Localization;

/// <summary>
/// Represents a language and its pluralization rule.
/// </summary>
public sealed class Language
{
    private readonly Func<int, LocalizationPluralCase> _pluralSelector;

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    /// <param name="name">
    /// The language code, such as <c>de</c>, <c>en</c>, or <c>ru</c>.
    /// </param>
    /// <param name="pluralSelector">
    /// The function used to determine the plural case for a quantity.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty,
    /// or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pluralSelector"/> is <see langword="null"/>.
    /// </exception>
    public Language(
        string name,
        Func<int, LocalizationPluralCase> pluralSelector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pluralSelector);

        Name = name;
        _pluralSelector = pluralSelector;
    }

    /// <summary>
    /// Gets the language code.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the plural case for the specified quantity.
    /// </summary>
    /// <param name="quantity">
    /// The quantity for which to determine the plural case.
    /// </param>
    /// <returns>
    /// The plural case corresponding to the specified quantity.
    /// </returns>
    public LocalizationPluralCase GetPluralCase(int quantity)
    {
        return _pluralSelector(quantity);
    }

    /// <summary>
    /// Finds a built-in language by its full name or language code.
    /// </summary>
    /// <param name="name">
    /// The full language name or language code.
    /// </param>
    /// <returns>
    /// The matching language, or <see langword="null"/> if no matching language
    /// is registered.
    /// </returns>
    public static Language? Find(string? name)
    {
        return Languages.Find(name);
    }

    /// <summary>
    /// Finds a built-in language corresponding to the specified culture.
    /// </summary>
    /// <param name="culture">
    /// The culture used to determine the language.
    /// </param>
    /// <returns>
    /// The matching language, or <see langword="null"/> if no matching language
    /// is registered.
    /// </returns>
    public static Language? Find(CultureInfo? culture)
    {
        if (culture is null)
            return null;

        return Find(culture.TwoLetterISOLanguageName);
    }

    /// <summary>
    /// Attempts to get a built-in language by its full name or language code.
    /// </summary>
    /// <param name="name">
    /// The full language name or language code.
    /// </param>
    /// <param name="language">
    /// When this method returns, contains the matching language when found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching language was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryGet(
        string? name,
        out Language? language)
    {
        language = Find(name);
        return language is not null;
    }

    /// <summary>
    /// Attempts to get a built-in language corresponding to the specified culture.
    /// </summary>
    /// <param name="culture">
    /// The culture used to determine the language.
    /// </param>
    /// <param name="language">
    /// When this method returns, contains the matching language when found;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching language was found; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool TryGet(
        CultureInfo? culture,
        out Language? language)
    {
        language = Find(culture);
        return language is not null;
    }
}
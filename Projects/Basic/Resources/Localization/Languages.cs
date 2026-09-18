using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Provides built-in definitions for supported European languages.
/// </summary>
public static class Languages
{
    private static readonly Dictionary<string, Language> _languages =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Func<int, LocalizationPluralCase>
        DefaultPluralSelector =
            quantity => quantity == 1
                ? LocalizationPluralCase.One
                : LocalizationPluralCase.Default;

    private static readonly Func<int, LocalizationPluralCase>
        FrenchPluralSelector =
            quantity => quantity is 0 or 1
                ? LocalizationPluralCase.One
                : LocalizationPluralCase.Default;

    private static readonly Func<int, LocalizationPluralCase>
        RussianPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);
                int mod10 = value % 10;
                int mod100 = value % 100;

                if (mod10 == 1 && mod100 != 11)
                    return LocalizationPluralCase.One;

                if (mod10 is >= 2 and <= 4 &&
                    mod100 is not (>= 12 and <= 14))
                {
                    return LocalizationPluralCase.Few;
                }

                return LocalizationPluralCase.Many;
            };

    private static readonly Func<int, LocalizationPluralCase>
        PolishPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);
                int mod10 = value % 10;
                int mod100 = value % 100;

                if (value == 1)
                    return LocalizationPluralCase.One;

                if (mod10 is >= 2 and <= 4 &&
                    mod100 is not (>= 12 and <= 14))
                {
                    return LocalizationPluralCase.Few;
                }

                return LocalizationPluralCase.Many;
            };

    private static readonly Func<int, LocalizationPluralCase>
        CzechPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);

                return value switch
                {
                    1 => LocalizationPluralCase.One,
                    >= 2 and <= 4 => LocalizationPluralCase.Few,
                    _ => LocalizationPluralCase.Default
                };
            };

    private static readonly Func<int, LocalizationPluralCase>
        SlovenianPluralSelector =
            quantity =>
            {
                int mod100 = Math.Abs(quantity) % 100;

                return mod100 switch
                {
                    1 => LocalizationPluralCase.One,
                    2 => LocalizationPluralCase.Two,
                    3 or 4 => LocalizationPluralCase.Few,
                    _ => LocalizationPluralCase.Default
                };
            };

    private static readonly Func<int, LocalizationPluralCase>
        LithuanianPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);
                int mod10 = value % 10;
                int mod100 = value % 100;

                if (mod10 == 1 &&
                    mod100 is not (>= 11 and <= 19))
                {
                    return LocalizationPluralCase.One;
                }

                if (mod10 is >= 2 and <= 9 &&
                    mod100 is not (>= 11 and <= 19))
                {
                    return LocalizationPluralCase.Few;
                }

                return LocalizationPluralCase.Default;
            };

    private static readonly Func<int, LocalizationPluralCase>
        LatvianPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);
                int mod10 = value % 10;
                int mod100 = value % 100;

                if (mod10 == 0 ||
                    mod100 is >= 11 and <= 19)
                {
                    return LocalizationPluralCase.Zero;
                }

                if (mod10 == 1 && mod100 != 11)
                    return LocalizationPluralCase.One;

                return LocalizationPluralCase.Default;
            };

    private static readonly Func<int, LocalizationPluralCase>
        RomanianPluralSelector =
            quantity =>
            {
                int value = Math.Abs(quantity);
                int mod100 = value % 100;

                if (value == 1)
                    return LocalizationPluralCase.One;

                if (value == 0 ||
                    mod100 is >= 1 and <= 19)
                {
                    return LocalizationPluralCase.Few;
                }

                return LocalizationPluralCase.Default;
            };

    /// <summary>
    /// Gets the German language definition.
    /// </summary>
    public static Language German { get; } =
        Register("German", "de", DefaultPluralSelector);

    /// <summary>
    /// Gets the English language definition.
    /// </summary>
    public static Language English { get; } =
        Register("English", "en", DefaultPluralSelector);

    /// <summary>
    /// Gets the Spanish language definition.
    /// </summary>
    public static Language Spanish { get; } =
        Register("Spanish", "es", DefaultPluralSelector);

    /// <summary>
    /// Gets the Italian language definition.
    /// </summary>
    public static Language Italian { get; } =
        Register("Italian", "it", DefaultPluralSelector);

    /// <summary>
    /// Gets the Portuguese language definition.
    /// </summary>
    public static Language Portuguese { get; } =
        Register("Portuguese", "pt", DefaultPluralSelector);

    /// <summary>
    /// Gets the Dutch language definition.
    /// </summary>
    public static Language Dutch { get; } =
        Register("Dutch", "nl", DefaultPluralSelector);

    /// <summary>
    /// Gets the Swedish language definition.
    /// </summary>
    public static Language Swedish { get; } =
        Register("Swedish", "sv", DefaultPluralSelector);

    /// <summary>
    /// Gets the Norwegian language definition.
    /// </summary>
    public static Language Norwegian { get; } =
        Register("Norwegian", "no", DefaultPluralSelector);

    /// <summary>
    /// Gets the Danish language definition.
    /// </summary>
    public static Language Danish { get; } =
        Register("Danish", "da", DefaultPluralSelector);

    /// <summary>
    /// Gets the Finnish language definition.
    /// </summary>
    public static Language Finnish { get; } =
        Register("Finnish", "fi", DefaultPluralSelector);

    /// <summary>
    /// Gets the Greek language definition.
    /// </summary>
    public static Language Greek { get; } =
        Register("Greek", "el", DefaultPluralSelector);

    /// <summary>
    /// Gets the Bulgarian language definition.
    /// </summary>
    public static Language Bulgarian { get; } =
        Register("Bulgarian", "bg", DefaultPluralSelector);

    /// <summary>
    /// Gets the Estonian language definition.
    /// </summary>
    public static Language Estonian { get; } =
        Register("Estonian", "et", DefaultPluralSelector);

    /// <summary>
    /// Gets the Hungarian language definition.
    /// </summary>
    public static Language Hungarian { get; } =
        Register("Hungarian", "hu", DefaultPluralSelector);

    /// <summary>
    /// Gets the French language definition.
    /// </summary>
    public static Language French { get; } =
        Register("French", "fr", FrenchPluralSelector);

    /// <summary>
    /// Gets the Russian language definition.
    /// </summary>
    public static Language Russian { get; } =
        Register("Russian", "ru", RussianPluralSelector);

    /// <summary>
    /// Gets the Ukrainian language definition.
    /// </summary>
    public static Language Ukrainian { get; } =
        Register("Ukrainian", "uk", RussianPluralSelector);

    /// <summary>
    /// Gets the Belarusian language definition.
    /// </summary>
    public static Language Belarusian { get; } =
        Register("Belarusian", "be", RussianPluralSelector);

    /// <summary>
    /// Gets the Polish language definition.
    /// </summary>
    public static Language Polish { get; } =
        Register("Polish", "pl", PolishPluralSelector);

    /// <summary>
    /// Gets the Czech language definition.
    /// </summary>
    public static Language Czech { get; } =
        Register("Czech", "cs", CzechPluralSelector);

    /// <summary>
    /// Gets the Slovak language definition.
    /// </summary>
    public static Language Slovak { get; } =
        Register("Slovak", "sk", CzechPluralSelector);

    /// <summary>
    /// Gets the Slovenian language definition.
    /// </summary>
    public static Language Slovenian { get; } =
        Register("Slovenian", "sl", SlovenianPluralSelector);

    /// <summary>
    /// Gets the Lithuanian language definition.
    /// </summary>
    public static Language Lithuanian { get; } =
        Register("Lithuanian", "lt", LithuanianPluralSelector);

    /// <summary>
    /// Gets the Latvian language definition.
    /// </summary>
    public static Language Latvian { get; } =
        Register("Latvian", "lv", LatvianPluralSelector);

    /// <summary>
    /// Gets the Romanian language definition.
    /// </summary>
    public static Language Romanian { get; } =
        Register("Romanian", "ro", RomanianPluralSelector);

    /// <summary>
    /// Finds a language by its full name or language code.
    /// </summary>
    /// <param name="name">
    /// The full language name or language code.
    /// </param>
    /// <returns>
    /// The matching language, or <see langword="null"/> if no matching
    /// language is registered.
    /// </returns>
    public static Language? Find(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return _languages.TryGetValue(
            name,
            out Language? language)
                ? language
                : null;
    }

    /// <summary>
    /// Attempts to find a language by its full name or language code.
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
    public static bool TryFind(
        string? name,
        out Language? language)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            language = null;
            return false;
        }

        return _languages.TryGetValue(name, out language);
    }

    /// <summary>
    /// Registers a built-in language using its full name and language code.
    /// </summary>
    /// <param name="fullName">
    /// The full language name.
    /// </param>
    /// <param name="name">
    /// The language code.
    /// </param>
    /// <param name="pluralSelector">
    /// The pluralization rule used by the language.
    /// </param>
    /// <returns>
    /// The registered language.
    /// </returns>
    private static Language Register(
        string fullName,
        string name,
        Func<int, LocalizationPluralCase> pluralSelector)
    {
        var language = new Language(
            name,
            pluralSelector);

        _languages.Add(fullName, language);
        _languages.Add(name, language);

        return language;
    }
}
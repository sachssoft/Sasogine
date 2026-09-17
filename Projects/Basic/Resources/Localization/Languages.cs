using System;

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Provides built-in definitions for supported European languages.
/// </summary>
public static class Languages
{
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
        new("de", DefaultPluralSelector);

    /// <summary>
    /// Gets the English language definition.
    /// </summary>
    public static Language English { get; } =
        new("en", DefaultPluralSelector);

    /// <summary>
    /// Gets the Spanish language definition.
    /// </summary>
    public static Language Spanish { get; } =
        new("es", DefaultPluralSelector);

    /// <summary>
    /// Gets the Italian language definition.
    /// </summary>
    public static Language Italian { get; } =
        new("it", DefaultPluralSelector);

    /// <summary>
    /// Gets the Portuguese language definition.
    /// </summary>
    public static Language Portuguese { get; } =
        new("pt", DefaultPluralSelector);

    /// <summary>
    /// Gets the Dutch language definition.
    /// </summary>
    public static Language Dutch { get; } =
        new("nl", DefaultPluralSelector);

    /// <summary>
    /// Gets the Swedish language definition.
    /// </summary>
    public static Language Swedish { get; } =
        new("sv", DefaultPluralSelector);

    /// <summary>
    /// Gets the Norwegian language definition.
    /// </summary>
    public static Language Norwegian { get; } =
        new("no", DefaultPluralSelector);

    /// <summary>
    /// Gets the Danish language definition.
    /// </summary>
    public static Language Danish { get; } =
        new("da", DefaultPluralSelector);

    /// <summary>
    /// Gets the Finnish language definition.
    /// </summary>
    public static Language Finnish { get; } =
        new("fi", DefaultPluralSelector);

    /// <summary>
    /// Gets the Greek language definition.
    /// </summary>
    public static Language Greek { get; } =
        new("el", DefaultPluralSelector);

    /// <summary>
    /// Gets the Bulgarian language definition.
    /// </summary>
    public static Language Bulgarian { get; } =
        new("bg", DefaultPluralSelector);

    /// <summary>
    /// Gets the Estonian language definition.
    /// </summary>
    public static Language Estonian { get; } =
        new("et", DefaultPluralSelector);

    /// <summary>
    /// Gets the Hungarian language definition.
    /// </summary>
    public static Language Hungarian { get; } =
        new("hu", DefaultPluralSelector);

    /// <summary>
    /// Gets the French language definition.
    /// </summary>
    public static Language French { get; } =
        new("fr", FrenchPluralSelector);

    /// <summary>
    /// Gets the Russian language definition.
    /// </summary>
    public static Language Russian { get; } =
        new("ru", RussianPluralSelector);

    /// <summary>
    /// Gets the Ukrainian language definition.
    /// </summary>
    public static Language Ukrainian { get; } =
        new("uk", RussianPluralSelector);

    /// <summary>
    /// Gets the Belarusian language definition.
    /// </summary>
    public static Language Belarusian { get; } =
        new("be", RussianPluralSelector);

    /// <summary>
    /// Gets the Polish language definition.
    /// </summary>
    public static Language Polish { get; } =
        new("pl", PolishPluralSelector);

    /// <summary>
    /// Gets the Czech language definition.
    /// </summary>
    public static Language Czech { get; } =
        new("cs", CzechPluralSelector);

    /// <summary>
    /// Gets the Slovak language definition.
    /// </summary>
    public static Language Slovak { get; } =
        new("sk", CzechPluralSelector);

    /// <summary>
    /// Gets the Slovenian language definition.
    /// </summary>
    public static Language Slovenian { get; } =
        new("sl", SlovenianPluralSelector);

    /// <summary>
    /// Gets the Lithuanian language definition.
    /// </summary>
    public static Language Lithuanian { get; } =
        new("lt", LithuanianPluralSelector);

    /// <summary>
    /// Gets the Latvian language definition.
    /// </summary>
    public static Language Latvian { get; } =
        new("lv", LatvianPluralSelector);

    /// <summary>
    /// Gets the Romanian language definition.
    /// </summary>
    public static Language Romanian { get; } =
        new("ro", RomanianPluralSelector);
}
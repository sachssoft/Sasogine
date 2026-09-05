using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Provides extension methods for formatting gameplay-related values
/// for display purposes.
/// </summary>
public static class DisplayExtensions
{
    /// <summary>
    /// Converts the specified countdown duration to a formatted countdown string.
    /// </summary>
    /// <param name="countdown">
    /// The countdown duration to format.
    /// </param>
    /// <param name="style">
    /// The formatting style used to determine which time units are displayed.
    /// </param>
    /// <returns>
    /// A formatted countdown string.
    /// </returns>
    public static string ToCountdownString(
        this TimeSpan countdown,
        CountdownStyle style = CountdownStyle.Full)
    {
        int totalDays = countdown.Days;
        int years = totalDays / 365;
        int weeks = totalDays % 365 / 7;
        int days = totalDays % 7;

        int hours = countdown.Hours;
        int minutes = countdown.Minutes;
        int seconds = countdown.Seconds;

        var units = new List<(int value, string label)>
        {
            (years, "Y"),
            (weeks, "W"),
            (days, "D"),
            (hours, "H"),
            (minutes, "M"),
            (seconds, "S"),
        };

        IEnumerable<(int value, string label)> filteredUnits;

        switch (style)
        {
            case CountdownStyle.Full:
                filteredUnits = units.Where(u => u.value > 0);
                break;

            case CountdownStyle.FullWithZeros:
                filteredUnits = units;
                break;

            case CountdownStyle.Compact:
                filteredUnits = units.Where(u => u.value > 0).Take(2);
                break;

            case CountdownStyle.CompactWithZeros:
                filteredUnits = units.Take(2);
                break;

            default:
                filteredUnits = units.Where(u => u.value > 0);
                break;
        }

        if (!filteredUnits.Any())
            return "0S";

        return string.Join(
            " ",
            filteredUnits.Select(u => $"{u.value}{u.label}"));
    }

    /// <summary>
    /// Converts the specified floating-point value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="number">
    /// The value to format.
    /// </param>
    /// <param name="style">
    /// The compact number formatting style.
    /// </param>
    /// <param name="culture">
    /// The culture used for number formatting and localized suffixes.
    /// If <see langword="null"/>, <see cref="CultureInfo.CurrentUICulture"/>
    /// is used.
    /// </param>
    /// <returns>
    /// A compact string representation of the specified value.
    /// </returns>
    public static string ToCompactString(
        this float number,
        CompactNumberStyle style,
        CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentUICulture;

        float abs = float.Abs(number);
        float divisor;
        string suffix;

        switch (style)
        {
            case CompactNumberStyle.Casual:
                (divisor, suffix) = abs switch
                {
                    >= 1e12f => (1e12f, "TAsset"),
                    >= 1e9f => (1e9f, "B"),
                    >= 1e6f => (1e6f, "M"),
                    >= 1e3f => (1e3f, "k"),
                    _ => (1, "")
                };
                break;

            case CompactNumberStyle.Technical:
                (divisor, suffix) = abs switch
                {
                    >= 1e12f => (1e12f, "TAsset"),
                    >= 1e9f => (1e9f, "G"),
                    >= 1e6f => (1e6f, "M"),
                    >= 1e3f => (1e3f, "K"),
                    _ => (1, "")
                };
                break;

            case CompactNumberStyle.Local:
                (divisor, suffix) = abs switch
                {
                    >= 1e12f => (1e12f, LocalSuffix("TAsset", "Bio.", culture)),
                    >= 1e9f => (1e9f, LocalSuffix("B", "Mrd.", culture)),
                    >= 1e6f => (1e6f, LocalSuffix("M", "Mio.", culture)),
                    >= 1e3f => (1e3f, LocalSuffix("k", "Tsd.", culture)),
                    _ => (1, "")
                };
                break;

            default:
                (divisor, suffix) = (1, "");
                break;
        }

        double compact = number / divisor;
        string formatted = compact.ToString("0.#", culture);

        return $"{formatted}{suffix}".Trim();
    }

    /// <summary>
    /// Converts the specified integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this int value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified long integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this long value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified short integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this short value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified signed byte value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this sbyte value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified unsigned integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this uint value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified unsigned long integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this ulong value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified unsigned short integer value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this ushort value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    /// <summary>
    /// Converts the specified byte value to a compact
    /// human-readable number representation.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="style">The compact number formatting style.</param>
    /// <param name="culture">
    /// The culture used for formatting, or <see langword="null"/>
    /// to use the current UI culture.
    /// </param>
    /// <returns>A compact string representation of the value.</returns>
    public static string ToCompactString(
        this byte value,
        CompactNumberStyle style,
        CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    private static string LocalSuffix(
        string fallback,
        string german,
        CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "de" => german,
            "ru" => MapToCyrillicSuffix(german),
            "uk" => MapToCyrillicSuffix(german),
            "be" => MapToCyrillicSuffix(german),
            _ => fallback
        };
    }

    private static string MapToCyrillicSuffix(string german)
    {
        return german switch
        {
            "Tsd." => "тыс.",
            "Mio." => "млн",
            "Mrd." => "млрд",
            "Bio." => "трлн",
            _ => german
        };
    }
}
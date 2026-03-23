using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Gameplay;

public static class DisplayExtensions
{
    public static string ToCountdownString(this TimeSpan countdown, CountdownStyle style = CountdownStyle.Full)
    {
        int totalDays = countdown.Days;
        int years = totalDays / 365;
        int weeks = totalDays % 365 / 7;
        int days = totalDays % 7;

        int hours = countdown.Hours;
        int minutes = countdown.Minutes;
        int seconds = countdown.Seconds;

        // Liste aller Einheiten mit Label in Reihenfolge
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
                // nur Werte > 0
                filteredUnits = units.Where(u => u.value > 0);
                break;

            case CountdownStyle.FullWithZeros:
                // alle Werte, auch 0
                filteredUnits = units;
                break;

            case CountdownStyle.Compact:
                // max 2 Werte, nur > 0
                filteredUnits = units.Where(u => u.value > 0).Take(2);
                break;

            case CountdownStyle.CompactWithZeros:
                // max 2 Werte inkl. 0: einfach die ersten 2 Einheiten immer anzeigen
                filteredUnits = units.Take(2);
                break;

            default:
                filteredUnits = units.Where(u => u.value > 0);
                break;
        }

        // Falls nichts angezeigt wird (z.B. 0s bei Full), zeigen wir mindestens "0S"
        if (!filteredUnits.Any())
            return "0S";

        // Baue String
        return string.Join(" ", filteredUnits.Select(u => $"{u.value}{u.label}"));
    }


    public static string ToCompactString(this float number, CompactNumberStyle style, CultureInfo? culture = null)
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

    private static string LocalSuffix(string fallback, string german, CultureInfo culture)
    {
        return culture.TwoLetterISOLanguageName switch
        {
            "de" => german,

            // Russisch, Ukrainisch, Belarussisch
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

    public static string ToCompactString(this int value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this long value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this short value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this sbyte value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this uint value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this ulong value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this ushort value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);

    public static string ToCompactString(this byte value, CompactNumberStyle style, CultureInfo? culture = null) =>
        ((float)value).ToCompactString(style, culture);
}

namespace Sachssoft.Sasogine.Resources.Localization;

/// <summary>
/// Specifies the plural categories used to select localized
/// grammatical forms according to language-specific plural rules.
/// </summary>
/// <remarks>
/// The categories correspond to the plural categories defined by
/// the Unicode CLDR plural rules. Individual languages may use only
/// a subset of these categories and may assign them according to
/// different numeric rules.
/// </remarks>
public enum LocalizationPluralCase
{
    // Null-Menge (0)
    // Beispiel: Arabisch hat eine spezielle Form für Null

    /// <summary>
    /// Represents the <c>zero</c> plural category, used by languages
    /// that provide a distinct grammatical form for certain zero values.
    /// </summary>
    Zero,

    // Einzahl (1)
    // Beispiel: Englisch: "1 cat", Deutsch: "1 Katze"

    /// <summary>
    /// Represents the <c>one</c> plural category, commonly used for
    /// singular grammatical forms.
    /// </summary>
    One,

    // Dual (2) – nur in einigen Sprachen wie Arabisch oder Slowenisch
    // Beispiel Arabisch: "2 كتب" ("2 Bücher")

    /// <summary>
    /// Represents the <c>two</c> plural category, used by languages
    /// that provide a distinct grammatical form for certain values
    /// associated with two.
    /// </summary>
    Two,

    // Wenige – kleine Zahlen, sprachabhängig (oft 3–4)
    // Beispiel Arabisch: "3 كتب" ("3 Bücher")

    /// <summary>
    /// Represents the <c>few</c> plural category for values requiring
    /// a language-specific grammatical form for a small or otherwise
    /// specially classified quantity.
    /// </summary>
    Few,

    // Viele – große Zahlen, sprachabhängig
    // Beispiel Arabisch: "11 كتاباً" ("11 Bücher")

    /// <summary>
    /// Represents the <c>many</c> plural category for values requiring
    /// a language-specific grammatical form for quantities classified
    /// as many.
    /// </summary>
    Many,

    // Standard / Fallback – für alle Zahlen, die nicht von anderen Regeln abgedeckt sind
    // Entspricht CLDR "other", Standard-Pluralform in den meisten Sprachen
    // Beispiel Englisch Plural: "2 cats", "5 cats"

    /// <summary>
    /// Represents the default plural category used when no more
    /// specific plural category applies.
    /// </summary>
    /// <remarks>
    /// This corresponds to the CLDR <c>other</c> category and acts
    /// as the general fallback plural form.
    /// </remarks>
    Default,

    // eigener Wert zur Klarheit
    // Other ist dasselbe wie Default

    /// <summary>
    /// Represents the CLDR <c>other</c> plural category.
    /// This value is an alias of <see cref="Default"/>.
    /// </summary>
    Other = Default
}

/*
Beispiele der Pluralisierung nach Sprache (nach CLDR-Standard):

Deutsch (de):
    One:     1       -> Einzahl ("1 Kuh")
    Default: 0, 2, 3, ... -> Mehrzahl / allgemeiner Plural ("0 Kühe", "2 Kühe", "3 Kühe", ...)

Englisch (en):
    One:     1       -> Singular ("1 cow")
    Default: 0, 2, 3, ... -> Plural ("0 cows", "2 cows", "3 cows", ...)

Arabisch (ar):
    Zero:    0      -> Spezieller Fall für null
    One:     1      -> Einzahl
    Two:     2      -> Dual, z.B. zwei Objekte
    Few:     3–10   -> Kleiner Plural
    Many:    11–99  -> Großer Plural
    Default: 100+   -> Fallback, allgemeiner Plural

Slowenisch (sl):
    One:      1       -> Einzahl
    Two:      2       -> Dual
    Few:      3–4     -> Kleiner Plural
    Default:  0, 5+   -> Fallback / allgemeiner Plural

Hinweis: 
- "Default" wird immer genutzt, wenn keine andere Kategorie zutrifft oder die Sprache keine weiteren Fälle kennt.
- Die genauen Grenzen für "Few" oder "Many" hängen von der Sprache ab.
*/
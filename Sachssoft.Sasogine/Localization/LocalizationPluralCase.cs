using System;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Represents plural categories for localization.
    /// Based on CLDR plural rules:
    /// https://unicode-org.github.io/cldr-staging/charts/latest/supplemental/language_plural_rules.html
    /// </summary>
    public enum LocalizationPluralCase
    {
        // Null-Menge (0)
        // Beispiel: Arabisch hat eine spezielle Form für Null
        /// <summary>
        /// Zero quantity (0)
        /// </summary>
        Zero,

        // Einzahl (1)
        // Beispiel: Englisch: "1 cat", Deutsch: "1 Katze"
        /// <summary>
        /// Singular (1)
        /// </summary>
        One,

        // Dual (2) – nur in einigen Sprachen wie Arabisch oder Slowenisch
        // Beispiel Arabisch: "2 كتب" ("2 Bücher")
        /// <summary>
        /// Dual (2) – used in some languages like Arabic
        /// </summary>
        Two,

        // Wenige – kleine Zahlen, sprachabhängig (oft 3–4)
        // Beispiel Arabisch: "3 كتب" ("3 Bücher")
        /// <summary>
        /// Few – small numbers (3–4, language-dependent)
        /// </summary>
        Few,

        // Viele – große Zahlen, sprachabhängig
        // Beispiel Arabisch: "11 كتاباً" ("11 Bücher")
        /// <summary>
        /// Many – large numbers (language-dependent)
        /// </summary>
        Many,

        // Standard / Fallback – für alle Zahlen, die nicht von anderen Regeln abgedeckt sind
        // Entspricht CLDR "other", Standard-Pluralform in den meisten Sprachen
        // Beispiel Englisch Plural: "2 cats", "5 cats"
        /// <summary>
        /// Other / fallback for numbers not covered by other categories
        /// </summary>
        Default,         // eigener Wert zur Klarheit
        Other = Default  // Other ist dasselbe wie Default
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
}

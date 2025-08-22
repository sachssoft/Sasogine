namespace Sachssoft.Sasogine.Surface
{
    /// <summary>
    /// Defines formatting styles for compact number display,
    /// typically used in user interfaces to shorten large numbers.
    /// </summary>
    public enum CompactNumberStyle
    {
        /// <summary>
        /// Casual game style using common abbreviations:
        /// k (thousand), M (million), B (billion), T (trillion).
        /// Example: 1,200,000 → "1.2M"
        /// </summary>
        Casual,

        /// <summary>
        /// Technical style using SI unit prefixes:
        /// K (kilo), M (mega), G (giga), T (tera).
        /// Example: 1,200,000 → "1.2M"
        /// </summary>
        Technical,

        /// <summary>
        /// Localized style based on the current UI language.
        /// For example, German uses "Tsd.", "Mio.", "Mrd.", "Bio."
        /// instead of "k", "M", "B", "T".
        /// Example: 1,200,000 → "1.2 Mio." (in German)
        /// </summary>
        Local
    }
}

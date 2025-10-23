namespace Sachssoft.Sasogine.Gameplay;

public enum CountdownStyle
{
    /// <summary>
    /// Full style showing all non-zero time units.
    /// Example: "2W 4D 12H 11M 30S"
    /// </summary>
    Full,

    /// <summary>
    /// Full style including zero-value units.
    /// Example: "2W 4D 0H 0M 30S"
    /// </summary>
    FullWithZeros,

    /// <summary>
    /// Shortened style showing max two largest non-zero units.
    /// Example: "2W 4D"
    /// </summary>
    Compact,

    /// <summary>
    /// Shortened style showing max two units including zeros.
    /// Example: "2W 0D"
    /// </summary>
    CompactWithZeros
}
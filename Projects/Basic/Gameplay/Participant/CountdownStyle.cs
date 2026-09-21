namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Specifies how a countdown is formatted and which time units
/// are included in its textual representation.
/// </summary>
public enum CountdownStyle
{
    /// <summary>
    /// Displays all non-zero time units.
    /// For example, <c>2W 4D 12H 11M 30S</c>.
    /// </summary>
    Full,

    /// <summary>
    /// Displays all time units, including units whose value is zero.
    /// For example, <c>2W 4D 0H 0M 30S</c>.
    /// </summary>
    FullWithZeros,

    /// <summary>
    /// Displays at most the two largest non-zero time units.
    /// For example, <c>2W 4D</c>.
    /// </summary>
    Compact,

    /// <summary>
    /// Displays at most the two largest applicable time units,
    /// including units whose value is zero.
    /// For example, <c>2W 0D</c>.
    /// </summary>
    CompactWithZeros
}
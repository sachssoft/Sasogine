using System;

namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Defines a non-generic contract for tiered score thresholds.
/// </summary>
public interface ITieredScore
{
    /// <summary>
    /// Gets the value type used by the tiered score.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets the bronze threshold.
    /// </summary>
    object Bronze { get; }

    /// <summary>
    /// Gets the silver threshold.
    /// </summary>
    object Silver { get; }

    /// <summary>
    /// Gets the gold threshold.
    /// </summary>
    object Gold { get; }

    /// <summary>
    /// Creates a new tiered score using the specified threshold values.
    /// </summary>
    /// <param name="bronze">The bronze threshold.</param>
    /// <param name="silver">The silver threshold.</param>
    /// <param name="gold">The gold threshold.</param>
    /// <returns>
    /// A new tiered score containing the specified threshold values.
    /// </returns>
    ITieredScore WithValues(
        object bronze,
        object silver,
        object gold);

    /// <summary>
    /// Determines the tier achieved by the specified value.
    /// </summary>
    /// <param name="value">
    /// The value to evaluate.
    /// </param>
    /// <returns>
    /// The tier achieved by the specified value.
    /// </returns>
    TierResult GetResult(object value);
}
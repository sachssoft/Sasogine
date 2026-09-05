using System;

namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Represents tiered score thresholds where higher values produce better results.
/// </summary>
/// <typeparam name="TValue">
/// The value type used for score thresholds.
/// </typeparam>
/// <remarks>
/// The thresholds are normalized so that
/// <c>Bronze &lt;= Silver &lt;= Gold</c>.
/// A value must reach or exceed a threshold to achieve the corresponding tier.
/// </remarks>
public readonly struct HighTieredScore<TValue> : ITieredScore
    where TValue : struct, IComparable<TValue>
{
    /// <summary>
    /// The minimum value required to achieve the bronze tier.
    /// </summary>
    public readonly TValue Bronze;

    /// <summary>
    /// The minimum value required to achieve the silver tier.
    /// </summary>
    public readonly TValue Silver;

    /// <summary>
    /// The minimum value required to achieve the gold tier.
    /// </summary>
    public readonly TValue Gold;

    /// <summary>
    /// Initializes a new instance of the <see cref="HighTieredScore{TValue}"/> struct.
    /// </summary>
    /// <param name="bronze">
    /// The bronze threshold.
    /// </param>
    /// <param name="silver">
    /// The silver threshold.
    /// </param>
    /// <param name="gold">
    /// The gold threshold.
    /// </param>
    public HighTieredScore(
        TValue bronze,
        TValue silver,
        TValue gold)
    {
        Bronze = Max(bronze, Max(silver, gold));
        Silver = Clamp(silver, Min(silver, gold), Bronze);
        Gold = Min(gold, Min(bronze, silver));
    }

    /// <summary>
    /// Creates a new score definition with the specified gold threshold.
    /// </summary>
    /// <param name="gold">
    /// The new gold threshold.
    /// </param>
    /// <returns>
    /// A new tiered score containing the updated threshold.
    /// </returns>
    public HighTieredScore<TValue> ChangeGold(TValue gold)
    {
        return new HighTieredScore<TValue>(
            Bronze,
            Silver,
            gold);
    }

    /// <summary>
    /// Creates a new score definition with the specified silver threshold.
    /// </summary>
    /// <param name="silver">
    /// The new silver threshold.
    /// </param>
    /// <returns>
    /// A new tiered score containing the updated threshold.
    /// </returns>
    public HighTieredScore<TValue> ChangeSilver(TValue silver)
    {
        return new HighTieredScore<TValue>(
            Bronze,
            silver,
            Gold);
    }

    /// <summary>
    /// Creates a new score definition with the specified bronze threshold.
    /// </summary>
    /// <param name="bronze">
    /// The new bronze threshold.
    /// </param>
    /// <returns>
    /// A new tiered score containing the updated threshold.
    /// </returns>
    public HighTieredScore<TValue> ChangeBronze(TValue bronze)
    {
        return new HighTieredScore<TValue>(
            bronze,
            Silver,
            Gold);
    }

    /// <summary>
    /// Converts this score definition to a
    /// <see cref="LowTieredScore{TValue}"/>.
    /// </summary>
    /// <returns>
    /// A low-tiered score using the same threshold values.
    /// </returns>
    public LowTieredScore<TValue> ToLow()
    {
        return new LowTieredScore<TValue>(
            Bronze,
            Silver,
            Gold);
    }

    /// <summary>
    /// Determines the tier achieved by the specified value.
    /// </summary>
    /// <param name="value">
    /// The value to evaluate.
    /// </param>
    /// <returns>
    /// The highest tier reached by the specified value.
    /// </returns>
    public TierResult GetResult(TValue value)
    {
        return value.CompareTo(Gold) >= 0
            ? TierResult.Gold
            : value.CompareTo(Silver) >= 0
                ? TierResult.Silver
                : value.CompareTo(Bronze) >= 0
                    ? TierResult.Bronze
                    : TierResult.None;
    }

    /// <summary>
    /// Determines whether two tiered score definitions are equal.
    /// </summary>
    public static bool operator ==(
        HighTieredScore<TValue> a,
        HighTieredScore<TValue> b)
    {
        return a.Bronze.CompareTo(b.Bronze) == 0 &&
               a.Silver.CompareTo(b.Silver) == 0 &&
               a.Gold.CompareTo(b.Gold) == 0;
    }

    /// <summary>
    /// Determines whether two tiered score definitions are not equal.
    /// </summary>
    public static bool operator !=(
        HighTieredScore<TValue> a,
        HighTieredScore<TValue> b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified value exceeds the bronze threshold.
    /// </summary>
    public static bool operator <(
        HighTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Bronze) > 0;
    }

    /// <summary>
    /// Determines whether the specified value is below the gold threshold.
    /// </summary>
    public static bool operator >(
        HighTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Gold) < 0;
    }

    /// <summary>
    /// Determines whether the specified value reaches the gold threshold.
    /// </summary>
    public static bool operator <=(
        HighTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Gold) >= 0;
    }

    /// <summary>
    /// Determines whether the specified value is at or below the bronze threshold.
    /// </summary>
    public static bool operator >=(
        HighTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Bronze) <= 0;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is HighTieredScore<TValue> other &&
               this == other;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Bronze,
            Silver,
            Gold);
    }

    Type ITieredScore.Type => typeof(TValue);

    object ITieredScore.Bronze => Bronze;

    object ITieredScore.Silver => Silver;

    object ITieredScore.Gold => Gold;

    ITieredScore ITieredScore.WithValues(
        object bronze,
        object silver,
        object gold)
    {
        if (bronze is not TValue b)
        {
            throw new ArgumentException(
                $"Bronze must be of type {typeof(TValue).Name}.",
                nameof(bronze));
        }

        if (silver is not TValue s)
        {
            throw new ArgumentException(
                $"Silver must be of type {typeof(TValue).Name}.",
                nameof(silver));
        }

        if (gold is not TValue g)
        {
            throw new ArgumentException(
                $"Gold must be of type {typeof(TValue).Name}.",
                nameof(gold));
        }

        return new HighTieredScore<TValue>(
            b,
            s,
            g);
    }

    TierResult ITieredScore.GetResult(object value)
    {
        if (value is not TValue typedValue)
        {
            throw new ArgumentException(
                $"Value must be of type {typeof(TValue).Name}.",
                nameof(value));
        }

        return GetResult(typedValue);
    }

    internal static TValue Min(TValue a, TValue b)
    {
        return a.CompareTo(b) <= 0 ? a : b;
    }

    internal static TValue Max(TValue a, TValue b)
    {
        return a.CompareTo(b) >= 0 ? a : b;
    }

    internal static TValue Clamp(
        TValue value,
        TValue min,
        TValue max)
    {
        if (value.CompareTo(min) < 0)
            return min;

        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }
}
using System;

namespace Sachssoft.Engine.Gameplay;

/// <summary>
/// Represents tiered score thresholds where lower values produce better results.
/// </summary>
/// <typeparam name="TValue">
/// The value type used for score thresholds.
/// </typeparam>
/// <remarks>
/// The thresholds are normalized so that
/// <c>Bronze &gt;= Silver &gt;= Gold</c>, preserving all three supplied values.
/// A value must be less than or equal to a threshold to achieve the corresponding tier.
/// </remarks>
public readonly struct LowTieredScore<TValue> : ITieredScore
    where TValue : struct, IComparable<TValue>
{
    /// <summary>
    /// The threshold for achieving the bronze tier.
    /// </summary>
    public readonly TValue Bronze;

    /// <summary>
    /// The threshold for achieving the silver tier.
    /// </summary>
    public readonly TValue Silver;

    /// <summary>
    /// The threshold for achieving the gold tier.
    /// </summary>
    public readonly TValue Gold;

    /// <summary>
    /// Initializes a new instance of the <see cref="LowTieredScore{TValue}"/> struct.
    /// </summary>
    /// <param name="bronze">The bronze threshold.</param>
    /// <param name="silver">The silver threshold.</param>
    /// <param name="gold">The gold threshold.</param>
    public LowTieredScore(
        TValue bronze,
        TValue silver,
        TValue gold)
    {
        if (bronze.CompareTo(silver) < 0)
            (bronze, silver) = (silver, bronze);
        if (silver.CompareTo(gold) < 0)
            (silver, gold) = (gold, silver);
        if (bronze.CompareTo(silver) < 0)
            (bronze, silver) = (silver, bronze);

        Bronze = bronze;
        Silver = silver;
        Gold = gold;
    }

    /// <summary>
    /// Creates a new score definition with the specified gold threshold.
    /// </summary>
    /// <param name="gold">The gold threshold to preserve exactly.</param>
    /// <returns>A score with the requested gold value and neighboring limits raised only as needed.</returns>
    /// <remarks>Bronze and silver are raised when required to retain Bronze &gt;= Silver &gt;= Gold.</remarks>
    public LowTieredScore<TValue> ChangeGold(TValue gold)
    {
        return new LowTieredScore<TValue>(
            Max(Bronze, gold),
            Max(Silver, gold),
            gold);
    }

    /// <summary>
    /// Creates a new score definition with the specified silver threshold.
    /// </summary>
    /// <param name="silver">The silver threshold to preserve exactly.</param>
    /// <returns>A score with the requested silver value and neighboring limits adjusted only as needed.</returns>
    /// <remarks>Bronze may increase and gold may decrease to retain Bronze &gt;= Silver &gt;= Gold.</remarks>
    public LowTieredScore<TValue> ChangeSilver(TValue silver)
    {
        return new LowTieredScore<TValue>(
            Max(Bronze, silver),
            silver,
            Min(Gold, silver));
    }

    /// <summary>
    /// Creates a new score definition with the specified bronze threshold.
    /// </summary>
    /// <param name="bronze">The bronze threshold to preserve exactly.</param>
    /// <returns>A score with the requested bronze value and neighboring limits lowered only as needed.</returns>
    /// <remarks>Silver and gold are lowered when required to retain Bronze &gt;= Silver &gt;= Gold.</remarks>
    public LowTieredScore<TValue> ChangeBronze(TValue bronze)
    {
        return new LowTieredScore<TValue>(
            bronze,
            Min(Silver, bronze),
            Min(Gold, bronze));
    }

    /// <summary>
    /// Converts this score definition to a
    /// <see cref="HighTieredScore{TValue}"/>.
    /// </summary>
    /// <returns>
    /// A high-tiered score using the same threshold values.
    /// </returns>
    public HighTieredScore<TValue> ToHigh()
    {
        return new HighTieredScore<TValue>(
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
    /// The highest tier achieved by the specified value.
    /// </returns>
    public TierResult GetResult(TValue value)
    {
        return value.CompareTo(Gold) <= 0
            ? TierResult.Gold
            : value.CompareTo(Silver) <= 0
                ? TierResult.Silver
                : value.CompareTo(Bronze) <= 0
                    ? TierResult.Bronze
                    : TierResult.None;
    }

    /// <summary>
    /// Determines whether two tiered score definitions are equal.
    /// </summary>
    public static bool operator ==(
        LowTieredScore<TValue> a,
        LowTieredScore<TValue> b)
    {
        return a.Bronze.CompareTo(b.Bronze) == 0 &&
               a.Silver.CompareTo(b.Silver) == 0 &&
               a.Gold.CompareTo(b.Gold) == 0;
    }

    /// <summary>
    /// Determines whether two tiered score definitions are not equal.
    /// </summary>
    public static bool operator !=(
        LowTieredScore<TValue> a,
        LowTieredScore<TValue> b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Determines whether the specified value is below the bronze threshold.
    /// </summary>
    public static bool operator <(
        LowTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Bronze) < 0;
    }

    /// <summary>
    /// Determines whether the specified value exceeds the gold threshold.
    /// </summary>
    public static bool operator >(
        LowTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Gold) > 0;
    }

    /// <summary>
    /// Determines whether the specified value reaches the gold threshold.
    /// </summary>
    public static bool operator <=(
        LowTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Gold) <= 0;
    }

    /// <summary>
    /// Determines whether the specified value is at or above the bronze threshold.
    /// </summary>
    public static bool operator >=(
        LowTieredScore<TValue> score,
        TValue value)
    {
        return value.CompareTo(score.Bronze) >= 0;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is LowTieredScore<TValue> other &&
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

        return new LowTieredScore<TValue>(
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
